using System;
using System.Data;
using System.Globalization;
using System.Threading.Tasks;
using Card_Addiction_POS_System.Data.SQLServer;
using Card_Addiction_POS_System.Functions.Models;
using Microsoft.Data.SqlClient;

namespace Card_Addiction_POS_System.Functions.Pricing
{
    /// <summary>
    /// Updates a single inventory row's market price based on user-selected form values.
    /// </summary>
    internal sealed class SetCustomMktPrice_Inventory
    {
        private readonly SqlConnectionFactory _connectionFactory;
        private readonly Func<Task<string>> _getPasswordAsync;

        public SetCustomMktPrice_Inventory(SqlConnectionFactory connectionFactory, Func<Task<string>> getPasswordAsync)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            _getPasswordAsync = getPasswordAsync ?? throw new ArgumentNullException(nameof(getPasswordAsync));
        }

        /// <summary>
        /// Set a custom market price for a single card row and return a user-friendly success message.
        /// </summary>
        public async Task<string> SetCustomPriceAsync(int cardGameId, int cardId, string cardName, decimal newMktPrice)
        {
            if (!SelectedCardGameLogic.TryGetById(cardGameId, out var game))
            {
                throw new ArgumentException("Invalid card game selection.", nameof(cardGameId));
            }

            if (cardId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(cardId), "CardId must be greater than 0.");
            }

            if (newMktPrice < 0m)
            {
                throw new ArgumentOutOfRangeException(nameof(newMktPrice), "Market price cannot be negative.");
            }

            var tableName = string.Concat(game.DatabaseName, "Inventory");

            var sql = $@"UPDATE {tableName}
                         SET mktPrice = @mktPrice,
                             priceUp2Date = @priceUp2Date
                         WHERE cardId = @cardId;";

            var password = await _getPasswordAsync().ConfigureAwait(false);
            using var conn = _connectionFactory.CreateForCurrentUser(password);
            password = string.Empty;

            await conn.OpenAsync().ConfigureAwait(false);

            using var cmd = new SqlCommand(sql, conn);

            var priceParam = new SqlParameter("@mktPrice", SqlDbType.Decimal)
            {
                Precision = 18,
                Scale = 2,
                Value = newMktPrice
            };

            cmd.Parameters.Add(priceParam);
            cmd.Parameters.Add("@priceUp2Date", SqlDbType.Bit).Value = true;
            cmd.Parameters.Add("@cardId", SqlDbType.Int).Value = cardId;

            var rowsAffected = await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            if (rowsAffected <= 0)
            {
                throw new InvalidOperationException("No inventory row was updated. Verify the selected card still exists.");
            }

            return $"Changed the mkt price of {cardName} to {newMktPrice.ToString("C2", CultureInfo.GetCultureInfo("en-US"))} in the {game.DatabaseName} inventory.";
        }
    }
}
