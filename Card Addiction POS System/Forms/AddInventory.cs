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
using Card_Addiction_POS_System.Data.Settings;
using Card_Addiction_POS_System.Data.SQLServer;
using Card_Addiction_POS_System.Functions.Inventory.AddNew;
using Card_Addiction_POS_System.Functions.Models;
using Card_Addiction_POS_System.Security;

namespace Card_Addiction_POS_System.Forms
{
    public partial class AddInventory : SfForm
    {

        public bool IsNavigating { get; set; }
        public virtual string FormTitle { get; set; } = "Add Inventory";

        public AddInventory()
        {
            InitializeComponent();
        }

        private void AddInventory_Load(object sender, EventArgs e)
        {

        }

        private void AddInventory_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (IsNavigating)
            {
                return; // Do nothing if navigating to another form
            }
            else
            {
                Application.Exit(); // Exit the application if not navigating
            }
        }

        private async void btnFetchSets_Click(object sender, EventArgs e)
        {
            // Get the selected game id from the control
            var selectedId = selectCardGameControl1.SelectedCardGameId;
            if (!SelectedCardGameLogic.TryGetById(selectedId, out var game))
            {
                MessageBox.Show("Please select a valid card game.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnFetchSets.Enabled = false;
            var prevCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;

            try
            {
                // Build connection factory from persisted app settings (same approach used elsewhere)
                var settingsStore = new JsonSettingsStore(AppPaths.SettingsPath);
                var appSettings = settingsStore.Load();
                var connectionFactory = new SqlConnectionFactory(appSettings);

                // Create ReleaseCheck with the shared password provider used across the app
                var releaseCheck = new ReleaseCheck(connectionFactory, Session.PasswordProvider.GetPasswordAsync);

                // 1) Fetch groups from TCGCSV and insert into NewCardgameSetTemp (and remove existing-set duplicates)
                await releaseCheck.PopulateNewCardgameSetTempFromTcgCsvAsync(game); // no ConfigureAwait(false)

                // 2) Read the contents of NewCardgameSetTemp for display
                var rows = await releaseCheck.GetNewCardgameSetTempAsync(game.CardGameId);

                // Bind to grid on UI thread
                if (this.IsHandleCreated && this.InvokeRequired)
                {
                    this.Invoke(() => sfDataGrid_NewSets.DataSource = rows);
                }
                else
                {
                    sfDataGrid_NewSets.DataSource = rows;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fetch failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnFetchSets.Enabled = true;
                Cursor.Current = prevCursor;
            }
        }
    }
}
