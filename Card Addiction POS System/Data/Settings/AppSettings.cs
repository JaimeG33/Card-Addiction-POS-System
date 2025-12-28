using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Card_Addiction_POS_System.Data.Settings
{
    /// <summary>
    /// App settings stored on the local machine (per Windows user).
    /// Does NOT store SQL passwords.
    /// </summary>
    public sealed record AppSettings(
        string ServerHost,
        int Port,
        string Database,
        bool Encrypt,
        bool TrustServerCertificate
    )
    {
        public static AppSettings Default => new(
            ServerHost: "",
            Port: 1433,
            Database: "Revised Demo Database CAv2",
            Encrypt: true,
            TrustServerCertificate: true // set to false when you install a proper server cert
        );
    }
}
