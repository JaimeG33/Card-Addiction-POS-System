using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Card_Addiction_POS_System.Data.SQLServer;
using Card_Addiction_POS_System.Functions.Models;

namespace Card_Addiction_POS_System.Functions.Sales
{
    /// <summary>
    /// Service responsible for sale workflow DB operations:
    /// - create sale row (optionally using a provided saleId),
    /// - insert TransactionLine rows (including transactionId),
    /// - update amtInStock in game-specific inventory tables (batched per table).
    /// </summary>
    internal class SaleWorkflowService
    {
        private readonly SqlConnectionFactory _connectionFactory;
        private readonly Func<Task<string>> _getPasswordAsync;

        // Map cardGameId (the numeric id used in TransactionLineItem.CardGameId) to the corresponding inventory table.
        private static readonly Dictionary<int, string> _cardGameToTable = new()
        {
            [1] = "YugiohInventory",
            [2] = "MagicInventory",
            [3] = "PokemonInventory"
        };

        public SaleWorkflowService(SqlConnectionFactory connectionFactory, Func<Task<string>> getPasswordAsync)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            _getPasswordAsync = getPasswordAsync ?? throw new ArgumentNullException(nameof(getPasswordAsync));
        }

        /// <summary>
        /// Create a Sale record.
        /// - If providedSaleId has a value, that id will be inserted into the saleId column.
        /// - Otherwise the insert will rely on an IDENTITY and the method returns the generated identity.
        /// Returns the resulting sale id (either providedSaleId or identity).
        /// </summary>
        public async Task<int> CreateSaleAsync(DateTimeOffset saleDate, int? providedSaleId = null, byte registerId = 1, byte? employeeId = null, string? orderStatus = null)
        {
            const string insertWithSaleId = @"
                INSERT INTO dbo.Sale (saleDate, saleId, register, employeeId, orderStatus)
                VALUES (@saleDate, @saleId, @register, @employeeId, @orderStatus);";

            const string insertWithoutSaleId = @"
                INSERT INTO dbo.Sale (saleDate, register, employeeId, orderStatus)
                VALUES (@saleDate, @register, @employeeId, @orderStatus);
                SELECT CAST(SCOPE_IDENTITY() AS int);";

            var password = await _getPasswordAsync().ConfigureAwait(false);
            using var conn = _connectionFactory.CreateForCurrentUser(password);
            password = string.Empty;

            await conn.OpenAsync().ConfigureAwait(false);

            if (providedSaleId.HasValue)
            {
                using var cmd = new SqlCommand(insertWithSaleId, conn);
                cmd.Parameters.Add("@saleDate", SqlDbType.DateTime).Value = saleDate.UtcDateTime;
                // saleId is smallint in DB
                cmd.Parameters.Add("@saleId", SqlDbType.SmallInt).Value = providedSaleId.Value;
                cmd.Parameters.Add("@register", SqlDbType.TinyInt).Value = registerId;
                if (employeeId.HasValue)
                    cmd.Parameters.Add("@employeeId", SqlDbType.TinyInt).Value = employeeId.Value;
                else
                    cmd.Parameters.Add("@employeeId", SqlDbType.TinyInt).Value = DBNull.Value;
                cmd.Parameters.Add("@orderStatus", SqlDbType.NVarChar, 50).Value = (object?)orderStatus ?? DBNull.Value;

                await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                return providedSaleId.Value;
            }
            else
            {
                using var cmd = new SqlCommand(insertWithoutSaleId, conn);
                cmd.Parameters.Add("@saleDate", SqlDbType.DateTime).Value = saleDate.UtcDateTime;
                cmd.Parameters.Add("@register", SqlDbType.TinyInt).Value = registerId;
                if (employeeId.HasValue)
                    cmd.Parameters.Add("@employeeId", SqlDbType.TinyInt).Value = employeeId.Value;
                else
                    cmd.Parameters.Add("@employeeId", SqlDbType.TinyInt).Value = DBNull.Value;
                cmd.Parameters.Add("@orderStatus", SqlDbType.NVarChar, 50).Value = (object?)orderStatus ?? DBNull.Value;

                var result = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
                if (result == null || result == DBNull.Value)
                    throw new Exception("Failed to create Sale record or retrieve its identity.");

                return Convert.ToInt32(result);
            }
        }

        /// <summary>
        /// Update orderStatus for an existing sale row.
        /// </summary>
        public async Task UpdateSaleStatusAsync(int saleId, string orderStatus)
        {
            const string updateSql = @"
                UPDATE dbo.Sale
                SET orderStatus = @orderStatus
                WHERE saleId = @saleId;";

            var password = await _getPassword_async_guard().ConfigureAwait(false);
            using var conn = _connectionFactory.CreateForCurrentUser(password);
            password = string.Empty;

            await conn.OpenAsync().ConfigureAwait(false);

            using var cmd = new SqlCommand(updateSql, conn);
            cmd.Parameters.Add("@orderStatus", SqlDbType.NVarChar, 50).Value = orderStatus ?? string.Empty;
            cmd.Parameters.Add("@saleId", SqlDbType.SmallInt).Value = saleId;

            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);

            async Task<string> _getPassword_async_guard() => await _getPasswordAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Persist transaction lines and update inventory stock.
        /// - Inserts one row per TransactionLineItem into TransactionLine table.
        /// - Ensures each inserted row includes a transactionId (1..N) for the sale.
        /// - Updates amtInStock in inventory tables grouped by cardGameId (batched CASE..WHEN per table).
        /// All operations are executed inside a single DB transaction.
        /// </summary>
        public async Task SaveTransactionLinesAsync(int saleId, IReadOnlyList<TransactionLineItem> lines)
        {
            if (lines == null) throw new ArgumentNullException(nameof(lines));
            if (lines.Count == 0) return;

            var password = await _getPassword_async_guard().ConfigureAwait(false);
            using var conn = _connectionFactory.CreateForCurrentUser(password);
            password = string.Empty;

            await conn.OpenAsync().ConfigureAwait(false);

            using var tran = conn.BeginTransaction();
            try
            {
                // TransactionLine requires transactionId and saleId (composite PK).
                const string insertLineSql = @"
                    INSERT INTO TransactionLine
                    (transactionId, saleId, cardGameId, cardId, conditionId, cardName, rarity, setId, timeMktPrice, agreedPrice, amtTraded, buyOrSell)
                    VALUES
                    (@transactionId, @saleId, @cardGameId, @cardId, @conditionId, @cardName, @rarity, @setId, @timeMktPrice, @agreedPrice, @amtTraded, @buyOrSell);";

                // transactionId must increment per item within the same sale starting at 1
                short transactionCounter = 1;

                foreach (var item in lines)
                {
                    using var cmd = new SqlCommand(insertLineSql, conn, tran);

                    // transactionId & saleId are smallints in the DB
                    cmd.Parameters.Add("@transactionId", SqlDbType.SmallInt).Value = transactionCounter;
                    cmd.Parameters.Add("@saleId", SqlDbType.SmallInt).Value = saleId;

                    cmd.Parameters.Add("@cardGameId", SqlDbType.TinyInt).Value = (byte)item.CardGameId;
                    cmd.Parameters.Add("@cardId", SqlDbType.Int).Value = item.CardId;
                    cmd.Parameters.Add("@conditionId", SqlDbType.TinyInt).Value = (byte)item.ConditionId;
                    cmd.Parameters.Add("@cardName", SqlDbType.NVarChar, 200).Value = item.CardName ?? string.Empty;
                    cmd.Parameters.Add("@rarity", SqlDbType.NVarChar, 50).Value = item.Rarity ?? string.Empty;
                    cmd.Parameters.Add("@setId", SqlDbType.SmallInt).Value = item.SetId;

                    // prices are smallmoney in DB
                    var pTime = cmd.Parameters.Add("@timeMktPrice", SqlDbType.SmallMoney);
                    pTime.Value = item.TimeMktPrice;
                    var pAgreed = cmd.Parameters.Add("@agreedPrice", SqlDbType.SmallMoney);
                    pAgreed.Value = item.AgreedPrice;

                    cmd.Parameters.Add("@amtTraded", SqlDbType.TinyInt).Value = (byte)item.AmtTraded;
                    cmd.Parameters.Add("@buyOrSell", SqlDbType.Bit).Value = item.BuyOrSell;

                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);

                    // Optionally update the in-memory item with assigned IDs
                    item.SaleId = saleId;
                    item.TransactionId = transactionCounter;

                    transactionCounter++;
                }

                // Update inventory amtInStock in each game table (batched).
                var grouped = lines.GroupBy(l => l.CardGameId);
                foreach (var group in grouped)
                {
                    if (!_cardGameToTable.TryGetValue(group.Key, out var tableName))
                        continue; // unknown game id; skip

                    var sb = new StringBuilder();
                    var idList = new List<int>();

                    sb.Append($"UPDATE {tableName} SET amtInStock = amtInStock + CASE cardId ");

                    foreach (var item in group)
                    {
                        var stockChange = item.BuyOrSell ? -item.AmtTraded : item.AmtTraded;
                        sb.Append($"WHEN {item.CardId} THEN {stockChange} ");
                        idList.Add(item.CardId);
                    }

                    sb.Append("END WHERE cardId IN (");
                    sb.Append(string.Join(",", idList));
                    sb.Append(");");

                    using var cmdUpdate = new SqlCommand(sb.ToString(), conn, tran);
                    await cmdUpdate.ExecuteNonQueryAsync().ConfigureAwait(false);
                }

                tran.Commit();
            }
            catch
            {
                try { tran.Rollback(); } catch { /* swallow */ }
                throw;
            }

            async Task<string> _getPassword_async_guard() => await _getPasswordAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Returns the orderStatus for the specified saleId or null if the sale row does not exist.
        /// </summary>
        public async Task<string?> GetSaleOrderStatusAsync(int saleId)
        {
            const string sql = @"
        SELECT orderStatus
        FROM dbo.Sale
        WHERE saleId = @saleId;";

            var password = await _getPassword_async_guard().ConfigureAwait(false);
            using var conn = _connectionFactory.CreateForCurrentUser(password);
            password = string.Empty;

            await conn.OpenAsync().ConfigureAwait(false);

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@saleId", SqlDbType.SmallInt).Value = saleId;

            var result = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
            if (result == null || result == DBNull.Value)
                return null;

            return Convert.ToString(result);
        }

        private async Task<string> _getPassword_async_guard()
        {
            return await _getPasswordAsync().ConfigureAwait(false);
        }
    }
}
