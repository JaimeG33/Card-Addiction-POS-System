using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Card_Addiction_POS_System.Data.Settings;
using System.IO;

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

        // Shared "current username" for the running application. HeaderControl reads this.
        public static string? CurrentUsername { get; private set; }

        // Event raised when the current username changes.
        public static event EventHandler<UserChangedEventArgs>? CurrentUsernameChanged;

        public static void SetCurrentUsername(string? username)
        {
            CurrentUsername = username;
            CurrentUsernameChanged?.Invoke(null, new UserChangedEventArgs(username));
        }

        public static void ClearCurrentUsername()
        {
            SetCurrentUsername(null);
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

        /// <summary>
        /// Create a connection for the currently signed-in user. Caller must supply the user's password
        /// from a secure source (prompt, credential manager, vault). Throws if no current username is set.
        /// </summary>
        public SqlConnection CreateForCurrentUser(string password)
        {
            if (string.IsNullOrEmpty(CurrentUsername))
                throw new InvalidOperationException("No current username is set. Call SetCurrentUsername after successful login.");

            if (password is null)
                throw new ArgumentNullException(nameof(password));

            return Create(CurrentUsername, password);
        }

        public bool TryOpen(string username, string password, out string message)
        {
            try
            {
                using var conn = Create(username, password);
                conn.Open();

                SetCurrentUsername(username);

                message = $"Connected successfully as {username}.";
                return true;
            }
            catch (Exception ex)
            {
                var diagnosticsHeader =
                    $"Host: {_settings.ServerHost}\n" +
                    $"Port: {_settings.Port}\n" +
                    $"Database: {_settings.Database}\n" +
                    $"Encrypt: {_settings.Encrypt}\n" +
                    $"TrustServerCertificate: {_settings.TrustServerCertificate}\n" +
                    $"User: {username}\n";

                var details = $"{diagnosticsHeader}\n{ex}";

                var logDir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "Card_Addiction_POS_System",
                    "Logs"
                );
                Directory.CreateDirectory(logDir);

                var logPath = Path.Combine(logDir, $"sql_error_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
                File.WriteAllText(logPath, details, Encoding.UTF8);

                ErrorDialog.Show(owner: null, title: "Database connection error", details: details);

                message = $"Connection failed. Details saved to:\n{logPath}";
                return false;
            }
        }
    }

    public sealed class UserChangedEventArgs : EventArgs
    {
        public string? Username { get; }

        public UserChangedEventArgs(string? username)
        {
            Username = username;
        }
    }
}
