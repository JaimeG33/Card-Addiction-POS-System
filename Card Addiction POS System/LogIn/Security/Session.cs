using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Card_Addiction_POS_System.Data.SQLServer;

namespace Card_Addiction_POS_System.Security
{
    /// <summary>
    /// Lightweight session holder for the current user password provider and helpers.
    /// Password lives in-memory only and should be cleared on logout/exit.
    /// </summary>
    public static class Session
    {
        public static PasswordProvider PasswordProvider { get; } = new PasswordProvider();

        public static void Clear()
        {
            PasswordProvider.Clear();
            SqlConnectionFactory.ClearCurrentUsername();
        }
    }
}
