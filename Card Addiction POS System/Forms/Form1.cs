using System.Windows.Forms;
using Card_Addiction_POS_System.Data.Settings;
using Card_Addiction_POS_System.Data.SQLServer;
using Card_Addiction_POS_System.Forms;
using Card_Addiction_POS_System.Security;


namespace Card_Addiction_POS_System
{
    public partial class Form1 : Form
    {
        private readonly ISettingsStore _settingsStore = new JsonSettingsStore(AppPaths.SettingsPath);
        private AppSettings _settings = AppSettings.Default;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            tbPswd.PasswordChar = '*';

            _settings = _settingsStore.Load();

            if (string.IsNullOrWhiteSpace(_settings.ServerHost))
            {
                if (!PromptForServerSettings(force: true))
                {
                    MessageBox.Show("Server IP/host is required. Exiting.");
                    Close();
                    return;
                }
            }
        }

        private void btnIP_Click(object sender, EventArgs e)
        {
            PromptForServerSettings(force: false);
        }

        private bool PromptForServerSettings(bool force)
        {
            using var dlg = new ServerHostForm(_settings);
            var result = dlg.ShowDialog(this);

            if (result != DialogResult.OK)
                return !force;

            _settings = dlg.Result;
            _settingsStore.Save(_settings);

            return true;
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            var username = tbUser.Text.Trim();
            var password = tbPswd.Text; // don't Trim() passwords

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Username and password are required.");
                return;
            }

            _settings = _settingsStore.Load();
            if (string.IsNullOrWhiteSpace(_settings.ServerHost))
            {
                if (!PromptForServerSettings(force: true))
                    return;
            }

            // Setup Connection Factory to connect to SQL Server DB
            var factory = new SqlConnectionFactory(_settings);

            if (factory.TryOpen(username, password, out var message))
            {
                MessageBox.Show(message);

                // Record current username (used throughout app) and cache password in-memory for the session.
                SqlConnectionFactory.SetCurrentUsername(username);
                Session.PasswordProvider.SetPassword(password);

                // Navigate to home page (other forms will reuse Session.PasswordProvider)
                var home = new HomePage();
                home.Show();
                Hide();
            }
            else
            {
                MessageBox.Show(message);
            }
        }

        private void cbShowPswd_CheckedChanged(object sender, EventArgs e)
        {
            tbPswd.PasswordChar = cbShowPswd.Checked ? '\0' : '*';
        }

        private void tbPswd_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                btnLogin_Click(this, EventArgs.Empty);
        }
    }
}