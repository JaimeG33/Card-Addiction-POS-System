using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Card_Addiction_POS_System.Data.SQLServer;

namespace Card_Addiction_POS_System.Functions.Inventory
{
    /// <summary>
    /// Small helper service to update the amtInStock column for a single inventory row.
    /// Designed to be reusable from forms or other services.  Does not refresh any UI.
    /// </summary>
    internal class AmtInStock_UpdateDB
    {
        private readonly SqlConnectionFactory _connectionFactory;
        private readonly Func<Task<string>> _getPasswordAsync;

        // Keep same mapping used by SearchInventoryDB.InventoryService so callers can use the same card game keys.
        private static readonly Dictionary<string, string> _cardGameToTable = new()
        {
            ["Yugioh"] = "YugiohInventory",
            ["Magic"] = "MagicInventory",
            ["Pokemon"] = "PokemonInventory"
        };

        public AmtInStock_UpdateDB(SqlConnectionFactory connectionFactory, Func<Task<string>> getPasswordAsync)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            _getPasswordAsync = getPasswordAsync ?? throw new ArgumentNullException(nameof(getPasswordAsync));
        }

        /// <summary>
        /// Update the amtInStock column for a specific card row.
        /// Does not refresh any UI; returns when the database update completes.
        /// </summary>
        /// <param name="cardGameKey">Logical game key (e.g. "Yugioh", "Magic", "Pokemon").</param>
        /// <param name="cardId">The cardId primary key to update.</param>
        /// <param name="newAmount">New amount in stock (will be stored as INT).</param>
        public async Task UpdateAmountAsync(string cardGameKey, int cardId, int newAmount)
        {
            if (string.IsNullOrWhiteSpace(cardGameKey))
                throw new ArgumentException("Card game key is required.", nameof(cardGameKey));

            if (!_cardGameToTable.TryGetValue(cardGameKey, out var tableName))
                throw new ArgumentException("Invalid card game selection.", nameof(cardGameKey));

            const string updateTemplate = @"UPDATE {0}
                                            SET amtInStock = @amtInStock
                                            WHERE cardId = @cardId;";

            var updateQuery = string.Format(updateTemplate, tableName);

            // Obtain password on-demand and create connection for the current user.
            var password = await _getPasswordAsync().ConfigureAwait(false);

            using var conn = _connectionFactory.CreateForCurrentUser(password);
            // Clear local password as soon as possible.
            password = string.Empty;

            await conn.OpenAsync().ConfigureAwait(false);

            using var cmd = new SqlCommand(updateQuery, conn);
            cmd.Parameters.Add("@amtInStock", SqlDbType.Int).Value = newAmount;
            cmd.Parameters.Add("@cardId", SqlDbType.Int).Value = cardId;

            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
        }
    }
}
