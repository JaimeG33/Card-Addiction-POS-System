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

        public async Task<List<NewTempCardgameInventoryRow>> RunAsync(int cardGameId, int maxBatch, CancellationToken ct = default)
        {
            if (cardGameId <= 0) throw new ArgumentOutOfRangeException(nameof(cardGameId));
            if (maxBatch <= 0) return new List<NewTempCardgameInventoryRow>();

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
                    var products = await FetchProductsAsync(cardGameId, set.setId, ct).ConfigureAwait(false);

                    // Clear any previous rows for this batchPosition to avoid duplicates
                    using (var deleteCmd = new SqlCommand("DELETE FROM dbo.NewTempCardgameInventory WHERE batchPosition = @batchPosition;", conn, tx))
                    {
                        deleteCmd.Parameters.Add("@batchPosition", SqlDbType.Int).Value = set.tempId;
                        await deleteCmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
                    }

                    const string insertSql = @"
INSERT INTO dbo.NewTempCardgameInventory
    (batchPosition, cardGameId, setId, cardId, cardName, rarity, foil, imageURL, mktPriceURL, mktPrice, amtInStock, approved, needsReview, issueNotes)
VALUES
    (@batchPosition, @cardGameId, @setId, @cardId, @cardName, @rarity, @foil, @imageURL, @mktPriceURL, @mktPrice, @amtInStock, @approved, @needsReview, @issueNotes);";

                    using var insertCmd = new SqlCommand(insertSql, conn, tx);
                    insertCmd.Parameters.Add("@batchPosition", SqlDbType.Int);
                    insertCmd.Parameters.Add("@cardGameId", SqlDbType.Int);
                    insertCmd.Parameters.Add("@setId", SqlDbType.Int);
                    insertCmd.Parameters.Add("@cardId", SqlDbType.Int);
                    insertCmd.Parameters.Add("@cardName", SqlDbType.NVarChar, 255);
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

            // 3) Return the rows just inserted for display
            var batchPositions = sets.Select(s => s.tempId).ToList();
            return await LoadInventoryRowsAsync(conn, batchPositions, ct).ConfigureAwait(false);
        }

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

        private static string? GetRarity(ProductResult p)
        {
            if (p.ExtendedData == null) return null;
            var rarity = p.ExtendedData.FirstOrDefault(ed =>
                ed.Name?.Equals("Rarity", StringComparison.OrdinalIgnoreCase) == true ||
                ed.DisplayName?.Equals("Rarity", StringComparison.OrdinalIgnoreCase) == true);
            return rarity?.Value;
        }

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
SELECT tempId, batchPosition, cardGameId, setId, cardId, cardName, rarity, foil, imageURL, mktPriceURL, mktPrice, amtInStock, approved, needsReview, issueNotes, dateInserted
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
                    CardName = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                    Rarity = reader.IsDBNull(6) ? null : reader.GetString(6),
                    Foil = reader.IsDBNull(7) ? null : reader.GetString(7),
                    ImageUrl = reader.IsDBNull(8) ? null : reader.GetString(8),
                    MktPriceUrl = reader.IsDBNull(9) ? null : reader.GetString(9),
                    MktPrice = reader.IsDBNull(10) ? (decimal?)null : reader.GetDecimal(10),
                    AmtInStock = reader.IsDBNull(11) ? (int?)null : reader.GetInt32(11),
                    Approved = reader.IsDBNull(12) ? (bool?)null : reader.GetBoolean(12),
                    NeedsReview = reader.IsDBNull(13) ? false : reader.GetBoolean(13),
                    IssueNotes = reader.IsDBNull(14) ? null : reader.GetString(14),
                    DateInserted = reader.IsDBNull(15) ? (DateTime?)null : reader.GetDateTime(15)
                });
            }

            return results;
        }

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
            public string CardName { get; init; } = string.Empty;
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
