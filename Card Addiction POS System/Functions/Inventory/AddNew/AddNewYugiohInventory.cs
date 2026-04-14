using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Card_Addiction_POS_System.Data.SQLServer;
using Microsoft.Data.SqlClient;

namespace Card_Addiction_POS_System.Functions.Inventory.AddNew
{
    /// <summary>
    /// Scans TCGCSV products for the selected Yugioh sets and loads them into dbo.NewTempCardgameInventory,
    /// then returns the inserted rows for display.
    /// </summary>
    internal sealed class AddNewYugiohInventory
    {
        private readonly SqlConnectionFactory _connectionFactory;
        private readonly Func<Task<string>> _getPasswordAsync;
        private readonly HttpClient _http;

        /// <summary>
        /// Accepts the app's SqlConnectionFactory + password delegate; allows injecting HttpClient for tests.
        /// </summary>
        public AddNewYugiohInventory(SqlConnectionFactory connectionFactory, Func<Task<string>> getPasswordAsync, HttpClient? http = null)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            _getPasswordAsync = getPasswordAsync ?? throw new ArgumentNullException(nameof(getPasswordAsync));
            _http = http ?? new HttpClient { Timeout = TimeSpan.FromSeconds(30) };

            // Some endpoints block unknown clients; present a common browser UA.
            if (!_http.DefaultRequestHeaders.UserAgent.Any())
            {
                _http.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/122.0.0.0 Safari/537.36");
            }
        }

        // ---- MAIN ORCHESTRATION: FETCH, INSERT, RETURN ----
        // Pull selected sets, fetch their products, populate temp inventory, normalize, then return inserted rows.

        public async Task<List<NewTempCardgameInventoryRow>> RunAsync(int cardGameId, int maxBatch, CancellationToken ct = default)
        {
            if (cardGameId <= 0) throw new ArgumentOutOfRangeException(nameof(cardGameId));
            if (maxBatch <= 0) return new List<NewTempCardgameInventoryRow>();

            // Open connection per current user
            var password = await _getPasswordAsync().ConfigureAwait(false);
            using var conn = _connectionFactory.CreateForCurrentUser(password);
            password = string.Empty;

            await conn.OpenAsync(ct).ConfigureAwait(false);

            // 1) Get the selected sets (up to current batch) that are marked includeInBatch = 1
            var sets = new List<(int tempId, int setId, string setName)>();
            const string selectSetsSql = @"
SELECT TOP (@maxBatch) tempId, setId, setName
FROM dbo.NewCardgameSetTemp
WHERE cardGameId = @cardGameId AND includeInBatch = 1
ORDER BY tempId;";

            using (var cmd = new SqlCommand(selectSetsSql, conn))
            {
                cmd.Parameters.Add("@maxBatch", SqlDbType.Int).Value = maxBatch;
                cmd.Parameters.Add("@cardGameId", SqlDbType.Int).Value = cardGameId;

                using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
                while (await reader.ReadAsync(ct).ConfigureAwait(false))
                {
                    sets.Add((
                        reader.GetInt32(0),
                        reader.GetInt32(1),
                        reader.IsDBNull(2) ? string.Empty : reader.GetString(2)
                    ));
                }
            }

            if (sets.Count == 0)
            {
                return new List<NewTempCardgameInventoryRow>();
            }

            // 2) Process each set: fetch products and insert into NewTempCardgameInventory
            using var tx = conn.BeginTransaction();
            try
            {
                foreach (var set in sets)
                {
                    // Pull products for this set from TCGCSV
                    var products = await FetchProductsAsync(cardGameId, set.setId, ct).ConfigureAwait(false);

                    // Clear any previous rows for this batchPosition to avoid duplicates
                    using (var deleteCmd = new SqlCommand("DELETE FROM dbo.NewTempCardgameInventory WHERE batchPosition = @batchPosition;", conn, tx))
                    {
                        deleteCmd.Parameters.Add("@batchPosition", SqlDbType.Int).Value = set.tempId;
                        await deleteCmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
                    }

                    // Insert fresh rows for this set
                    const string insertSql = @"
INSERT INTO dbo.NewTempCardgameInventory
    (batchPosition, cardGameId, setId, cardId, cardName, abbreviation, rarity, foil, imageURL, mktPriceURL, mktPrice, amtInStock, approved, needsReview, issueNotes)
VALUES
    (@batchPosition, @cardGameId, @setId, @cardId, @cardName, @abbreviation, @rarity, @foil, @imageURL, @mktPriceURL, @mktPrice, @amtInStock, @approved, @needsReview, @issueNotes);";

                    using var insertCmd = new SqlCommand(insertSql, conn, tx);
                    insertCmd.Parameters.Add("@batchPosition", SqlDbType.Int);
                    insertCmd.Parameters.Add("@cardGameId", SqlDbType.Int);
                    insertCmd.Parameters.Add("@setId", SqlDbType.Int);
                    insertCmd.Parameters.Add("@cardId", SqlDbType.Int);
                    insertCmd.Parameters.Add("@cardName", SqlDbType.NVarChar, 255);
                    insertCmd.Parameters.Add("@abbreviation", SqlDbType.NVarChar, 255);
                    insertCmd.Parameters.Add("@rarity", SqlDbType.NVarChar, 100);
                    insertCmd.Parameters.Add("@foil", SqlDbType.NVarChar, 50);
                    insertCmd.Parameters.Add("@imageURL", SqlDbType.NVarChar, 500);
                    insertCmd.Parameters.Add("@mktPriceURL", SqlDbType.NVarChar, 500);
                    insertCmd.Parameters.Add("@mktPrice", SqlDbType.Decimal).Precision = 18;
                    insertCmd.Parameters["@mktPrice"].Scale = 4;
                    insertCmd.Parameters.Add("@amtInStock", SqlDbType.Int);
                    insertCmd.Parameters.Add("@approved", SqlDbType.Bit);
                    insertCmd.Parameters.Add("@needsReview", SqlDbType.Bit);
                    insertCmd.Parameters.Add("@issueNotes", SqlDbType.NVarChar, -1);

                    foreach (var p in products)
                    {
                        insertCmd.Parameters["@batchPosition"].Value = set.tempId;
                        insertCmd.Parameters["@cardGameId"].Value = cardGameId;
                        insertCmd.Parameters["@setId"].Value = set.setId;
                        insertCmd.Parameters["@cardId"].Value = p.ProductId;
                        insertCmd.Parameters["@cardName"].Value = p.Name ?? string.Empty;
                        insertCmd.Parameters["@abbreviation"].Value = (object?)GetNumber(p) ?? DBNull.Value;
                        insertCmd.Parameters["@rarity"].Value = (object?)GetRarity(p) ?? DBNull.Value;
                        insertCmd.Parameters["@foil"].Value = DBNull.Value; // Yugioh: leave null
                        insertCmd.Parameters["@imageURL"].Value = (object?)p.ImageUrl ?? DBNull.Value;
                        insertCmd.Parameters["@mktPriceURL"].Value = (object?)p.Url ?? DBNull.Value;
                        insertCmd.Parameters["@mktPrice"].Value = DBNull.Value; // filled later
                        insertCmd.Parameters["@amtInStock"].Value = DBNull.Value; // filled later
                        insertCmd.Parameters["@approved"].Value = DBNull.Value; // filled later
                        insertCmd.Parameters["@needsReview"].Value = false;
                        insertCmd.Parameters["@issueNotes"].Value = DBNull.Value;

                        await insertCmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
                    }
                }

                tx.Commit();
            }
            catch
            {
                try { tx.Rollback(); } catch { }
                throw;
            }

            // 3) Normalize sealed product rarity before returning
            var batchPositions = sets.Select(s => s.tempId).ToList();
            await UpdateSealedRarityAsync(conn, cardGameId, batchPositions, ct).ConfigureAwait(false);

            // 4) Return the rows just inserted for display
            return await LoadInventoryRowsAsync(conn, batchPositions, ct).ConfigureAwait(false);
        }

        public static IReadOnlyList<string> GetRequiredColumnsForValidation() =>
            new[] { "cardId", "cardGameId", "cardName", "setId", "rarity" };

        // ---- HTTP FETCH: PRODUCTS ----
        // Calls TCGCSV products endpoint for a given category/set and deserializes the JSON.

        private async Task<List<ProductResult>> FetchProductsAsync(int cardGameId, int setId, CancellationToken ct)
        {
            // TCGCSV expects categoryId/groupId in the path: tcgplayer/{categoryId}/{groupId}/products
            var url = $"https://tcgcsv.com/tcgplayer/{cardGameId}/{setId}/products";
            using var resp = await _http.GetAsync(url, ct).ConfigureAwait(false);
            resp.EnsureSuccessStatusCode();

            var json = await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
            var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var parsed = JsonSerializer.Deserialize<TcgCsvProductsResponse>(json, opts);

            if (parsed == null || !parsed.Success || parsed.Results == null)
            {
                return new List<ProductResult>();
            }

            return parsed.Results.Where(r => r.GroupId == setId).ToList();
        }

        // ---- PRODUCT FIELD HELPERS ----
        // Extracts specific extended data fields (rarity/number) from TCGCSV product payload.

        private static string? GetRarity(ProductResult p)
        {
            if (p.ExtendedData == null) return null;
            var rarity = p.ExtendedData.FirstOrDefault(ed =>
                ed.Name?.Equals("Rarity", StringComparison.OrdinalIgnoreCase) == true ||
                ed.DisplayName?.Equals("Rarity", StringComparison.OrdinalIgnoreCase) == true);
            return rarity?.Value;
        }

        private static string? GetNumber(ProductResult p)
        {
            if (p.ExtendedData == null) return null;
            var num = p.ExtendedData.FirstOrDefault(ed =>
                ed.Name?.Equals("Number", StringComparison.OrdinalIgnoreCase) == true ||
                ed.DisplayName?.Equals("Number", StringComparison.OrdinalIgnoreCase) == true);
            return num?.Value;
        }

        // ---- DB READBACK ----
        // Loads inserted temp inventory rows for the specified batch positions.

        private static async Task<List<NewTempCardgameInventoryRow>> LoadInventoryRowsAsync(SqlConnection conn, List<int> batchPositions, CancellationToken ct)
        {
            var results = new List<NewTempCardgameInventoryRow>();
            if (batchPositions.Count == 0)
            {
                return results;
            }

            // Build a parameterized IN clause: @bp0, @bp1, ...
            var paramNames = new List<string>(batchPositions.Count);
            using var cmd = new SqlCommand();
            cmd.Connection = conn;

            for (int i = 0; i < batchPositions.Count; i++)
            {
                string pname = $"@bp{i}";
                paramNames.Add(pname);
                cmd.Parameters.Add(pname, SqlDbType.Int).Value = batchPositions[i];
            }

            cmd.CommandText = $@"
SELECT tempId, batchPosition, cardGameId, setId, cardId, cardName, abbreviation, rarity, foil, imageURL, mktPriceURL, mktPrice, amtInStock, approved, needsReview, issueNotes, dateInserted
FROM dbo.NewTempCardgameInventory
WHERE batchPosition IN ({string.Join(",", paramNames)})
ORDER BY batchPosition, tempId;";

            using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
            while (await reader.ReadAsync(ct).ConfigureAwait(false))
            {
                results.Add(new NewTempCardgameInventoryRow
                {
                    TempId = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                    BatchPosition = reader.IsDBNull(1) ? 0 : reader.GetInt32(1),
                    CardGameId = reader.IsDBNull(2) ? 0 : reader.GetInt32(2),
                    SetId = reader.IsDBNull(3) ? 0 : reader.GetInt32(3),
                    CardId = reader.IsDBNull(4) ? 0 : reader.GetInt32(4),
                    ConditionId = 0, // default since column not present
                    CardName = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                    Abbreviation = reader.IsDBNull(6) ? null : reader.GetString(6),
                    Rarity = reader.IsDBNull(7) ? null : reader.GetString(7),
                    Foil = reader.IsDBNull(8) ? null : reader.GetString(8),
                    ImageUrl = reader.IsDBNull(9) ? null : reader.GetString(9),
                    MktPriceUrl = reader.IsDBNull(10) ? null : reader.GetString(10),
                    MktPrice = reader.IsDBNull(11) ? (decimal?)null : reader.GetDecimal(11),
                    AmtInStock = reader.IsDBNull(12) ? (int?)null : reader.GetInt32(12),
                    Approved = reader.IsDBNull(13) ? (bool?)null : reader.GetBoolean(13),
                    NeedsReview = reader.IsDBNull(14) ? false : reader.GetBoolean(14),
                    IssueNotes = reader.IsDBNull(15) ? null : reader.GetString(15),
                    DateInserted = reader.IsDBNull(16) ? (DateTime?)null : reader.GetDateTime(16)
                });
            }

            return results;
        }

        // ---- DATA NORMALIZATION ----
        // Sets rarity to "Sealed" for sealed products lacking a rarity value.

        private static async Task UpdateSealedRarityAsync(SqlConnection conn, int cardGameId, List<int> batchPositions, CancellationToken ct)
        {
            if (batchPositions == null || batchPositions.Count == 0)
            {
                return;
            }

            var paramNames = new List<string>(batchPositions.Count);
            using var cmd = new SqlCommand();
            cmd.Connection = conn;

            for (int i = 0; i < batchPositions.Count; i++)
            {
                var name = $"@bp{i}";
                paramNames.Add(name);
                cmd.Parameters.Add(name, SqlDbType.Int).Value = batchPositions[i];
            }

            cmd.Parameters.Add("@cardGameId", SqlDbType.Int).Value = cardGameId;

            cmd.CommandText = $@"
UPDATE dbo.NewTempCardgameInventory
SET rarity = 'Sealed'
WHERE cardGameId = @cardGameId
  AND batchPosition IN ({string.Join(",", paramNames)})
  AND (rarity IS NULL OR LTRIM(RTRIM(rarity)) = '')
  AND (
        LOWER(cardName) LIKE '%deck%'
        OR LOWER(cardName) LIKE '%pack%'
        OR LOWER(cardName) LIKE '%box%'
        OR LOWER(cardName) LIKE '%tin%'
        OR LOWER(cardName) LIKE '%display%'
        OR LOWER(cardName) LIKE '%collection%'
        OR LOWER(cardName) LIKE '%bundle%'
        OR LOWER(cardName) LIKE '%checklane blister%'
        OR LOWER(cardName) LIKE '%case%'
        OR LOWER(cardName) LIKE '%chest%'
        OR LOWER(cardName) LIKE '%build & battle%'
        OR LOWER(cardName) LIKE '%battle stadium%'
        OR LOWER(cardName) LIKE '%blister%'
        OR LOWER(cardName) LIKE '%secret lair%'
        OR LOWER(cardName) LIKE '%set'
        OR LOWER(cardName) LIKE '%set%'
        OR LOWER(cardName) LIKE '%toolkit%'
        OR LOWER(cardName) LIKE '%tcg%'
        OR LOWER(cardName) LIKE '%kit'
        OR LOWER(cardName) LIKE '%kit%'
        OR LOWER(cardName) LIKE '%calendar'
        OR LOWER(cardName) LIKE '%calendar%'
      );";

            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
        }

        // ---- DTOs FOR DESERIALIZATION / UI ----

        private sealed class TcgCsvProductsResponse
        {
            public bool Success { get; set; }
            public List<ProductResult>? Results { get; set; }
        }

        private sealed class ProductResult
        {
            public int ProductId { get; set; }
            public string? Name { get; set; }
            public string? ImageUrl { get; set; }
            public int GroupId { get; set; }
            public string? Url { get; set; }
            public List<ExtendedDataItem>? ExtendedData { get; set; }
        }

        private sealed class ExtendedDataItem
        {
            public string? Name { get; set; }
            public string? DisplayName { get; set; }
            public string? Value { get; set; }
        }

        public sealed class NewTempCardgameInventoryRow
        {
            public int TempId { get; init; }
            public int BatchPosition { get; init; }
            public int CardGameId { get; init; }
            public int SetId { get; init; }
            public int CardId { get; init; }
            public int ConditionId { get; init; } = 0;   // default 0 since column not present
            public string CardName { get; init; } = string.Empty;
            public string? Abbreviation { get; init; }
            public string? Rarity { get; init; }
            public string? Foil { get; init; }
            public string? ImageUrl { get; init; }
            public string? MktPriceUrl { get; init; }
            public decimal? MktPrice { get; init; }
            public int? AmtInStock { get; init; }
            public bool? Approved { get; init; }
            public bool NeedsReview { get; init; }
            public string? IssueNotes { get; init; }
            public DateTime? DateInserted { get; init; }
        }

    }
}
