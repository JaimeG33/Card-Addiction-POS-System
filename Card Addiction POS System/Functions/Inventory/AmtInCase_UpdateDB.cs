using System;
using System.Data;
using System.Threading.Tasks;
using Card_Addiction_POS_System.Data.SQLServer;
using Card_Addiction_POS_System.Functions.Models;
using Microsoft.Data.SqlClient;

namespace Card_Addiction_POS_System.Functions.Inventory
{
    /// <summary>
    /// Small helper service to update the amtInCase column for a single inventory row.
    /// Accepts numeric cardGameId and assembles the table name as DatabaseName + "Inventory".
    /// </summary>
    internal class AmtInCase_UpdateDB
    {
        private readonly SqlConnectionFactory _connectionFactory;
        private readonly Func<Task<string>> _getPasswordAsync;

        public AmtInCase_UpdateDB(SqlConnectionFactory connectionFactory, Func<Task<string>> getPasswordAsync)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            _getPasswordAsync = getPasswordAsync ?? throw new ArgumentNullException(nameof(getPasswordAsync));
        }

        /// <summary>
        /// Update the amtInCase column for a specific card row.
        /// </summary>
        public async Task UpdateAmountAsync(int cardGameId, int cardId, int newAmount)
        {
            if (!SelectedCardGameLogic.TryGetById(cardGameId, out var game))
                throw new ArgumentException("Invalid card game selection.", nameof(cardGameId));

            var tableName = string.Concat(game.DatabaseName, "Inventory");

            var updateQuery = $@"UPDATE {tableName}
                                 SET amtInCase = @amtInCase
                                 WHERE cardId = @cardId;";

            var password = await _getPasswordAsync().ConfigureAwait(false);
            using var conn = _connectionFactory.CreateForCurrentUser(password);
            password = string.Empty;

            await conn.OpenAsync().ConfigureAwait(false);

            using var cmd = new SqlCommand(updateQuery, conn);
            cmd.Parameters.Add("@amtInCase", SqlDbType.Int).Value = newAmount;
            cmd.Parameters.Add("@cardId", SqlDbType.Int).Value = cardId;

            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
        }
    }
}
