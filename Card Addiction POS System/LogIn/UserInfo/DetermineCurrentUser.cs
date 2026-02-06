using Card_Addiction_POS_System.LogIn.Security;
using Card_Addiction_POS_System.Data.Settings;
using Card_Addiction_POS_System.Data.SQLServer;
using Card_Addiction_POS_System.Security;
using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Card_Addiction_POS_System.LogIn.UserInfo
{
    internal class DetermineCurrentUser
    {
        private readonly SqlConnectionFactory _connectionFactory;
        private readonly Func<Task<string>> _getPasswordAsync;

        // Default ctor kept for backward compatibility with parts of the app that call parameterless.
        // It creates a connection factory from persisted settings and uses the in-memory Session password provider.
        public DetermineCurrentUser()
            : this(new SqlConnectionFactory(new JsonSettingsStore(AppPaths.SettingsPath).Load()),
                  Session.PasswordProvider.GetPasswordAsync)
        {
        }

        public DetermineCurrentUser(SqlConnectionFactory connectionFactory, Func<Task<string>> getPasswordAsync)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            _getPasswordAsync = getPasswordAsync ?? throw new ArgumentNullException(nameof(getPasswordAsync));
        }

        // Backwards-compatible placeholder method (kept because existing code used it).
        // Returns 1 by default; prefer PromptAndVerifyEmployeePinAsync to obtain a real employee id.
        public int employeeId()
        {
            return 1;
        }

        /// <summary>
        /// Shows a modal PIN entry dialog and attempts to find the EmployeeId that matches the provided PIN.
        /// Returns the matched EmployeeId, or null if cancelled or no match found.
        /// </summary>
        public async Task<int?> PromptAndVerifyEmployeePinAsync(IWin32Window? owner = null)
        {
            using var dlg = new PinEntryForm();
            var dr = owner != null ? dlg.ShowDialog(owner) : dlg.ShowDialog();
            if (dr != DialogResult.OK)
                return null;

            var pin = dlg.Pin?.Trim() ?? string.Empty;
            if (pin.Length == 0)
                return null;

            // Obtain password securely and create connection for current signed-in user
            var password = await _getPasswordAsync().ConfigureAwait(false);
            using var conn = _connectionFactory.CreateForCurrentUser(password);
            password = string.Empty; // clear local reference asap

            await conn.OpenAsync().ConfigureAwait(false);

            // Read all auth rows and compare using the PinHasher.
            // This approach allows searching for which employee the entered PIN belongs to.
            using var cmd = new SqlCommand(@"
SELECT EmployeeId, PinSalt, PinHash, Iterations, LockoutUntilUtc
FROM dbo.EmployeeAuth;", conn);

            using var reader = await cmd.ExecuteReaderAsync(CommandBehavior.SequentialAccess).ConfigureAwait(false);
            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                var empId = reader.GetInt32(reader.GetOrdinal("EmployeeId"));
                var salt = (byte[])reader["PinSalt"];
                var hash = (byte[])reader["PinHash"];
                var iterations = (int)reader["Iterations"];

                var lockoutObj = reader["LockoutUntilUtc"];
                var lockoutUntil = lockoutObj == DBNull.Value ? (DateTime?)null : (DateTime)lockoutObj;
                if (lockoutUntil.HasValue && lockoutUntil.Value > DateTime.UtcNow)
                {
                    // this employee is locked out; skip
                    continue;
                }

                if (PinHasher.VerifyPin(pin, salt, hash, iterations))
                {
                    // Reset failed attempts for this employee on success
                    reader.Close();
                    using var reset = new SqlCommand(@"
UPDATE dbo.EmployeeAuth
SET FailedAttempts = 0, LockoutUntilUtc = NULL
WHERE EmployeeId = @EmployeeId;", conn);
                    reset.Parameters.Add("@EmployeeId", SqlDbType.Int).Value = empId;
                    await reset.ExecuteNonQueryAsync().ConfigureAwait(false);

                    return empId;
                }
            }

            // No match found
            return null;
        }

        // Small modal dialog for PIN entry
        private sealed class PinEntryForm : Form
        {
            private readonly TextBox _txt;
            private readonly Button _ok;
            private readonly Button _cancel;

            public string? Pin => _txt.Text;

            public PinEntryForm()
            {
                Text = "Enter Employee PIN";
                FormBorderStyle = FormBorderStyle.FixedDialog;
                StartPosition = FormStartPosition.CenterParent;
                MinimizeBox = false;
                MaximizeBox = false;
                ShowInTaskbar = false;
                Width = 300;
                Height = 150;
                Font = SystemFonts.MessageBoxFont;

                var lbl = new Label
                {
                    Text = "PIN:",
                    Location = new Point(12, 14),
                    AutoSize = true
                };

                _txt = new TextBox
                {
                    Location = new Point(50, 10),
                    Width = 220,
                    PasswordChar = '*',
                    TabIndex = 0
                };

                _ok = new Button
                {
                    Text = "OK",
                    DialogResult = DialogResult.OK,
                    Location = new Point(60, 50),
                    Width = 80,
                    TabIndex = 1
                };

                _cancel = new Button
                {
                    Text = "Cancel",
                    DialogResult = DialogResult.Cancel,
                    Location = new Point(160, 50),
                    Width = 80,
                    TabIndex = 2
                };

                Controls.Add(lbl);
                Controls.Add(_txt);
                Controls.Add(_ok);
                Controls.Add(_cancel);

                AcceptButton = _ok;
                CancelButton = _cancel;
            }
        }
    }
}
