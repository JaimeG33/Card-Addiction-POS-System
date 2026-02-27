using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Card_Addiction_POS_System.Data.SQLServer;
using Microsoft.Data.SqlClient;

namespace Card_Addiction_POS_System.Functions.Inventory.AddNew
{
    /// <summary>
    /// Central helper that inspects temp inventory rows for missing/invalid data
    /// and writes issue notes back to the database.
    /// </summary>
    internal sealed class ErrorIdentifyerHelper
    {
        /// <summary>
        /// Aggregated results describing critical/ok sets and per-row issues.
        /// </summary>
        public sealed class InventoryIssueResult
        {
            public HashSet<int> CriticalSetIds { get; } = new();
            public HashSet<int> GoodSetIds { get; } = new();
            public Dictionary<int, HashSet<string>> InventoryCellIssues { get; } = new();     // red (required missing)
            public Dictionary<int, HashSet<string>> InventoryCellWarnings { get; } = new();   // blue (e.g., rarity not in table)
            public HashSet<string> MissingRarityValues { get; } = new();
            public Dictionary<int, string> IssueNotesByTempId { get; } = new();               // tempId -> note
        }

        // ---- ISSUE EVALUATION ----
        // Evaluates each temp inventory row for required-field gaps and rarity table presence.

        public async Task<InventoryIssueResult> EvaluateInventoryIssuesAsync(
            IEnumerable<AddNewYugiohInventory.NewTempCardgameInventoryRow> rows,
            SqlConnectionFactory connectionFactory,
            Func<Task<string>> getPasswordAsync,
            string databaseName,
            int cardGameId,
            CancellationToken ct = default)
        {
            var result = new InventoryIssueResult();
            if (rows == null) return result;

            // Load known rarities for this game (if table exists/accessible)
            var (raritiesInDb, rarityLookupAvailable) = await LoadRaritiesAsync(connectionFactory, getPasswordAsync, databaseName, cardGameId, ct).ConfigureAwait(false);

            var setIdsWithRequiredIssues = new HashSet<int>();
            var setIdsSeen = new HashSet<int>();

            foreach (var row in rows)
            {
                setIdsSeen.Add(row.SetId);
                var missingCols = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                // Required fields check
                if (row.CardId == 0) missingCols.Add(nameof(row.CardId));
                if (row.CardGameId == 0) missingCols.Add(nameof(row.CardGameId));
                if (row.SetId == 0) missingCols.Add(nameof(row.SetId));
                if (string.IsNullOrWhiteSpace(row.CardName)) missingCols.Add(nameof(row.CardName));
                if (string.IsNullOrWhiteSpace(row.Rarity)) missingCols.Add(nameof(row.Rarity));

                if (missingCols.Count > 0)
                {
                    result.InventoryCellIssues[row.TempId] = missingCols;
                    if (row.SetId != 0) setIdsWithRequiredIssues.Add(row.SetId);
                }

                // Optional rarity validation against DB values (warn only)
                if (rarityLookupAvailable &&
                    !string.IsNullOrWhiteSpace(row.Rarity) &&
                    !raritiesInDb.Contains(row.Rarity.Trim(), StringComparer.OrdinalIgnoreCase))
                {
                    result.MissingRarityValues.Add(row.Rarity);
                    if (!result.InventoryCellWarnings.TryGetValue(row.TempId, out var warnCols))
                    {
                        warnCols = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                        result.InventoryCellWarnings[row.TempId] = warnCols;
                    }
                    warnCols.Add(nameof(row.Rarity));
                }

                // Build a human-readable issue note for UI/storage
                var note = BuildIssueNote(missingCols, rarityLookupAvailable, rarityMissing: result.InventoryCellWarnings.ContainsKey(row.TempId));
                result.IssueNotesByTempId[row.TempId] = note;
            }

            // Good vs critical set classification
            foreach (var setId in setIdsSeen)
            {
                if (setId != 0 && !setIdsWithRequiredIssues.Contains(setId))
                {
                    result.GoodSetIds.Add(setId);
                }
            }

            foreach (var setId in setIdsWithRequiredIssues)
            {
                result.CriticalSetIds.Add(setId);
            }

            return result;
        }

        // ---- ISSUE NOTE CONSTRUCTION ----
        // Turns missing columns + rarity warning into a single note string.

        private static string BuildIssueNote(
            HashSet<string> missingCols,
            bool rarityLookupAvailable,
            bool rarityMissing)
        {
            var parts = new List<string>();
            if (missingCols is { Count: > 0 })
            {
                parts.Add("Missing: " + string.Join(", ", missingCols));
            }

            if (rarityLookupAvailable && rarityMissing)
            {
                parts.Add("Rarity not found in rarity table.");
            }

            return parts.Count == 0 ? string.Empty : string.Join(" | ", parts);
        }

        // ---- DB UPDATE OF ISSUE NOTES ----
        // Persists calculated issue notes back into dbo.NewTempCardgameInventory.

        public async Task UpdateIssueNotesAsync(
            IEnumerable<AddNewYugiohInventory.NewTempCardgameInventoryRow> rows,
            Dictionary<int, int> displayToActualTempId,
            SqlConnectionFactory connectionFactory,
            Func<Task<string>> getPasswordAsync,
            int cardGameId,
            InventoryIssueResult issues,
            CancellationToken ct = default)
        {
            var password = await getPasswordAsync().ConfigureAwait(false);
            using var conn = connectionFactory.CreateForCurrentUser(password);
            password = string.Empty;
            await conn.OpenAsync(ct).ConfigureAwait(false);

            const string sql = @"
UPDATE dbo.NewTempCardgameInventory
SET issueNotes = @issueNotes
WHERE cardGameId = @cardGameId AND tempId = @tempId;";

            foreach (var row in rows)
            {
                // UI can remap display tempId to actual DB tempId
                if (!displayToActualTempId.TryGetValue(row.TempId, out var actualTempId) || actualTempId == 0)
                {
                    actualTempId = row.TempId;
                }

                issues.IssueNotesByTempId.TryGetValue(row.TempId, out var note);
                var noteVal = string.IsNullOrWhiteSpace(note) ? DBNull.Value : (object)note!;

                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add("@cardGameId", SqlDbType.Int).Value = cardGameId;
                cmd.Parameters.Add("@tempId", SqlDbType.Int).Value = actualTempId;
                cmd.Parameters.Add("@issueNotes", SqlDbType.NVarChar, -1).Value = noteVal;

                await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            }
        }

        // ---- RARITY LOOKUP LOADING ----
        // Attempts to read rarity values from the game's rarity table; returns availability flag.

        private static async Task<(HashSet<string> rarities, bool available)> LoadRaritiesAsync(
            SqlConnectionFactory connectionFactory,
            Func<Task<string>> getPasswordAsync,
            string databaseName,
            int cardGameId,
            CancellationToken ct)
        {
            var rarities = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (string.IsNullOrWhiteSpace(databaseName)) return (rarities, false);
            if (!Regex.IsMatch(databaseName, @"^[A-Za-z0-9_]+$")) return (rarities, false);

            var tableName = $"{databaseName}Rarity";

            var password = await getPasswordAsync().ConfigureAwait(false);
            using var conn = connectionFactory.CreateForCurrentUser(password);
            password = string.Empty;
            await conn.OpenAsync(ct).ConfigureAwait(false);

            var sql = $"SELECT rarity FROM dbo.[{tableName}] WHERE cardGameId = @cardGameId;";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@cardGameId", SqlDbType.Int).Value = cardGameId;

            try
            {
                using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
                while (await reader.ReadAsync(ct).ConfigureAwait(false))
                {
                    if (!reader.IsDBNull(0))
                    {
                        var r = reader.GetString(0);
                        if (!string.IsNullOrWhiteSpace(r)) rarities.Add(r.Trim());
                    }
                }

                return (rarities, true);
            }
            catch
            {
                return (new HashSet<string>(StringComparer.OrdinalIgnoreCase), false);
            }
        }
    }
}
