using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Card_Addiction_POS_System.Data.Settings;

namespace Card_Addiction_POS_System.Data.SQLServer
{
    /// <summary>
    /// Creates SqlConnections based on saved server settings + user-entered credentials.
    /// </summary>
    public sealed class SqlConnectionFactory
    {
        private readonly AppSettings _settings;

        public SqlConnectionFactory(AppSettings settings)
        {
            _settings = settings;
        }

        public string BuildConnectionString(string username, string password)
        {
            var builder = new SqlConnectionStringBuilder
            {
                DataSource = $"{_settings.ServerHost},{_settings.Port}",
                InitialCatalog = _settings.Database,
                UserID = username,
                Password = password,
                IntegratedSecurity = false,
                Encrypt = _settings.Encrypt,
                TrustServerCertificate = _settings.TrustServerCertificate,
                ConnectTimeout = 5,
                Pooling = true
            };

            return builder.ConnectionString;
        }

        public SqlConnection Create(string username, string password)
        {
            var cs = BuildConnectionString(username, password);
            return new SqlConnection(cs);
        }

        public bool TryOpen(string username, string password, out string message)
        {
            try
            {
                using var conn = Create(username, password);
                conn.Open();
                message = $"Connected successfully as {username}.";
                return true;
            }
            catch (SqlException ex)
            {
                message = $"Connection failed: {ex.Message}";
                return false;
            }
            catch (Exception ex)
            {
                message = $"Unexpected error: {ex.Message}";
                return false;
            }
        }
    }
}
