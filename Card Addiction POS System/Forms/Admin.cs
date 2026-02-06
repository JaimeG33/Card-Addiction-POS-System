using Card_Addiction_POS_System.LogIn.Security;
using Card_Addiction_POS_System.Data.Settings;
using Card_Addiction_POS_System.Data.SQLServer;
using Microsoft.Data.SqlClient;
using Syncfusion.WinForms.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Card_Addiction_POS_System.Security;

namespace Card_Addiction_POS_System.Forms
{
    public partial class Admin : SfForm
    {
        public Admin()
        {
            InitializeComponent();
        }

        private void Admin_Load(object sender, EventArgs e)
        {

        }


        public static class EmployeeAuthRepository
        {
            public static async Task SetEmployeePinAsync(SqlConnection conn, int employeeId, string pin)
            {
                var (salt, hash, iters) = PinHasher.HashPin(pin);

                using var cmd = new SqlCommand(@"
MERGE dbo.EmployeeAuth AS target
USING (SELECT @EmployeeId AS EmployeeId) AS src
ON target.EmployeeId = src.EmployeeId
WHEN MATCHED THEN
    UPDATE SET PinSalt=@Salt, PinHash=@Hash, Iterations=@Iterations, PinUpdatedAt=SYSUTCDATETIME(),
               FailedAttempts=0, LockoutUntilUtc=NULL
WHEN NOT MATCHED THEN
    INSERT (EmployeeId, PinSalt, PinHash, Iterations)
    VALUES (@EmployeeId, @Salt, @Hash, @Iterations);", conn);

                cmd.Parameters.Add("@EmployeeId", SqlDbType.Int).Value = employeeId;
                cmd.Parameters.Add("@Salt", SqlDbType.VarBinary, 16).Value = salt;
                cmd.Parameters.Add("@Hash", SqlDbType.VarBinary, 32).Value = hash;
                cmd.Parameters.Add("@Iterations", SqlDbType.Int).Value = iters;

                await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
        }

        private async void btnNewEpin_Click(object sender, EventArgs e)
        {
            try
            {
                // 1) Read employeeId safely from the IntegerTextBox control
                int employeeId = Convert.ToInt32(integerTextBox1.Text);

                // 2) Read pin (treat as text to allow leading zeros)
                string pin = integerTextBox2.Text?.Trim() ?? "";

                // 3) Validate
                if (employeeId <= 0)
                {
                    MessageBox.Show("Enter a valid EmployeeId.");
                    return;
                }

                // Common POS PIN rule: 4 digits (you can change this)
                if (pin.Length != 4 || !pin.All(char.IsDigit))
                {
                    MessageBox.Show("PIN must be exactly 4 digits.");
                    return;
                }

                // 4) Build connection factory from persisted app settings and create a connection
                var settingsStore = new JsonSettingsStore(AppPaths.SettingsPath);
                var appSettings = settingsStore.Load();
                var connectionFactory = new SqlConnectionFactory(appSettings);

                // Obtain the current user's password securely (in-memory provider used across app)
                var password = await Session.PasswordProvider.GetPasswordAsync().ConfigureAwait(false);

                // Create a connection for the current signed-in user and open it
                using var conn = connectionFactory.CreateForCurrentUser(password);

                // Clear password reference ASAP
                password = string.Empty;

                await conn.OpenAsync().ConfigureAwait(false);

                // 5) Save hashed PIN using async repository method
                await EmployeeAuthRepository.SetEmployeePinAsync(conn, employeeId, pin).ConfigureAwait(false);

                // Inform the user on the UI thread
                if (this.IsHandleCreated && this.InvokeRequired)
                {
                    this.Invoke(() => MessageBox.Show("PIN set successfully."));
                    this.Invoke(() => { integerTextBox2.Text = ""; integerTextBox2.Focus(); });
                }
                else
                {
                    MessageBox.Show("PIN set successfully.");
                    integerTextBox2.Text = "";
                    integerTextBox2.Focus();
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Database error: {ex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }
    }
}
