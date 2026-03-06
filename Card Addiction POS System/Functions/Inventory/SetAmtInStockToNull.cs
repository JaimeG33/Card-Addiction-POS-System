using System;
using System.Data;
using System.Threading.Tasks;
using Card_Addiction_POS_System.Data.SQLServer;
using Card_Addiction_POS_System.Functions.Models;
using Microsoft.Data.SqlClient;

namespace Card_Addiction_POS_System.Functions.Inventory
{
    /// <summary>
    /// Sets amtInStock to NULL for a selected inventory row.
    /// </summary>
    internal sealed class SetAmtInStockToNull
    {
        private readonly SqlConnectionFactory _connectionFactory;
        private readonly Func<Task<string>> _getPasswordAsync;

        public SetAmtInStockToNull(SqlConnectionFactory connectionFactory, Func<Task<string>> getPasswordAsync)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            _getPasswordAsync = getPasswordAsync ?? throw new ArgumentNullException(nameof(getPasswordAsync));
        }

        public async Task<string> SetNotInInventoryAsync(int cardGameId, int cardId, string cardName)
        {
            if (!SelectedCardGameLogic.TryGetById(cardGameId, out var game))
            {
                throw new ArgumentException("Invalid card game selection.", nameof(cardGameId));
            }

            if (cardId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(cardId), "CardId must be greater than 0.");
            }

            var tableName = string.Concat(game.DatabaseName, "Inventory");

            var sql = $@"UPDATE {tableName}
                         SET amtInStock = @amtInStock
                         WHERE cardId = @cardId;";

            var password = await _getPasswordAsync().ConfigureAwait(false);
            using var conn = _connectionFactory.CreateForCurrentUser(password);
            password = string.Empty;

            await conn.OpenAsync().ConfigureAwait(false);

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@amtInStock", SqlDbType.Int).Value = DBNull.Value;
            cmd.Parameters.Add("@cardId", SqlDbType.Int).Value = cardId;

            var rowsAffected = await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            if (rowsAffected <= 0)
            {
                throw new InvalidOperationException("No inventory row was updated. Verify the selected card still exists.");
            }

            return $"{cardName} was set to \"Not Inv\" in the {game.DatabaseName} inventory.";
        }
    }
}
