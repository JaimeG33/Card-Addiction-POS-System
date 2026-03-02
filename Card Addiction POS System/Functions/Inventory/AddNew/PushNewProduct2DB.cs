using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Card_Addiction_POS_System.Data.SQLServer;
using Card_Addiction_POS_System.Functions.Models;
using Microsoft.Data.SqlClient;

namespace Card_Addiction_POS_System.Functions.Inventory.AddNew
{
    /// <summary>
    /// Orchestrates pushing validated temp data into the live rarity/set/inventory tables.
    /// </summary>
    internal sealed class PushNewProduct2DB
    {
        private readonly SqlConnectionFactory _connectionFactory;
        private readonly Func<Task<string>> _getPasswordAsync;

        public PushNewProduct2DB(SqlConnectionFactory connectionFactory, Func<Task<string>> getPasswordAsync)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            _getPasswordAsync = getPasswordAsync ?? throw new ArgumentNullException(nameof(getPasswordAsync));
        }

        public async Task RunAsync(SelectedCardGameLogic game, CancellationToken ct = default)
        {
            if (game is null)
            {
                throw new ArgumentNullException(nameof(game));
            }

            if (game.CardGameId <= 0)
            {
                throw new InvalidOperationException("Select a card game before pushing data to the database.");
            }

            var password = await _getPasswordAsync().ConfigureAwait(false);
            using var conn = _connectionFactory.CreateForCurrentUser(password);
            password = string.Empty;

            await conn.OpenAsync(ct).ConfigureAwait(false);
            using var tx = conn.BeginTransaction();

            // --- Step 0: Detect duplicates in the destination tables and stop if any are found.
            var duplicateSets = await FindDuplicateSetIdsAsync(conn, tx, game, ct).ConfigureAwait(false);
            var duplicateCards = await FindDuplicateCardIdsAsync(conn, tx, game, ct).ConfigureAwait(false);

            if (duplicateSets.Count > 0 || duplicateCards.Count > 0)
            {
                tx.Rollback();
                throw new InvalidOperationException(BuildDuplicateMessage(duplicateSets, duplicateCards));
            }

            // --- Step 1: Add missing rarities
            await UpsertMissingRaritiesAsync(conn, tx, game, ct).ConfigureAwait(false);

            // --- Step 2: Push sets
            await InsertSetsAsync(conn, tx, game, ct).ConfigureAwait(false);

            // --- Step 3: Push inventory
            await InsertInventoryAsync(conn, tx, game, ct).ConfigureAwait(false);

            tx.Commit();
        }

        private static string BuildDuplicateMessage(IReadOnlyCollection<int> setIds, IReadOnlyCollection<int> cardIds)
        {
            var parts = new List<string>();
            if (setIds.Count > 0)
            {
                parts.Add($"SetId already exists: {string.Join(", ", setIds.OrderBy(id => id))}");
            }

            if (cardIds.Count > 0)
            {
                parts.Add($"CardId already exists: {string.Join(", ", cardIds.OrderBy(id => id))}");
            }

            return parts.Count == 0
                ? "Duplicate rows detected."
                : $"Remove duplicates from the temp tables before pushing:\n{string.Join("\n", parts)}";
        }

        private static string GetRarityTableName(SelectedCardGameLogic game) =>
            $"{game.DatabaseName}Rarity";

        private static string GetSetTableName(SelectedCardGameLogic game) =>
            $"{game.DatabaseName}Set";

        private static string GetInventoryTableName(SelectedCardGameLogic game) =>
            $"{game.DatabaseName}Inventory";

        private static async Task<IReadOnlyCollection<int>> FindDuplicateSetIdsAsync(
            SqlConnection conn,
            SqlTransaction tx,
            SelectedCardGameLogic game,
            CancellationToken ct)
        {
            var sql = $@"
SELECT DISTINCT s.setId
FROM dbo.[{GetSetTableName(game)}] AS tgt WITH (UPDLOCK, HOLDLOCK)
INNER JOIN dbo.NewCardgameSetTemp AS s
    ON s.cardGameId = @cardGameId
   AND s.includeInBatch = 1
   AND s.setId = tgt.setId;";

            using var cmd = new SqlCommand(sql, conn, tx);
            cmd.Parameters.Add("@cardGameId", SqlDbType.Int).Value = game.CardGameId;

            var results = new List<int>();
            using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
            while (await reader.ReadAsync(ct).ConfigureAwait(false))
            {
                if (!reader.IsDBNull(0))
                {
                    results.Add(reader.GetInt32(0));
                }
            }

            return results;
        }

        private static async Task<IReadOnlyCollection<int>> FindDuplicateCardIdsAsync(
            SqlConnection conn,
            SqlTransaction tx,
            SelectedCardGameLogic game,
            CancellationToken ct)
        {
            var sql = $@"
SELECT DISTINCT inv.cardId
FROM dbo.[{GetInventoryTableName(game)}] AS inv WITH (UPDLOCK, HOLDLOCK)
INNER JOIN dbo.NewTempCardgameInventory AS src
    ON src.cardGameId = @cardGameId
   AND src.cardId = inv.cardId;";

            using var cmd = new SqlCommand(sql, conn, tx);
            cmd.Parameters.Add("@cardGameId", SqlDbType.Int).Value = game.CardGameId;

            var results = new List<int>();
            using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
            while (await reader.ReadAsync(ct).ConfigureAwait(false))
            {
                if (!reader.IsDBNull(0))
                {
                    results.Add(reader.GetInt32(0));
                }
            }

            return results;
        }

        private static async Task UpsertMissingRaritiesAsync(
            SqlConnection conn,
            SqlTransaction tx,
            SelectedCardGameLogic game,
            CancellationToken ct)
        {
            var rarityTable = GetRarityTableName(game);

            // Existing rarities
            var existing = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var loadSql = $@"SELECT rarity FROM dbo.[{rarityTable}] WHERE cardGameId = @cardGameId;";
            using (var loadCmd = new SqlCommand(loadSql, conn, tx))
            {
                loadCmd.Parameters.Add("@cardGameId", SqlDbType.Int).Value = game.CardGameId;
                using var reader = await loadCmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
                while (await reader.ReadAsync(ct).ConfigureAwait(false))
                {
                    if (!reader.IsDBNull(0))
                    {
                        existing.Add(reader.GetString(0).Trim());
                    }
                }
            }

            // Missing rarities from temp
            var tempRarities = new List<string>();
            const string tempSql = @"
SELECT DISTINCT rarity
FROM dbo.NewTempCardgameInventory
WHERE cardGameId = @cardGameId
  AND rarity IS NOT NULL
  AND LTRIM(RTRIM(rarity)) <> '';";

            using (var tempCmd = new SqlCommand(tempSql, conn, tx))
            {
                tempCmd.Parameters.Add("@cardGameId", SqlDbType.Int).Value = game.CardGameId;
                using var reader = await tempCmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
                while (await reader.ReadAsync(ct).ConfigureAwait(false))
                {
                    if (!reader.IsDBNull(0))
                    {
                        tempRarities.Add(reader.GetString(0).Trim());
                    }
                }
            }

            var toInsert = tempRarities
                .Where(r => !string.IsNullOrWhiteSpace(r))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Where(r => !existing.Contains(r))
                .ToList();

            if (toInsert.Count == 0)
            {
                return;
            }

            var insertSql = $@"
INSERT INTO dbo.[{rarityTable}] (cardGameId, rarity)
VALUES (@cardGameId, @rarity);";

            using var insertCmd = new SqlCommand(insertSql, conn, tx);
            insertCmd.Parameters.Add("@cardGameId", SqlDbType.Int).Value = game.CardGameId;
            var rarityParam = insertCmd.Parameters.Add("@rarity", SqlDbType.NVarChar, 100);

            foreach (var rarity in toInsert)
            {
                rarityParam.Value = rarity;
                await insertCmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            }
        }

        private static async Task InsertSetsAsync(
            SqlConnection conn,
            SqlTransaction tx,
            SelectedCardGameLogic game,
            CancellationToken ct)
        {
            var setTable = GetSetTableName(game);

            var sql = $@"
INSERT INTO dbo.[{setTable}] (setId, cardGameId, setName, abbreviation, setDate)
SELECT src.setId,
       src.cardGameId,
       src.setName,
       src.abbreviation,
       src.setDate
FROM dbo.NewCardgameSetTemp AS src
WHERE src.cardGameId = @cardGameId
  AND src.includeInBatch = 1
  AND NOT EXISTS (
        SELECT 1 FROM dbo.[{setTable}] tgt
        WHERE tgt.setId = src.setId
    );";

            using var cmd = new SqlCommand(sql, conn, tx);
            cmd.Parameters.Add("@cardGameId", SqlDbType.Int).Value = game.CardGameId;
            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
        }

        private static async Task<bool> TargetTableHasColumnAsync(
            SqlConnection conn,
            SqlTransaction tx,
            string tableName,
            string columnName,
            CancellationToken ct)
        {
            const string sql = @"
SELECT TOP (1) 1
FROM sys.columns c
INNER JOIN sys.tables t ON t.object_id = c.object_id
INNER JOIN sys.schemas s ON s.schema_id = t.schema_id
WHERE s.name = 'dbo'
  AND t.name = @tableName
  AND c.name = @columnName;";

            using var cmd = new SqlCommand(sql, conn, tx);
            cmd.Parameters.Add("@tableName", SqlDbType.NVarChar, 128).Value = tableName;
            cmd.Parameters.Add("@columnName", SqlDbType.NVarChar, 128).Value = columnName;

            var result = await cmd.ExecuteScalarAsync(ct).ConfigureAwait(false);
            return result != null && result != DBNull.Value;
        }

        private static async Task InsertInventoryAsync(
            SqlConnection conn,
            SqlTransaction tx,
            SelectedCardGameLogic game,
            CancellationToken ct)
        {
            var invTable = GetInventoryTableName(game);
            var hasAbbreviation = await TargetTableHasColumnAsync(conn, tx, invTable, "abbreviation", ct).ConfigureAwait(false);

            var abbreviationInsertColumn = hasAbbreviation ? ", abbreviation" : string.Empty;
            var abbreviationSelectColumn = hasAbbreviation
                ? ", NULLIF(LTRIM(RTRIM(src.abbreviation)), '') AS abbreviation"
                : string.Empty;

            var sql = $@"
INSERT INTO dbo.[{invTable}]
    (cardId, cardGameId, setId, conditionId, cardName, rarity, mktPrice, amtInStock, imageURL, mktPriceURL{abbreviationInsertColumn})
SELECT src.cardId,
       src.cardGameId,
       src.setId,
       COALESCE(src.conditionId, 0) AS conditionId,
       src.cardName,
       src.rarity,
       src.mktPrice,
       src.amtInStock,
       src.imageURL,
       src.mktPriceURL{abbreviationSelectColumn}
FROM dbo.NewTempCardgameInventory AS src
WHERE src.cardGameId = @cardGameId
  AND (src.approved = 1 OR src.approved IS NULL)
  AND (src.needsReview = 0 OR src.needsReview IS NULL)
  AND NOT EXISTS (
        SELECT 1 FROM dbo.[{invTable}] tgt
        WHERE tgt.cardId = src.cardId
    );";

            using var cmd = new SqlCommand(sql, conn, tx);
            cmd.Parameters.Add("@cardGameId", SqlDbType.Int).Value = game.CardGameId;
            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
        }
    }
}
