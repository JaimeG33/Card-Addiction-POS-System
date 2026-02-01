using System;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Card_Addiction_POS_System.Data.SQLServer;

namespace Card_Addiction_POS_System.Functions.Sales
{
    /// <summary>       
    /// Determines or reserves a sale id to be used for the current sale.
    /// Behavior:
    /// - If the most recent Sale row has an empty/null orderStatus or is in "pre-prep" (case-insensitive)
    ///   the existing saleId is returned so the application reuses the pre-prep row.
    /// - Otherwise the next id is computed as (MAX(saleId) + 1) and returned.
    /// </summary>
    internal sealed class DetermineSaleId
    {
        private readonly SqlConnectionFactory _connectionFactory;
        private readonly Func<Task<string>> _getPasswordAsync;

        public DetermineSaleId(SqlConnectionFactory connectionFactory, Func<Task<string>> getPasswordAsync)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            _getPasswordAsync = getPasswordAsync ?? throw new ArgumentNullException(nameof(getPasswordAsync));
        }

        /// <summary>
        /// Reserve or reuse the sale id for the next sale.
        /// - If latest sale row exists and its orderStatus is NULL/empty or indicates "pre-prep", returns that saleId.
        /// - Otherwise returns MAX(saleId) + 1.
        /// </summary>
        public async Task<int> ReserveNextSaleIdAsync()
        {
            const string latestQuery = @"
                SELECT TOP 1 saleId, orderStatus
                FROM dbo.Sale
                ORDER BY saleDate DESC;";

            const string maxQuery = "SELECT ISNULL(MAX(saleId), 0) FROM dbo.Sale;";

            var password = await _getPassword_async_guard().ConfigureAwait(false);
            using var conn = _connectionFactory.CreateForCurrentUser(password);
            password = string.Empty;

            await conn.OpenAsync().ConfigureAwait(false);

            // Try to get the latest sale row
            using (var cmdLatest = new SqlCommand(latestQuery, conn))
            {
                using var reader = await cmdLatest.ExecuteReaderAsync(CommandBehavior.SingleRow).ConfigureAwait(false);
                if (await reader.ReadAsync().ConfigureAwait(false))
                {
                    // saleId is SMALLINT in DB -> read as short then convert to int
                    int dbSaleId = reader.IsDBNull(0) ? 0 : (int)reader.GetInt16(0);
                    var dbOrderStatus = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);

                    if (string.IsNullOrWhiteSpace(dbOrderStatus)
                        || dbOrderStatus.Equals("pre-prep", StringComparison.OrdinalIgnoreCase)
                        || dbOrderStatus.Equals("preprep", StringComparison.OrdinalIgnoreCase)
                        || dbOrderStatus.Equals("preprep", StringComparison.OrdinalIgnoreCase)
                        || dbOrderStatus.Equals("PrePrep", StringComparison.OrdinalIgnoreCase))
                    {
                        if (dbSaleId > 0)
                            return dbSaleId;
                        // otherwise fall through to compute next
                    }
                }
            }

            // Compute next id = max + 1
            using (var cmdMax = new SqlCommand(maxQuery, conn))
            {
                var result = await cmdMax.ExecuteScalarAsync().ConfigureAwait(false);
                var max = (result == null || result == DBNull.Value) ? 0 : Convert.ToInt32(result);
                var next = max + 1;
                return next;
            }

            async Task<string> _getPassword_async_guard() => await _getPasswordAsync().ConfigureAwait(false);
        }
    }
}
