using Card_Addiction_POS_System.Functions.Models;
using Card_Addiction_POS_System.Data.SQLServer;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Card_Addiction_POS_System.Functions.Inventory.AddNew
{
    /// <summary>
    /// ReleaseCheck
    /// - Fetches set/group data from TCGCSV and diffs it against the DB.
    /// - Uses the application's SqlConnectionFactory + a password provider delegate
    ///   so it can open connections for the current signed-in user.
    /// - Also provides helpers to populate dbo.NewCardgameSetTemp and to read its contents.
    /// </summary>
    public sealed class ReleaseCheck
    {
        private readonly SqlConnectionFactory _connectionFactory;
        private readonly Func<Task<string>> _getPasswordAsync;
        private readonly HttpClient _http;

        /// <summary>
        /// Constructor.
        /// - Accepts the app's SqlConnectionFactory and a delegate that returns the current user's password asynchronously.
        /// - Optionally accept an HttpClient (useful for unit testing / DI). If null, a new HttpClient is created.
        /// </summary>
        public ReleaseCheck(SqlConnectionFactory connectionFactory, Func<Task<string>> getPasswordAsync, HttpClient? http = null)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            _getPasswordAsync = getPasswordAsync ?? throw new ArgumentNullException(nameof(getPasswordAsync));

            // Allow injection of HttpClient for testability/DI. Keep a conservative timeout.
            _http = http ?? new HttpClient();
            _http.Timeout = TimeSpan.FromSeconds(30);
        }

        /// <summary>
        /// Main entry point used elsewhere: gets missing sets (keeps prior behaviour).
        /// </summary>
        public async Task<List<AddNewSet>> GetMissingSetsAsync(int cardGameId, CancellationToken ct = default)
        {
            List<GroupResult> allGroups = await FetchGroupsAsync(cardGameId, ct).ConfigureAwait(false);
            HashSet<int> existingSetIds = await FetchExistingSetIdsAsync(cardGameId, ct).ConfigureAwait(false);

            var missing = new List<AddNewSet>();

            foreach (var g in allGroups)
            {
                if (existingSetIds.Contains(g.GroupId))
                    continue;

                missing.Add(new AddNewSet
                {
                    CardGameId = g.CategoryId,
                    SetId = g.GroupId,
                    SetName = g.Name,
                    Abbreviation = string.IsNullOrWhiteSpace(g.Abbreviation) ? null : g.Abbreviation,
                    SetDate = g.PublishedOn?.Date,
                    IsSupplemental = g.IsSupplemental
                });
            }

            // Optional: sort newest first (nulls sorted last)
            missing.Sort((a, b) => Nullable.Compare(b.SetDate, a.SetDate));

            return missing;
        }

        // ---- HTTP FETCH ----

        /// <summary>
        /// FetchGroupsAsync
        /// - Calls TCGCSV's groups endpoint for the given category id and deserializes JSON into domain models.
        /// </summary>
        private async Task<List<GroupResult>> FetchGroupsAsync(int cardGameId, CancellationToken ct)
        {
            string url = BuildGroupsUrl(cardGameId);

            using var resp = await _http.GetAsync(url, ct).ConfigureAwait(false);
            resp.EnsureSuccessStatusCode();

            string json = await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(false);

            var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            TcgCsvGroupsResponse? parsed = JsonSerializer.Deserialize<TcgCsvGroupsResponse>(json, opts);

            if (parsed == null || parsed.Results == null)
                return new List<GroupResult>();

            if (!parsed.Success)
                return new List<GroupResult>();

            return parsed.Results.Where(r => r.CategoryId == cardGameId).ToList();
        }

        /// <summary>
        /// BuildGroupsUrl
        /// - Centralized place to change the TCGCSV groups endpoint if needed.
        /// - Use the 'groups' JSON endpoint (not the CSV link).
        /// </summary>
        private string BuildGroupsUrl(int cardGameId)
        {
            // The site uses multiple URL shapes; prefer the "tcgplayer" route used on the site UI (matches screenshots).
            // If you prefer categories/{id}/groups change this string accordingly.
            return $"https://tcgcsv.com/tcgplayer/{cardGameId}/groups";
        }

        // ---- DB HELPERS ----

        /// <summary>
        /// PopulateNewCardgameSetTempFromTcgCsvAsync
        /// - Fetches all groups for the requested game, inserts them into dbo.NewCardgameSetTemp
        ///   (only groups whose publishedOn is not more than 1 month ahead of today's date).
        /// - After inserting, removes any temp rows that already exist in the game's Set table
        ///   (table name is derived from SelectedCardGameLogic.DatabaseName + "Set").
        /// - Operates under a single connection/transaction for consistency.
        /// 
        /// Note: dateStarted is set to the server datetime (SYSUTCDATETIME()) on insert to satisfy NOT NULL constraints.
        /// 
        /// Change: TRUNCATE/DELETE is executed before starting the insert transaction (separate autocommit statement).
        /// This avoids using a transaction object that can become "completed" by certain operations and then reused.
        /// Returns: number of rows present in NewCardgameSetTemp for this cardGameId after processing.
        /// 
        /// TempId assignment: explicitly sets tempId starting from 1, ordered by setDate DESC then setName.
        /// </summary>
        /// <param name="game">SelectedCardGameLogic describing the chosen game (id + DatabaseName).</param>
        public async Task<int> PopulateNewCardgameSetTempFromTcgCsvAsync(SelectedCardGameLogic game, CancellationToken ct = default)
        {
            if (game == null) throw new ArgumentNullException(nameof(game));

            // 1) Fetch groups from TCGCSV
            List<GroupResult> allGroups = await FetchGroupsAsync(game.CardGameId, ct).ConfigureAwait(false);

            // Threshold: publishedOn must not be more than 1 month ahead of now.
            var maxAllowed = DateTime.UtcNow.AddMonths(1);

            // Keep only groups within threshold (or with no published date)
            var toInsert = allGroups.Where(g => !g.PublishedOn.HasValue || g.PublishedOn.Value <= maxAllowed).ToList();

            // 2) Open a DB connection for current user
            var password = await _getPasswordAsync().ConfigureAwait(false);
            using var conn = _connectionFactory.CreateForCurrentUser(password);
            password = string.Empty;

            await conn.OpenAsync(ct).ConfigureAwait(false);

            // Ensure the temp tables are empty before we begin the insert transaction.
            // Use DELETE (no TRUNCATE) to avoid requiring ALTER/CONTROL permissions.
            try
            {
                using (var deleteSetsCmd = new SqlCommand("DELETE FROM dbo.NewCardgameSetTemp;", conn))
                {
                    await deleteSetsCmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
                }

                using (var deleteInventoryCmd = new SqlCommand("DELETE FROM dbo.NewTempCardgameInventory;", conn))
                {
                    await deleteInventoryCmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
                }
            }
            catch
            {
                // If clearing the temp tables fails for any reason, rethrow so caller can show the error.
                throw;
            }

            // If there's nothing to insert, we're done (tables were cleared).
            if (toInsert.Count == 0)
            {
                // count is zero
                return 0;
            }

            // 3) Begin a transaction for the insert + subsequent cleanup steps
            using var tx = conn.BeginTransaction();
            try
            {
                // Sort groups by setDate DESC, then setName (same order used for display)
                var sortedGroups = toInsert.OrderByDescending(g => g.PublishedOn ?? DateTime.MinValue)
                                           .ThenBy(g => g.Name ?? string.Empty)
                                           .ToList();

                // Enable IDENTITY_INSERT so we can set tempId explicitly
                using (var enableIdentityCmd = new SqlCommand("SET IDENTITY_INSERT dbo.NewCardgameSetTemp ON;", conn, tx))
                {
                    await enableIdentityCmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
                }

                // Insert each group into NewCardgameSetTemp with explicit tempId starting from 1
                // NOTE: includeInBatch column is set to 1 (true) by default on insert
                const string insertSql = @"
INSERT INTO dbo.NewCardgameSetTemp
    (tempId, setId, cardGameId, setName, setDesc, abbreviation, setDate, dateStarted, dateUploaded, approved, hasIssues, issueNotes, includeInBatch)
VALUES
    (@tempId, @setId, @cardGameId, @setName, @setDesc, @abbreviation, @setDate, SYSUTCDATETIME(), SYSUTCDATETIME(), 0, 0, NULL, 1);";

                using var insertCmd = new SqlCommand(insertSql, conn, tx);

                insertCmd.Parameters.Add("@tempId", SqlDbType.Int);
                insertCmd.Parameters.Add("@setId", SqlDbType.Int);
                insertCmd.Parameters.Add("@cardGameId", SqlDbType.Int);
                insertCmd.Parameters.Add("@setName", SqlDbType.NVarChar, 255);
                insertCmd.Parameters.Add("@setDesc", SqlDbType.NVarChar, 255);
                insertCmd.Parameters.Add("@abbreviation", SqlDbType.NVarChar, 50);
                insertCmd.Parameters.Add("@setDate", SqlDbType.Date);

                int currentTempId = 1;
                foreach (var g in sortedGroups)
                {
                    insertCmd.Parameters["@tempId"].Value = currentTempId;
                    insertCmd.Parameters["@setId"].Value = g.GroupId;
                    insertCmd.Parameters["@cardGameId"].Value = g.CategoryId;
                    insertCmd.Parameters["@setName"].Value = g.Name ?? string.Empty;
                    insertCmd.Parameters["@setDesc"].Value = DBNull.Value;
                    insertCmd.Parameters["@abbreviation"].Value = (object?)(string.IsNullOrWhiteSpace(g.Abbreviation) ? DBNull.Value : g.Abbreviation) ?? DBNull.Value;
                    insertCmd.Parameters["@setDate"].Value = g.PublishedOn.HasValue ? (object)g.PublishedOn.Value.Date : DBNull.Value;

                    // Execute insert; ignore unique violations here (if temp table has unique constraint) by catching and continuing.
                    try
                    {
                        await insertCmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
                        currentTempId++;
                    }
                    catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601) // primary key / unique violation
                    {
                        // row already present in temp; ignore and continue (don't increment tempId)
                        continue;
                    }
                }

                // Disable IDENTITY_INSERT after explicit insert
                using (var disableIdentityCmd = new SqlCommand("SET IDENTITY_INSERT dbo.NewCardgameSetTemp OFF;", conn, tx))
                {
                    await disableIdentityCmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
                }

                // 4) Remove any temp rows that match existing sets in the selected game's Set table
                //    Table name is derived from DatabaseName + "Set" (e.g. "MagicSet")
                var setTable = $"{game.DatabaseName}Set";

                // Validate table name is safe (only letters/numbers/underscore to reduce SQL injection risk)
                if (!System.Text.RegularExpressions.Regex.IsMatch(setTable, @"^[A-Za-z0-9_]+$"))
                    throw new InvalidOperationException("Invalid target set table name.");

                // Read existing setIds from that table
                var existingSetIds = new HashSet<int>();
                var readSql = $@"SELECT setId FROM dbo.[{setTable}] WHERE cardGameId = @cardGameId;";

                using (var readCmd = new SqlCommand(readSql, conn, tx))
                {
                    readCmd.Parameters.Add("@cardGameId", SqlDbType.Int).Value = game.CardGameId;

                    using var reader = await readCmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
                    while (await reader.ReadAsync(ct).ConfigureAwait(false))
                    {
                        if (!reader.IsDBNull(0))
                            existingSetIds.Add(reader.GetInt32(0));
                    }
                }

                if (existingSetIds.Count > 0)
                {
                    // Delete matching rows from NewCardgameSetTemp
                    const string deleteSql = @"DELETE FROM dbo.NewCardgameSetTemp WHERE setId = @setId AND cardGameId = @cardGameId;";

                    using var deleteCmd = new SqlCommand(deleteSql, conn, tx);
                    deleteCmd.Parameters.Add("@setId", SqlDbType.Int);
                    deleteCmd.Parameters.Add("@cardGameId", SqlDbType.Int).Value = game.CardGameId;

                    foreach (var sId in existingSetIds)
                    {
                        deleteCmd.Parameters["@setId"].Value = sId;
                        await deleteCmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
                    }

                    // After deleting existing sets, renumber tempIds to be sequential starting from 1
                    await RenumberTempIdsAsync(conn, tx, game.CardGameId, ct).ConfigureAwait(false);
                }

                tx.Commit();
            }
            catch
            {
                try { tx.Rollback(); } catch { }
                throw;
            }

            // Count rows in temp for this game and return
            const string countSql = @"SELECT COUNT(*) FROM dbo.NewCardgameSetTemp WHERE cardGameId = @cardGameId;";
            using var countCmd = new SqlCommand(countSql, conn);
            countCmd.Parameters.Add("@cardGameId", SqlDbType.Int).Value = game.CardGameId;

            var countObj = await countCmd.ExecuteScalarAsync(ct).ConfigureAwait(false);
            if (countObj == null || countObj == DBNull.Value) return 0;
            return Convert.ToInt32(countObj);
        }

        /// <summary>
        /// Renumbers tempId values sequentially starting from 1 for the given cardGameId,
        /// ordered by setDate DESC then setName. Rows with includeInBatch=0 are pushed to the end.
        /// </summary>
        private async Task RenumberTempIdsAsync(SqlConnection conn, SqlTransaction tx, int cardGameId, CancellationToken ct)
        {
            // Read all rows for this cardGameId, ordered by includeInBatch DESC (true first), then setDate DESC, then setName
            const string readSql = @"
SELECT tempId, setId, cardGameId, setName, setDesc, abbreviation, setDate, dateStarted, dateUploaded, includeInBatch, approved, hasIssues, issueNotes
FROM dbo.NewCardgameSetTemp
WHERE cardGameId = @cardGameId
ORDER BY includeInBatch DESC, setDate DESC, setName;";

            var rows = new List<(int oldTempId, int setId, int cardGameId, string setName, string? setDesc, string? abbreviation, DateTime? setDate, DateTime? dateStarted, DateTime? dateUploaded, bool includeInBatch, bool approved, bool hasIssues, string? issueNotes)>();

            using (var readCmd = new SqlCommand(readSql, conn, tx))
            {
                readCmd.Parameters.Add("@cardGameId", SqlDbType.Int).Value = cardGameId;

                using var reader = await readCmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
                while (await reader.ReadAsync(ct).ConfigureAwait(false))
                {
                    rows.Add((
                        reader.GetInt32(0),
                        reader.GetInt32(1),
                        reader.GetInt32(2),
                        reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                        reader.IsDBNull(4) ? null : reader.GetString(4),
                        reader.IsDBNull(5) ? null : reader.GetString(5),
                        reader.IsDBNull(6) ? (DateTime?)null : reader.GetDateTime(6),
                        reader.IsDBNull(7) ? (DateTime?)null : reader.GetDateTime(7),
                        reader.IsDBNull(8) ? (DateTime?)null : reader.GetDateTime(8),
                        !reader.IsDBNull(9) && reader.GetBoolean(9),
                        !reader.IsDBNull(10) && reader.GetBoolean(10),
                        !reader.IsDBNull(11) && reader.GetBoolean(11),
                        reader.IsDBNull(12) ? null : reader.GetString(12)
                    ));
                }
            }

            if (rows.Count == 0) return;

            // Delete all rows for this cardGameId
            const string deleteSql = @"DELETE FROM dbo.NewCardgameSetTemp WHERE cardGameId = @cardGameId;";
            using (var deleteCmd = new SqlCommand(deleteSql, conn, tx))
            {
                deleteCmd.Parameters.Add("@cardGameId", SqlDbType.Int).Value = cardGameId;
                await deleteCmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            }

            // Enable IDENTITY_INSERT
            using (var enableCmd = new SqlCommand("SET IDENTITY_INSERT dbo.NewCardgameSetTemp ON;", conn, tx))
            {
                await enableCmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            }

            // Re-insert with new sequential tempIds
            const string insertSql = @"
INSERT INTO dbo.NewCardgameSetTemp
    (tempId, setId, cardGameId, setName, setDesc, abbreviation, setDate, dateStarted, dateUploaded, includeInBatch, approved, hasIssues, issueNotes)
VALUES
    (@tempId, @setId, @cardGameId, @setName, @setDesc, @abbreviation, @setDate, @dateStarted, @dateUploaded, @includeInBatch, @approved, @hasIssues, @issueNotes);";

            using var insertCmd = new SqlCommand(insertSql, conn, tx);
            insertCmd.Parameters.Add("@tempId", SqlDbType.Int);
            insertCmd.Parameters.Add("@setId", SqlDbType.Int);
            insertCmd.Parameters.Add("@cardGameId", SqlDbType.Int);
            insertCmd.Parameters.Add("@setName", SqlDbType.NVarChar, 255);
            insertCmd.Parameters.Add("@setDesc", SqlDbType.NVarChar, 255);
            insertCmd.Parameters.Add("@abbreviation", SqlDbType.NVarChar, 50);
            insertCmd.Parameters.Add("@setDate", SqlDbType.Date);
            insertCmd.Parameters.Add("@dateStarted", SqlDbType.DateTime);
            insertCmd.Parameters.Add("@dateUploaded", SqlDbType.DateTime);
            insertCmd.Parameters.Add("@includeInBatch", SqlDbType.Bit);
            insertCmd.Parameters.Add("@approved", SqlDbType.Bit);
            insertCmd.Parameters.Add("@hasIssues", SqlDbType.Bit);
            insertCmd.Parameters.Add("@issueNotes", SqlDbType.NVarChar, -1);

            int newTempId = 1;
            foreach (var row in rows)
            {
                insertCmd.Parameters["@tempId"].Value = newTempId;
                insertCmd.Parameters["@setId"].Value = row.setId;
                insertCmd.Parameters["@cardGameId"].Value = row.cardGameId;
                insertCmd.Parameters["@setName"].Value = row.setName;
                insertCmd.Parameters["@setDesc"].Value = (object?)row.setDesc ?? DBNull.Value;
                insertCmd.Parameters["@abbreviation"].Value = (object?)row.abbreviation ?? DBNull.Value;
                insertCmd.Parameters["@setDate"].Value = row.setDate.HasValue ? (object)row.setDate.Value : DBNull.Value;
                insertCmd.Parameters["@dateStarted"].Value = row.dateStarted.HasValue ? (object)row.dateStarted.Value : DBNull.Value;
                insertCmd.Parameters["@dateUploaded"].Value = row.dateUploaded.HasValue ? (object)row.dateUploaded.Value : DBNull.Value;
                insertCmd.Parameters["@includeInBatch"].Value = row.includeInBatch;
                insertCmd.Parameters["@approved"].Value = row.approved;
                insertCmd.Parameters["@hasIssues"].Value = row.hasIssues;
                insertCmd.Parameters["@issueNotes"].Value = (object?)row.issueNotes ?? DBNull.Value;

                await insertCmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
                newTempId++;
            }

            // Disable IDENTITY_INSERT
            using (var disableCmd = new SqlCommand("SET IDENTITY_INSERT dbo.NewCardgameSetTemp OFF;", conn, tx))
            {
                await disableCmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Updates the includeInBatch flag for a specific tempId and renumbers all tempIds sequentially.
        /// Rows with includeInBatch=true are numbered 1..N (sorted by setDate DESC, setName), 
        /// rows with includeInBatch=false follow after.
        /// </summary>
        public async Task UpdateIncludeInBatchAsync(int cardGameId, int tempId, bool includeInBatch, CancellationToken ct = default)
        {
            var password = await _getPasswordAsync().ConfigureAwait(false);
            using var conn = _connectionFactory.CreateForCurrentUser(password);
            password = string.Empty;

            await conn.OpenAsync(ct).ConfigureAwait(false);

            using var tx = conn.BeginTransaction();
            try
            {
                // Update the includeInBatch flag for the specified tempId
                const string updateSql = @"UPDATE dbo.NewCardgameSetTemp SET includeInBatch = @includeInBatch WHERE tempId = @tempId AND cardGameId = @cardGameId;";
                using (var updateCmd = new SqlCommand(updateSql, conn, tx))
                {
                    updateCmd.Parameters.Add("@includeInBatch", SqlDbType.Bit).Value = includeInBatch;
                    updateCmd.Parameters.Add("@tempId", SqlDbType.Int).Value = tempId;
                    updateCmd.Parameters.Add("@cardGameId", SqlDbType.Int).Value = cardGameId;
                    await updateCmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
                }

                // Renumber all tempIds for this cardGameId
                await RenumberTempIdsAsync(conn, tx, cardGameId, ct).ConfigureAwait(false);

                tx.Commit();
            }
            catch
            {
                try { tx.Rollback(); } catch { }
                throw;
            }
        }

        /// <summary>
        /// Returns the current rows in dbo.NewCardgameSetTemp for the given cardGameId.
        /// Useful for binding to UI grids.
        /// </summary>
        public async Task<List<NewCardgameSetTempRow>> GetNewCardgameSetTempAsync(int cardGameId, CancellationToken ct = default)
        {
            var results = new List<NewCardgameSetTempRow>();

            var password = await _getPasswordAsync().ConfigureAwait(false);
            using var conn = _connectionFactory.CreateForCurrentUser(password);
            password = string.Empty;

            await conn.OpenAsync(ct).ConfigureAwait(false);

            const string sql = @"
SELECT tempId, setId, cardGameId, setName, setDesc, abbreviation, setDate, dateStarted, dateUploaded, includeInBatch, approved, hasIssues, issueNotes
FROM dbo.NewCardgameSetTemp
WHERE cardGameId = @cardGameId
ORDER BY tempId;";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@cardGameId", SqlDbType.Int).Value = cardGameId;

            using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);

            var ordTempId = reader.GetOrdinal("tempId");
            var ordSetId = reader.GetOrdinal("setId");
            var ordCardGameId = reader.GetOrdinal("cardGameId");
            var ordSetName = reader.GetOrdinal("setName");
            var ordSetDesc = reader.GetOrdinal("setDesc");
            var ordAbbreviation = reader.GetOrdinal("abbreviation");
            var ordSetDate = reader.GetOrdinal("setDate");
            var ordDateStarted = reader.GetOrdinal("dateStarted");
            var ordDateUploaded = reader.GetOrdinal("dateUploaded");
            var ordIncludeInBatch = reader.GetOrdinal("includeInBatch");
            var ordApproved = reader.GetOrdinal("approved");
            var ordHasIssues = reader.GetOrdinal("hasIssues");
            var ordIssueNotes = reader.GetOrdinal("issueNotes");

            while (await reader.ReadAsync(ct).ConfigureAwait(false))
            {
                results.Add(new NewCardgameSetTempRow
                {
                    TempId = reader.IsDBNull(ordTempId) ? 0 : reader.GetInt32(ordTempId),
                    SetId = reader.IsDBNull(ordSetId) ? 0 : reader.GetInt32(ordSetId),
                    CardGameId = reader.IsDBNull(ordCardGameId) ? 0 : reader.GetInt32(ordCardGameId),
                    SetName = reader.IsDBNull(ordSetName) ? string.Empty : reader.GetString(ordSetName),
                    SetDesc = reader.IsDBNull(ordSetDesc) ? null : reader.GetString(ordSetDesc),
                    Abbreviation = reader.IsDBNull(ordAbbreviation) ? null : reader.GetString(ordAbbreviation),
                    SetDate = reader.IsDBNull(ordSetDate) ? (DateTime?)null : reader.GetDateTime(ordSetDate),
                    DateStarted = reader.IsDBNull(ordDateStarted) ? (DateTime?)null : reader.GetDateTime(ordDateStarted),
                    DateUploaded = reader.IsDBNull(ordDateUploaded) ? (DateTime?)null : reader.GetDateTime(ordDateUploaded),
                    IncludeInBatch = !reader.IsDBNull(ordIncludeInBatch) && reader.GetBoolean(ordIncludeInBatch),
                    Approved = !reader.IsDBNull(ordApproved) && reader.GetBoolean(ordApproved),
                    HasIssues = !reader.IsDBNull(ordHasIssues) && reader.GetBoolean(ordHasIssues),
                    IssueNotes = reader.IsDBNull(ordIssueNotes) ? null : reader.GetString(ordIssueNotes)
                });
            }

            return results;
        }

        // ---- Existing DB helper reused from earlier code ----

        private async Task<HashSet<int>> FetchExistingSetIdsAsync(int cardGameId, CancellationToken ct)
        {
            var setIds = new HashSet<int>();

            const string sql = @"
            SELECT setId
            FROM dbo._Set
            WHERE cardGameId = @cardGameId;
        ";

            var password = await _getPasswordAsync().ConfigureAwait(false);
            using var conn = _connectionFactory.CreateForCurrentUser(password);
            password = string.Empty;

            await conn.OpenAsync(ct).ConfigureAwait(false);

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@cardGameId", SqlDbType.Int).Value = cardGameId;

            using var reader = await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection, ct).ConfigureAwait(false);
            while (await reader.ReadAsync(ct).ConfigureAwait(false))
            {
                if (!reader.IsDBNull(0))
                    setIds.Add(reader.GetInt32(0));
            }

            return setIds;
        }

        // Simple DTO for rows read from NewCardgameSetTemp
        public sealed class NewCardgameSetTempRow
        {
            public int TempId { get; init; }
            public int SetId { get; init; }
            public int CardGameId { get; init; }
            public string SetName { get; init; } = string.Empty;
            public string? SetDesc { get; init; }
            public string? Abbreviation { get; init; }
            public DateTime? SetDate { get; init; }
            public DateTime? DateStarted { get; init; }
            public DateTime? DateUploaded { get; init; }
            public bool IncludeInBatch { get; init; }
            public bool Approved { get; init; }
            public bool HasIssues { get; init; }
            public string? IssueNotes { get; init; }
        }
    }
}
