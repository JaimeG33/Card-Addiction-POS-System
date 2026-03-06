using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Data.SqlClient;
using Card_Addiction_POS_System.Data.SQLServer;
using System.Data;
using Card_Addiction_POS_System.Functions.Models;

//
// SearchInventoryDB - Overview & usage template
// -------------------------------------------------
// Purpose:
// - Encapsulates database access for inventory-related operations (search and price updates).
// - Provides an inner concrete `InventoryService` implementing the public `IInventoryService` interface.
// - The implementation is careful about security (password obtained on-demand), SQL parameterization,
//   and efficient data reading (cached ordinals).
//
// Usage template (call from Forms or other UI code):
// -------------------------------------------------
// // Create a factory that knows how to construct connections for the current user.
// var connectionFactory = new SqlConnectionFactory(/* server/config args as required */);
//
// // Provide a delegate to securely obtain the current user's password on demand.
// // Example: a dialog that prompts the user asynchronously, or a credential manager lookup.
// Func<Task<string>> getPasswordAsync = async () =>
/// {
//     // Prompt user or fetch from secure store
//     return await Task.FromResult("user-password");
// };
//
// // Construct the service
// var inventoryService = new SearchInventoryDB.InventoryService(connectionFactory, getPasswordAsync);
//
// // Example: Search for cards (async call from an async-event handler)
// IReadOnlyList<InventoryItem> items = await inventoryService.SearchInventoryAsync(cardGameId: 1, searchText: "Black Lotus");
//
// // Example: Update price for a card
// await inventoryService.UpdatePriceAsync(cardGameId: 1, cardId: 12345, newPrice: 99.99m);
//
// Notes:
// - Always call these methods from async contexts to avoid blocking UI threads.
// - The password delegate is called inside the method; do not cache the returned password longer than necessary.
// - For DI-friendly usage, wrap InventoryService with your IoC container and supply a secure password provider.
// 
// Detailed comments are included inline in the implementation below to explain specific patterns.
//

namespace Card_Addiction_POS_System.Functions.Inventory
{
    // Move the interface to the namespace level and make it public so it can be used
    // by public types in other assemblies (e.g. the public BuySell constructor).
    public interface IInventoryService
    {
        // NOTE: Changed to accept cardGameId (int). The table name is assembled from
        // the canonical DatabaseName for the game (DatabaseName + "Inventory").
        Task<IReadOnlyList<InventoryItem>> SearchInventoryAsync(
            int cardGameId,
            string searchText);

        // Persist a new market price for a specific card row. Now accepts cardGameId.
        Task UpdatePriceAsync(int cardGameId, int cardId, decimal newPrice);
    }

    internal class SearchInventoryDB
    {
        /// <summary>
        /// InventoryService
        /// - Concrete implementation of IInventoryService backed by SQL Server.
        /// - Designed to be instantiated with a connection factory and a delegate to obtain the
        ///   user's password at call time (avoids long-lived password variables).
        /// - Key implementation details are documented inline near the code that implements them.
        /// </summary>
        public class InventoryService : IInventoryService
        {
            private readonly SqlConnectionFactory _connectionFactory;

            // Delegate used to obtain the current user's password securely at call time.
            // Using a Func<Task<string>> makes it easy to prompt the user or query a secure vault asynchronously.
            private readonly Func<Task<string>> _getPasswordAsync;

            /// <summary>
            /// Constructor
            /// - Accepts a SqlConnectionFactory to centralize connection creation and a getPassword delegate.
            /// - Throws if getPasswordAsync is null so callers must provide a secure method to obtain credentials.
            /// </summary>
            public InventoryService(SqlConnectionFactory connectionFactory, Func<Task<string>> getPasswordAsync)
            {
                _connectionFactory = connectionFactory;
                _getPasswordAsync = getPasswordAsync ?? throw new ArgumentNullException(nameof(getPasswordAsync));
            }

            /// <summary>
            /// SearchInventoryAsync
            /// - Executes a parameterized SQL query to find rows where cardName LIKE @cardName.
            /// - Uses the provided connection factory to create a connection for the current user using a password
            ///   obtained on-demand via the delegate.
            /// - Uses explicit parameter types (SqlDbType.NVarChar) instead of AddWithValue to avoid subtle type inference issues.
            /// - Caches ordinals (GetOrdinal) once per reader which is faster and less error prone than using string-based access repeatedly.
            /// - Converts SQL types to CLR-friendly values and guards against DBNull where appropriate.
            /// 
            /// Behavior changes:
            /// - Accepts a numeric cardGameId (instead of a string key).
            /// - Resolves the card game's canonical DatabaseName via SelectedCardGameLogic.TryGetById and constructs
            ///   the target table name as DatabaseName + "Inventory". Example: "Magic" -> "MagicInventory".
            /// </summary>
            public async Task<IReadOnlyList<InventoryItem>> SearchInventoryAsync(
                int cardGameId,
                string searchText)
            {
                if (!SelectedCardGameLogic.TryGetById(cardGameId, out var game))
                    throw new ArgumentException("Invalid card game selection.", nameof(cardGameId));

                var tableName = string.Concat(game.DatabaseName, "Inventory");

                var baseQuery =
                    $@"SELECT cardName, COALESCE(abbreviation, '') AS abbreviation, rarity, setId, mktPrice, conditionId, amtInStock, amtInCase, 
                     priceUp2Date, imageURL, mktPriceURL, cardId
                    FROM {tableName}
                    WHERE cardName LIKE @cardName
                    ORDER BY cardName;";

                var results = new List<InventoryItem>();

                var password = await _getPasswordAsync().ConfigureAwait(false);
                using var conn = _connection_factory_create_for_current_user_guard(password);
                password = string.Empty;

                await conn.OpenAsync().ConfigureAwait(false);

                using var cmd = new SqlCommand(baseQuery, conn);
                cmd.Parameters.Add("@cardName", SqlDbType.NVarChar, 256).Value = "%" + searchText.Trim() + "%";

                using var reader = await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection).ConfigureAwait(false);

                var ordCardName = reader.GetOrdinal("cardName");
                var ordAbbreviation = reader.GetOrdinal("abbreviation");
                var ordRarity = reader.GetOrdinal("rarity");
                var ordSetId = reader.GetOrdinal("setId");
                var ordMktPrice = reader.GetOrdinal("mktPrice");
                var ordConditionId = reader.GetOrdinal("conditionId");
                var ordAmtInStock = reader.GetOrdinal("amtInStock");
                var ordAmtInCase = reader.GetOrdinal("amtInCase");
                var ordPriceUp2Date = reader.GetOrdinal("priceUp2Date");
                var ordImageUrl = reader.GetOrdinal("imageURL");
                var ordMktPriceUrl = reader.GetOrdinal("mktPriceURL");
                var ordCardId = reader.GetOrdinal("cardId");

                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    var conditionId = !reader.IsDBNull(ordConditionId) ? Convert.ToInt32(reader.GetValue(ordConditionId)) : 0;
                    var amtInStock = !reader.IsDBNull(ordAmtInStock) ? Convert.ToInt32(reader.GetValue(ordAmtInStock)) : 0;
                    var amtInCase = !reader.IsDBNull(ordAmtInCase) ? Convert.ToInt32(reader.GetValue(ordAmtInCase)) : 0;

                    results.Add(new InventoryItem
                    {
                        CardName = reader.IsDBNull(ordCardName) ? string.Empty : reader.GetString(ordCardName),
                        Abbreviation = reader.IsDBNull(ordAbbreviation) ? string.Empty : reader.GetString(ordAbbreviation),
                        Rarity = reader.IsDBNull(ordRarity) ? string.Empty : reader.GetString(ordRarity),
                        SetId = reader.IsDBNull(ordSetId) ? 0 : reader.GetInt32(ordSetId),
                        MktPrice = reader.IsDBNull(ordMktPrice) ? 0m : reader.GetDecimal(ordMktPrice),
                        ConditionId = conditionId,
                        AmtInStock = amtInStock,
                        AMtInCase = amtInCase,
                        PriceUp2Date = !reader.IsDBNull(ordPriceUp2Date) && reader.GetBoolean(ordPriceUp2Date),
                        ImageUrl = reader.IsDBNull(ordImageUrl) ? null : reader.GetString(ordImageUrl),
                        MktPriceUrl = reader.IsDBNull(ordMktPriceUrl) ? null : reader.GetString(ordMktPriceUrl),
                        CardId = reader.IsDBNull(ordCardId) ? 0 : reader.GetInt32(ordCardId)
                    });
                }

                return results;
            }

            /// <summary>
            /// UpdatePriceAsync
            /// - Updates the mktPrice column for a specified card row in the game-specific table.
            /// - Uses parameterized SQL and a SqlParameter for decimal with explicit precision and scale to avoid
            ///   mismatches with server-side types (e.g. smallmoney).
            /// - Accepts cardGameId and resolves DatabaseName -> TableName as DatabaseName + "Inventory".
            /// </summary>
            public async Task UpdatePriceAsync(int cardGameId, int cardId, decimal newPrice)
            {
                if (!SelectedCardGameLogic.TryGetById(cardGameId, out var game))
                    throw new ArgumentException("Invalid card game selection.", nameof(cardGameId));

                var tableName = string.Concat(game.DatabaseName, "Inventory");

                // Use parameterized update to avoid SQL injection
                var updateQuery = $@"UPDATE {tableName}
                                     SET mktPrice = @mktPrice
                                     WHERE cardId = @cardId;";

                // Obtain password and open connection
                var password = await _getPassword_async_guard().ConfigureAwait(false);
                using var conn = _connection_factory_create_for_current_user_guard(password);
                password = string.Empty;

                await conn.OpenAsync().ConfigureAwait(false);

                using var cmd = new SqlCommand(updateQuery, conn);
                
                // Use a SqlParameter for decimal with precision/scale to match smallmoney/similar server types.
                var param = new SqlParameter("@mktPrice", SqlDbType.Decimal)
                {
                    Precision = 18,
                    Scale = 2,
                    Value = newPrice
                };
                cmd.Parameters.Add(param);
                cmd.Parameters.Add("@cardId", SqlDbType.Int).Value = cardId;

                await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            }

            // Small local helpers to keep the common factory call behaviors in one place.
            private SqlConnection _connection_factory_create_for_current_user_guard(string password)
            {
                return _connectionFactory.CreateForCurrentUser(password);
            }

            private Task<string> _getPassword_async_guard()
            {
                return _getPasswordAsync();
            }
        }
    }
}
