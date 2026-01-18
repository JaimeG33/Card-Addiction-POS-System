using System;
using System.Windows.Forms;
using Syncfusion.WinForms.Controls;
using Card_Addiction_POS_System.Data.Settings;
using Card_Addiction_POS_System.Data.SQLServer;
using Card_Addiction_POS_System.Functions.Inventory;
using Card_Addiction_POS_System.Security;

namespace Card_Addiction_POS_System.Forms
{
    public partial class HomePage : SfForm
    {
        // This is the home page used to navigate to other forms.
        public bool IsNavigating { get; set; }
        public virtual string FormTitle { get; set; } = "Home Page";
        public HomePage()
        {
            InitializeComponent();
        }

        private void HomePage_Load(object sender, EventArgs e)
        {

        }

        private void HomePage_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (IsNavigating)
            {
                return; // Do nothing if navigating to another form
            }
            else
            {
                // Clear session on application exit
                Session.Clear();
                Application.Exit(); // Exit the application if not navigating
            }
        }

        private void btnSale_Click(object sender, EventArgs e)
        {
            IsNavigating = true;

            // Load saved server settings (note: AppSettings does NOT store SQL passwords)
            var settingsStore = new JsonSettingsStore(AppPaths.SettingsPath);
            var appSettings = settingsStore.Load();

            // Create the SqlConnectionFactory using those settings
            var connectionFactory = new SqlConnectionFactory(appSettings);

            // Reuse the password stored at login via Session.PasswordProvider
            var inventoryService = new SearchInventoryDB.InventoryService(connectionFactory, Session.PasswordProvider.GetPasswordAsync);

            var posForm = new BuySell(inventoryService);

            // Ensure password cleared when leaving sales
            posForm.FormClosed += (s, args) => { /* optionally clear here if you log out from BuySell */ };

            posForm.Show();
            this.Close();  // BaseForm will see IsNavigating == true and NOT exit app
        }
    }
}
