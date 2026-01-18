using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Data.SqlClient;
using Card_Addiction_POS_System.Data.SQLServer;
using System.Data;
using Card_Addiction_POS_System.Functions.Models;

namespace Card_Addiction_POS_System.Functions.Inventory
{
    // Move the interface to the namespace level and make it public so it can be used
    // by public types in other assemblies (e.g. the public BuySell constructor).
    public interface IInventoryService
    {
        Task<IReadOnlyList<InventoryItem>> SearchInventoryAsync(
            string cardGameKey,
            string searchText);
    }

    internal class SearchInventoryDB
    {
        public class InventoryService : IInventoryService
        {
            private readonly SqlConnectionFactory _connectionFactory;
            // Delegate used to obtain the current user's password securely at call time.
            private readonly Func<Task<string>> _getPasswordAsync;

            private static readonly Dictionary<string, string> _cardGameToTable = new()
            {
                ["Yugioh"] = "YugiohInventory",
                ["Magic"] = "MagicInventory",
                ["Pokemon"] = "PokemonInventory"
            };

            public InventoryService(SqlConnectionFactory connectionFactory, Func<Task<string>> getPasswordAsync)
            {
                _connectionFactory = connectionFactory;
                _getPasswordAsync = getPasswordAsync ?? throw new ArgumentNullException(nameof(getPasswordAsync));
            }

            public async Task<IReadOnlyList<InventoryItem>> SearchInventoryAsync(
                string cardGameKey,
                string searchText)
            {
                if (!_cardGameToTable.TryGetValue(cardGameKey, out var tableName))
                    throw new ArgumentException("Invalid card game selection.", nameof(cardGameKey));

                const string baseQuery =
                    @"SELECT cardName, rarity, setId, mktPrice, conditionId, amtInStock, 
                     priceUp2Date, imageURL, mktPriceURL, cardId
                    FROM {0}
                    WHERE cardName LIKE @cardName
                    ORDER BY cardName;";

                var query = string.Format(baseQuery, tableName);

                var results = new List<InventoryItem>();

                // Obtain password on-demand (e.g. from prompt, credential manager, vault)
                var password = await _getPasswordAsync().ConfigureAwait(false);

                // Create connection for the current user using the password provided at runtime.
                using var conn = _connectionFactory.CreateForCurrentUser(password);

                // Optionally clear local password reference as soon as practical:
                password = string.Empty;

                await conn.OpenAsync().ConfigureAwait(false);

                using var cmd = new SqlCommand(query, conn);
                // Use explicit parameter type to avoid AddWithValue pitfalls
                cmd.Parameters.Add("@cardName", SqlDbType.NVarChar, 256).Value = "%" + searchText.Trim() + "%";

                using var reader = await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection).ConfigureAwait(false);

                // Cache ordinals for performance and clarity
                var ordCardName = reader.GetOrdinal("cardName");
                var ordRarity = reader.GetOrdinal("rarity");
                var ordSetId = reader.GetOrdinal("setId");
                var ordMktPrice = reader.GetOrdinal("mktPrice");
                var ordConditionId = reader.GetOrdinal("conditionId");
                var ordAmtInStock = reader.GetOrdinal("amtInStock");
                var ordPriceUp2Date = reader.GetOrdinal("priceUp2Date");
                var ordImageUrl = reader.GetOrdinal("imageURL");
                var ordMktPriceUrl = reader.GetOrdinal("mktPriceURL");
                var ordCardId = reader.GetOrdinal("cardId");

                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    // tinyint -> byte in CLR; cast to int for your model if needed
                    var conditionId = !reader.IsDBNull(ordConditionId) ? (int)reader.GetByte(ordConditionId) : 0;
                    var amtInStock = !reader.IsDBNull(ordAmtInStock) ? (int)reader.GetByte(ordAmtInStock) : 0;

                    results.Add(new InventoryItem
                    {
                        CardName = reader.IsDBNull(ordCardName) ? string.Empty : reader.GetString(ordCardName),
                        Rarity = reader.IsDBNull(ordRarity) ? string.Empty : reader.GetString(ordRarity),
                        SetId = reader.IsDBNull(ordSetId) ? 0 : reader.GetInt16(ordSetId),
                        MktPrice = reader.IsDBNull(ordMktPrice) ? 0m : reader.GetDecimal(ordMktPrice),
                        ConditionId = conditionId,
                        AmtInStock = amtInStock,
                        PriceUp2Date = !reader.IsDBNull(ordPriceUp2Date) && reader.GetBoolean(ordPriceUp2Date),
                        ImageUrl = reader.IsDBNull(ordImageUrl) ? null : reader.GetString(ordImageUrl),
                        MktPriceUrl = reader.IsDBNull(ordMktPriceUrl) ? null : reader.GetString(ordMktPriceUrl),
                        CardId = reader.IsDBNull(ordCardId) ? 0 : reader.GetInt32(ordCardId)
                    });
                }

                return results;
            }
        }
    }
}
