using Syncfusion.WinForms.Controls;
using Syncfusion.WinForms.DataGrid;
using Syncfusion.WinForms.DataGrid.Enums;
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

        // new fields to track batch counts
        private int setsFound = 0;
        private int maxBatchSize = 0;
        private int currentBatch = 0;
        // keep a copy of rows for counting / reflection-safe access
        private List<object> _newSetsRows = new();

        public bool IsNavigating { get; set; }
        public virtual string FormTitle { get; set; } = "Add Inventory";

        public AddInventory()
        {
            InitializeComponent();

            // Configure the small grid used to show new sets (checkbox first, set name emphasized)
            ConfigureNewSetsGrid();
        }

        private void ConfigureNewSetsGrid()
        {
            // Prevent auto-generation so we control order/width
            sfDataGrid_NewSets.AutoGenerateColumns = false;
            sfDataGrid_NewSets.AllowSorting = true;
            sfDataGrid_NewSets.AutoSizeColumnsMode = AutoSizeColumnsMode.Fill;
            sfDataGrid_NewSets.Columns.Clear();

            // 1) includeInBatch checkbox - leftmost
            sfDataGrid_NewSets.Columns.Add(new GridCheckBoxColumn
            {
                MappingName = "IncludeInBatch",
                HeaderText = "Include",
                Width = 80,
                MinimumWidth = 60
            });

            // 2) Set name - larger width
            sfDataGrid_NewSets.Columns.Add(new GridTextColumn
            {
                MappingName = "SetName",
                HeaderText = "Set Name",
                Width = 400,
                MinimumWidth = 200
            });

            // 3) Other columns with small weights - let them auto-size to content/header
            sfDataGrid_NewSets.Columns.Add(new GridNumericColumn
            {
                MappingName = "SetId",
                HeaderText = "Set Id",
                Width = 1,
                MinimumWidth = 60,
                NumberFormatInfo = new System.Globalization.NumberFormatInfo { NumberDecimalDigits = 0, NumberGroupSeparator = string.Empty }
            });

            sfDataGrid_NewSets.Columns.Add(new GridTextColumn
            {
                MappingName = "Abbreviation",
                HeaderText = "Abbrev",
                Width = 1,
                MinimumWidth = 60
            });

            sfDataGrid_NewSets.Columns.Add(new GridDateTimeColumn
            {
                MappingName = "SetDate",
                HeaderText = "Date",
                Width = 1,
                MinimumWidth = 80,
                Format = "d"
            });

            sfDataGrid_NewSets.Columns.Add(new GridCheckBoxColumn
            {
                MappingName = "Approved",
                HeaderText = "Approved",
                Width = 1,
                MinimumWidth = 70
            });

            // keep any other technical columns hidden (optional)
            sfDataGrid_NewSets.Columns.Add(new GridTextColumn { MappingName = "SetDesc", Visible = false });
            sfDataGrid_NewSets.Columns.Add(new GridNumericColumn { MappingName = "CardGameId", Visible = false });
            sfDataGrid_NewSets.Columns.Add(new GridTextColumn { MappingName = "IssueNotes", Visible = false });

            // Wire checkbox click handler (already wired in designer but ensure handler exists)
            sfDataGrid_NewSets.CellCheckBoxClick += sfDataGrid_NewSets_CellCheckBoxClick;
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
                var setsCount = await releaseCheck.PopulateNewCardgameSetTempFromTcgCsvAsync(game); // returns int setsFound
                setsFound = setsCount;
                // update UI textbox to show sets found (max batch size default)
                maxBatchSize = setsFound;
                // keep a default currentBatch equal to the text box value (will set below)
                currentBatch = setsFound;

                // 2) Read the contents of NewCardgameSetTemp for display
                var rows = await releaseCheck.GetNewCardgameSetTempAsync(game.CardGameId);

                // Keep a copy for counting/checking via reflection
                _newSetsRows = rows.Cast<object>().ToList();

                // Ensure integerTextBox shows setsFound and respects limits
                if (setsFound < 0) setsFound = 0;
                // integerTextBox has IntegerValue property (long)
                integerTextBox_MBS.IntegerValue = setsFound;
                integerTextBox_MBS.Text = setsFound.ToString();
                // Also set the label to show maxBatchSize
                lblMaxBatchSize.Text = setsFound.ToString();

                // If there are fewer includeInBatch rows than setsFound, sync maxBatchSize
                var includeCount = _newSetsRows.Count(r =>
                {
                    var p = r.GetType().GetProperty("IncludeInBatch");
                    if (p == null) return false;
                    return (bool)(p.GetValue(r) ?? false);
                });
                maxBatchSize = includeCount;
                lblMaxBatchSize.Text = maxBatchSize.ToString();
                if (integerTextBox_MBS.IntegerValue > maxBatchSize)
                {
                    integerTextBox_MBS.IntegerValue = maxBatchSize;
                    integerTextBox_MBS.Text = maxBatchSize.ToString();
                }

                currentBatch = (int)integerTextBox_MBS.IntegerValue;

                // 3) Bind to grid on UI thread
                if (this.IsHandleCreated && this.InvokeRequired)
                {
                    this.Invoke(() => sfDataGrid_NewSets.DataSource = rows);
                }
                else
                {
                    sfDataGrid_NewSets.DataSource = rows;
                }

                // 4) Notify user with sets found & next steps
                MessageBox.Show($"{setsFound} sets were identified. Select the sets you wish to add to the database, then press 'Scan Items' when ready.", "Sets Identified", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

        private void integerTextBox_MBS_TextChanged(object sender, EventArgs e)
        {
            // when user edits the numeric box, ensure it doesn't exceed maxBatchSize and update currentBatch
            try
            {
                var val = Convert.ToInt32(integerTextBox_MBS.IntegerValue);
                if (val > maxBatchSize)
                {
                    integerTextBox_MBS.IntegerValue = maxBatchSize;
                    integerTextBox_MBS.Text = maxBatchSize.ToString();
                    currentBatch = maxBatchSize;
                }
                else
                {
                    currentBatch = val;
                }
            }
            catch
            {
                // ignore parse issues, keep previous currentBatch
            }
        }

        private void sfDataGrid_NewSets_CellCheckBoxClick(object sender, Syncfusion.WinForms.DataGrid.Events.CellCheckBoxClickEventArgs e)
        {
            // Checkbox click occurs *before* the underlying data is updated by the grid control.
            // Defer the count operation until after the UI processes the click and updates the bound data.
            // Use BeginInvoke to queue the update after the current UI message completes.
            this.BeginInvoke(new Action(() =>
            {
                try
                {
                    if (_newSetsRows == null) return;

                    // Re-read the datasource to get updated checkbox states
                    var currentDataSource = sfDataGrid_NewSets.DataSource;
                    if (currentDataSource != null)
                    {
                        _newSetsRows = ((System.Collections.IEnumerable)currentDataSource).Cast<object>().ToList();
                    }

                    var includeCount = _newSetsRows.Count(r =>
                    {
                        var p = r.GetType().GetProperty("IncludeInBatch");
                        if (p == null) return false;
                        return (bool)(p.GetValue(r) ?? false);
                    });

                    maxBatchSize = includeCount;

                    // Update label to show new max batch size
                    lblMaxBatchSize.Text = maxBatchSize.ToString();

                    // If the numeric textbox value is higher than the new max, clamp it
                    if (integerTextBox_MBS.IntegerValue > maxBatchSize)
                    {
                        integerTextBox_MBS.IntegerValue = maxBatchSize;
                        integerTextBox_MBS.Text = maxBatchSize.ToString();
                    }

                    // Update currentBatch to reflect the numeric box
                    currentBatch = Convert.ToInt32(integerTextBox_MBS.IntegerValue);
                }
                catch
                {
                    // swallow; UI shouldn't crash from checkbox toggles
                }
            }));
        }

        private void sfDataGrid_NewSets_Click(object sender, EventArgs e)
        {

        }

        private void sfDataGrid_NewSets_CellClick(object sender, Syncfusion.WinForms.DataGrid.Events.CellClickEventArgs e)
        {

        }

        private void sfDataGrid_NewSets_CellCheckBoxClickHandler(object sender, Syncfusion.WinForms.DataGrid.Events.CellCheckBoxClickEventArgs e)
        {
            // kept for compatibility if another handler is wired; ensure single behavior in main handler above
            sfDataGrid_NewSets_CellCheckBoxClick(sender, e);
        }
    }
}
