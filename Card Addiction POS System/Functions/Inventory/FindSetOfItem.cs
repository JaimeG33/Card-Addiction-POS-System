using Card_Addiction_POS_System.Data.SQLServer;
using Card_Addiction_POS_System.Functions.Inventory;
using Card_Addiction_POS_System.Functions.Models;
using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Card_Addiction_POS_System.Functions.Inventory
{
    internal sealed class FindSetOfItem
    {
        private readonly SqlConnectionFactory _connectionFactory;
        private readonly Func<Task<string>> _getPasswordAsync;

        public FindSetOfItem(SqlConnectionFactory connectionFactory, Func<Task<string>> getPasswordAsync)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            _getPasswordAsync = getPasswordAsync ?? throw new ArgumentNullException(nameof(getPasswordAsync));
        }

        /// <summary>
        /// Finds the set name for the provided cardGameId + setId from dbo._Set.
        /// Returns empty string when no match exists.
        /// </summary>
        public async Task<string> FindSetNameAsync(int cardGameId, int setId, CancellationToken ct = default)
        {
            if (cardGameId <= 0) throw new ArgumentOutOfRangeException(nameof(cardGameId));
            if (setId <= 0) throw new ArgumentOutOfRangeException(nameof(setId));

            const string sql = @"
SELECT TOP (1) setName
FROM dbo._Set
WHERE cardGameId = @cardGameId
  AND setId = @setId;";

            var password = await _getPasswordAsync().ConfigureAwait(false);
            using var conn = _connectionFactory.CreateForCurrentUser(password);
            password = string.Empty;

            await conn.OpenAsync(ct).ConfigureAwait(false);

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@cardGameId", SqlDbType.Int).Value = cardGameId;
            cmd.Parameters.Add("@setId", SqlDbType.Int).Value = setId;

            var result = await cmd.ExecuteScalarAsync(ct).ConfigureAwait(false);

            return result == null || result == DBNull.Value
                ? string.Empty
                : Convert.ToString(result) ?? string.Empty;
        }

        /// <summary>
        /// Convenience overload when you already have SelectedCardGameLogic.
        /// </summary>
        public Task<string> FindSetNameAsync(SelectedCardGameLogic selectedGame, int setId, CancellationToken ct = default)
        {
            if (selectedGame == null) throw new ArgumentNullException(nameof(selectedGame));
            return FindSetNameAsync(selectedGame.CardGameId, setId, ct);
        }
    }
}



    // Example Call:
// var setFinder = new FindSetOfItem(connectionFactory, getPasswordAsync);
// string setName = await setFinder.FindSetNameAsync(selectedGameId, selectedSetId);