using Card_Addiction_POS_System.Forms;
using System;
using System.Threading.Tasks;
using Card_Addiction_POS_System.Data.SQLServer;
using Microsoft.Data.SqlClient;

namespace Card_Addiction_POS_System.Functions.Inventory.AddNew
{
    internal sealed class EditsMadeToInventoryTable
    {
        private readonly SqlConnectionFactory _connectionFactory;
        private readonly Func<Task<string>> _getPasswordAsync;

        public EditsMadeToInventoryTable(SqlConnectionFactory connectionFactory, Func<Task<string>> getPasswordAsync)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            _getPasswordAsync = getPasswordAsync ?? throw new ArgumentNullException(nameof(getPasswordAsync));
        }

        public async Task UpdateInventoryRowAsync(AddInventory.NewTempCardgameInventory_SelectedRow row)
        {
            if (row == null) throw new ArgumentNullException(nameof(row));
            if (row.ActualTempId <= 0) throw new ArgumentOutOfRangeException(nameof(row.ActualTempId));

            var password = await _getPasswordAsync().ConfigureAwait(false);
            using var conn = _connectionFactory.CreateForCurrentUser(password);
            password = string.Empty;
            await conn.OpenAsync().ConfigureAwait(false);

            const string sql = @"
UPDATE dbo.NewTempCardgameInventory
SET
    cardName    = @cardName,
    rarity      = @rarity,
    foil        = @foil,
    imageURL    = @imageURL,
    mktPriceURL = @mktPriceURL,
    mktPrice    = @mktPrice,
    amtInStock  = @amtInStock,
    approved    = @approved,
    needsReview = @needsReview,
    issueNotes  = @issueNotes
WHERE tempId = @tempId AND cardGameId = @cardGameId;";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@tempId", row.ActualTempId);
            cmd.Parameters.AddWithValue("@cardGameId", row.CardGameId);
            cmd.Parameters.AddWithValue("@cardName", (object?)row.CardName ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@rarity", (object?)row.Rarity ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@foil", (object?)row.Foil ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@imageURL", (object?)row.ImageUrl ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@mktPriceURL", (object?)row.MktPriceUrl ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@mktPrice", (object?)row.MktPrice ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@amtInStock", (object?)row.AmtInStock ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@approved", (object?)row.Approved ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@needsReview", row.NeedsReview);
            cmd.Parameters.AddWithValue("@issueNotes", (object?)row.IssueNotes ?? DBNull.Value);

            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
        }
    }
}
