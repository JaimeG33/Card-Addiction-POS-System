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
using Microsoft.Data.SqlClient;

namespace Card_Addiction_POS_System.Forms
{
    public partial class AddInventory : SfForm
    {
        // new fields to track batch counts
        private int setsFound = 0;
        private int maxBatchSize = 0;
        public int currentBatch = 1;
        // keep a copy of rows for counting / reflection-safe access
        private List<object> _newSetsRows = new();
        // store current cardGameId for checkbox operations
        private int _currentCardGameId = -1;

        // Navigation tools 
        public int selectedBatch { get; set; }
        public int selectedSetId { get; set; }
        public string selectedSetName { get; set; }
        public int displayedBatch { get; set; }
        public int displayedSetId { get; set; }
        public string displayedSetName { get; set; }
        public bool readyForItems = false;

        // Map displayed TempId -> actual DB tempId for inventory grid
        private readonly Dictionary<int, int> _invDisplayToActualTempId = new();

        // Selected inventory row snapshot
        private NewTempCardgameInventory_SelectedRow? _selectedInventoryRow;

        public bool IsNavigating { get; set; }
        public virtual string FormTitle { get; set; } = "Add Inventory";

        // Issue tracking for highlighting
        private ErrorIdentifyerHelper.InventoryIssueResult _invIssueResult = new();
        private SelectedCardGameLogic? _currentGame;

        public AddInventory()
        {
            InitializeComponent();
            ConfigureNewSetsGrid();
            ConfigureNewInvGrid();

            sfDataGrid_NewInv.QueryCellStyle += SfDataGrid_NewInv_QueryCellStyle;
            sfDataGrid_NewSets.QueryRowStyle += SfDataGrid_NewSets_QueryRowStyle;
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

            // 2) TempId - small numeric column
            sfDataGrid_NewSets.Columns.Add(new GridNumericColumn
            {
                MappingName = "TempId",
                HeaderText = "#",
                Width = 50,
                MinimumWidth = 40,
                NumberFormatInfo = new System.Globalization.NumberFormatInfo { NumberDecimalDigits = 0, NumberGroupSeparator = string.Empty }
            });

            // 3) Set name - larger width
            sfDataGrid_NewSets.Columns.Add(new GridTextColumn
            {
                MappingName = "SetName",
                HeaderText = "Set Name",
                Width = 400,
                MinimumWidth = 200
            });

            // 4) Other columns with small weights - let them auto-size to content/header
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

            // NOTE: CellCheckBoxClick is already wired in the designer; do not wire it again here.
        }

        private void ConfigureNewInvGrid()
        {
            sfDataGrid_NewInv.AutoGenerateColumns = false;
            sfDataGrid_NewInv.AllowSorting = true;
            sfDataGrid_NewInv.AutoSizeColumnsMode = AutoSizeColumnsMode.Fill;
            sfDataGrid_NewInv.Columns.Clear();

            sfDataGrid_NewInv.Columns.Add(new GridNumericColumn
            {
                MappingName = "TempId",
                HeaderText = "#",
                Width = 50,
                MinimumWidth = 50,
                NumberFormatInfo = new System.Globalization.NumberFormatInfo { NumberDecimalDigits = 0, NumberGroupSeparator = string.Empty }
            });

            sfDataGrid_NewInv.Columns.Add(new GridNumericColumn
            {
                MappingName = "BatchPosition",
                HeaderText = "Batch",
                Width = 70,
                MinimumWidth = 60,
                NumberFormatInfo = new System.Globalization.NumberFormatInfo { NumberDecimalDigits = 0, NumberGroupSeparator = string.Empty }
            });

            sfDataGrid_NewInv.Columns.Add(new GridNumericColumn
            {
                MappingName = "SetId",
                HeaderText = "Set Id",
                Width = 80,
                MinimumWidth = 70,
                NumberFormatInfo = new System.Globalization.NumberFormatInfo { NumberDecimalDigits = 0, NumberGroupSeparator = string.Empty }
            });

            sfDataGrid_NewInv.Columns.Add(new GridTextColumn
            {
                MappingName = "CardName",
                HeaderText = "Card Name",
                Width = 320,
                MinimumWidth = 220
            });

            sfDataGrid_NewInv.Columns.Add(new GridTextColumn
            {
                MappingName = "Abbreviation",
                HeaderText = "Abbrev",
                Width = 120,
                MinimumWidth = 90
            });

            sfDataGrid_NewInv.Columns.Add(new GridTextColumn
            {
                MappingName = "Rarity",
                HeaderText = "Rarity",
                Width = 120,
                MinimumWidth = 90
            });

            sfDataGrid_NewInv.Columns.Add(new GridTextColumn
            {
                MappingName = "Foil",
                HeaderText = "Foil",
                Width = 80,
                MinimumWidth = 70
            });

            sfDataGrid_NewInv.Columns.Add(new GridTextColumn
            {
                MappingName = "ImageUrl",
                HeaderText = "Image URL",
                Width = 160,
                MinimumWidth = 140
            });

            sfDataGrid_NewInv.Columns.Add(new GridTextColumn
            {
                MappingName = "MktPriceUrl",
                HeaderText = "Market URL",
                Width = 160,
                MinimumWidth = 140
            });

            sfDataGrid_NewInv.Columns.Add(new GridNumericColumn
            {
                MappingName = "MktPrice",
                HeaderText = "Price",
                Width = 80,
                MinimumWidth = 70,
                NumberFormatInfo = new System.Globalization.NumberFormatInfo { NumberDecimalDigits = 2, NumberGroupSeparator = string.Empty }
            });

            sfDataGrid_NewInv.Columns.Add(new GridNumericColumn
            {
                MappingName = "AmtInStock",
                HeaderText = "Stock",
                Width = 70,
                MinimumWidth = 60,
                NumberFormatInfo = new System.Globalization.NumberFormatInfo { NumberDecimalDigits = 0, NumberGroupSeparator = string.Empty }
            });

            sfDataGrid_NewInv.Columns.Add(new GridCheckBoxColumn
            {
                MappingName = "Approved",
                HeaderText = "Approved",
                Width = 80,
                MinimumWidth = 70
            });

            sfDataGrid_NewInv.Columns.Add(new GridCheckBoxColumn
            {
                MappingName = "NeedsReview",
                HeaderText = "Needs Review",
                Width = 100,
                MinimumWidth = 90
            });

            sfDataGrid_NewInv.Columns.Add(new GridTextColumn
            {
                MappingName = "IssueNotes",
                HeaderText = "Issues",
                Width = 150,
                MinimumWidth = 130
            });

            sfDataGrid_NewInv.Columns.Add(new GridDateTimeColumn
            {
                MappingName = "DateInserted",
                HeaderText = "Inserted",
                Width = 130,
                MinimumWidth = 110,
                Format = "g"
            });

            // Hidden technical columns
            sfDataGrid_NewInv.Columns.Add(new GridNumericColumn { MappingName = "CardGameId", Visible = false });
            sfDataGrid_NewInv.Columns.Add(new GridNumericColumn { MappingName = "CardId", Visible = false });
        }

        private static List<AddNewYugiohInventory.NewTempCardgameInventoryRow> RenumberInventoryRowsForDisplay(
            IEnumerable<AddNewYugiohInventory.NewTempCardgameInventoryRow> rows,
            Dictionary<int, int> displayToActual)
        {
            displayToActual.Clear();

            var ordered = rows
                .OrderBy(r => r.BatchPosition)
                .ThenBy(r => r.TempId)
                .ToList();

            var result = new List<AddNewYugiohInventory.NewTempCardgameInventoryRow>(ordered.Count);
            int idx = 1;
            foreach (var r in ordered)
            {
                displayToActual[idx] = r.TempId; // map display -> actual DB tempId

                result.Add(new AddNewYugiohInventory.NewTempCardgameInventoryRow
                {
                    TempId = idx++,                // display id starting at 1
                    BatchPosition = r.BatchPosition,
                    CardGameId = r.CardGameId,
                    SetId = r.SetId,
                    CardId = r.CardId,
                    CardName = r.CardName,
                    Abbreviation = r.Abbreviation,
                    Rarity = r.Rarity,
                    Foil = r.Foil,
                    ImageUrl = r.ImageUrl,
                    MktPriceUrl = r.MktPriceUrl,
                    MktPrice = r.MktPrice,
                    AmtInStock = r.AmtInStock,
                    Approved = r.Approved,
                    NeedsReview = r.NeedsReview,
                    IssueNotes = r.IssueNotes,
                    DateInserted = r.DateInserted
                });
            }

            return result;
        }

        private async Task<List<AddNewYugiohInventory.NewTempCardgameInventoryRow>> LoadInventoryBatchAsync(int batchPosition, CancellationToken ct = default)
        {
            var results = new List<AddNewYugiohInventory.NewTempCardgameInventoryRow>();
            if (_currentCardGameId <= 0 || batchPosition <= 0)
            {
                return results;
            }

            var settingsStore = new JsonSettingsStore(AppPaths.SettingsPath);
            var appSettings = settingsStore.Load();
            var connectionFactory = new SqlConnectionFactory(appSettings);

            var password = await Session.PasswordProvider.GetPasswordAsync().ConfigureAwait(false);
            using var conn = connectionFactory.CreateForCurrentUser(password);
            password = string.Empty;
            await conn.OpenAsync(ct).ConfigureAwait(false);

            const string sql = @"
SELECT tempId, batchPosition, cardGameId, setId, cardId, cardName, abbreviation, rarity, foil, imageURL, mktPriceURL, mktPrice, amtInStock, approved, needsReview, issueNotes, dateInserted
FROM dbo.NewTempCardgameInventory
WHERE cardGameId = @cardGameId AND batchPosition = @batchPosition
ORDER BY tempId;";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@cardGameId", SqlDbType.Int).Value = _currentCardGameId;
            cmd.Parameters.Add("@batchPosition", SqlDbType.Int).Value = batchPosition;

            using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
            while (await reader.ReadAsync(ct).ConfigureAwait(false))
            {
                results.Add(new AddNewYugiohInventory.NewTempCardgameInventoryRow
                {
                    TempId = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                    BatchPosition = reader.IsDBNull(1) ? 0 : reader.GetInt32(1),
                    CardGameId = reader.IsDBNull(2) ? 0 : reader.GetInt32(2),
                    SetId = reader.IsDBNull(3) ? 0 : reader.GetInt32(3),
                    CardId = reader.IsDBNull(4) ? 0 : reader.GetInt32(4),
                    CardName = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                    Abbreviation = reader.IsDBNull(6) ? null : reader.GetString(6),
                    Rarity = reader.IsDBNull(7) ? null : reader.GetString(7),
                    Foil = reader.IsDBNull(8) ? null : reader.GetString(8),
                    ImageUrl = reader.IsDBNull(9) ? null : reader.GetString(9),
                    MktPriceUrl = reader.IsDBNull(10) ? null : reader.GetString(10),
                    MktPrice = reader.IsDBNull(11) ? (decimal?)null : reader.GetDecimal(11),
                    AmtInStock = reader.IsDBNull(12) ? (int?)null : reader.GetInt32(12),
                    Approved = reader.IsDBNull(13) ? (bool?)null : reader.GetBoolean(13),
                    NeedsReview = reader.IsDBNull(14) && false ? false : reader.GetBoolean(14),
                    IssueNotes = reader.IsDBNull(15) ? null : reader.GetString(15),
                    DateInserted = reader.IsDBNull(16) ? (DateTime?)null : reader.GetDateTime(16)
                });
            }

            return RenumberInventoryRowsForDisplay(results, _invDisplayToActualTempId);
        }

        private async Task<string> GetSetNameForBatchAsync(int batchPosition, CancellationToken ct = default)
        {
            if (_currentCardGameId <= 0 || batchPosition <= 0)
            {
                return $"Batch {batchPosition}";
            }

            var settingsStore = new JsonSettingsStore(AppPaths.SettingsPath);
            var appSettings = settingsStore.Load();
            var connectionFactory = new SqlConnectionFactory(appSettings);

            var password = await Session.PasswordProvider.GetPasswordAsync().ConfigureAwait(false);
            using var conn = connectionFactory.CreateForCurrentUser(password);
            password = string.Empty;
            await conn.OpenAsync(ct).ConfigureAwait(false);

            const string sql = @"SELECT setName FROM dbo.NewCardgameSetTemp WHERE cardGameId = @cardGameId AND tempId = @tempId;";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@cardGameId", SqlDbType.Int).Value = _currentCardGameId;
            cmd.Parameters.Add("@tempId", SqlDbType.Int).Value = batchPosition;

            var result = await cmd.ExecuteScalarAsync(ct).ConfigureAwait(false);
            return result == null || result == DBNull.Value ? $"Batch {batchPosition}" : Convert.ToString(result) ?? $"Batch {batchPosition}";
        }

        private void UpdateBatchLabels(string setName)
        {
            void apply()
            {
                lblSetName.Text = setName;
                lblBatchNumber.Text = $"Batch {displayedBatch} of {currentBatch}";
            }

            if (InvokeRequired)
            {
                BeginInvoke((Action)(() => apply()));
            }
            else
            {
                apply();
            }
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

            _currentCardGameId = game.CardGameId;
            _currentGame = game;

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
            // Read the current row to determine tempId and the *old* includeInBatch state.
            // The new state will be the opposite of the old state.
            try
            {
                // Obtain the data item from the record (Syncfusion RecordEntry exposes Data).
                object? dataItem = null;
                var record = e.Record;
                if (record != null)
                {
                    var dataProperty = record.GetType().GetProperty("Data");
                    dataItem = dataProperty?.GetValue(record) ?? record;
                }

                if (dataItem == null) return;

                int tempId;
                bool oldIncludeInBatch;

                if (dataItem is ReleaseCheck.NewCardgameSetTempRow typedRow)
                {
                    tempId = typedRow.TempId;
                    oldIncludeInBatch = typedRow.IncludeInBatch;
                }
                else
                {
                    var tempIdProp = dataItem.GetType().GetProperty("TempId");
                    if (tempIdProp == null) return;
                    tempId = Convert.ToInt32(tempIdProp.GetValue(dataItem));

                    var includeInBatchProp = dataItem.GetType().GetProperty("IncludeInBatch");
                    if (includeInBatchProp == null) return;
                    oldIncludeInBatch = (bool)(includeInBatchProp.GetValue(dataItem) ?? false);
                }

                // New state is opposite of old
                var newIncludeInBatch = !oldIncludeInBatch;

                // Defer DB update and grid refresh until after the checkbox UI update completes
                this.BeginInvoke(new Action(async () =>
                {
                    try
                    {
                        if (_currentCardGameId < 0) return;

                        // Build connection factory and ReleaseCheck
                        var settingsStore = new JsonSettingsStore(AppPaths.SettingsPath);
                        var appSettings = settingsStore.Load();
                        var connectionFactory = new SqlConnectionFactory(appSettings);
                        var releaseCheck = new ReleaseCheck(connectionFactory, Session.PasswordProvider.GetPasswordAsync);

                        // Update DB: toggle includeInBatch and renumber tempIds
                        await releaseCheck.UpdateIncludeInBatchAsync(_currentCardGameId, tempId, newIncludeInBatch);

                        // Refresh grid from DB
                        var rows = await releaseCheck.GetNewCardgameSetTempAsync(_currentCardGameId);
                        _newSetsRows = rows.Cast<object>().ToList();

                        // Rebind to grid
                        sfDataGrid_NewSets.DataSource = rows;

                        // Recompute maxBatchSize
                        var includeCount = _newSetsRows.Count(r =>
                        {
                            var p = r.GetType().GetProperty("IncludeInBatch");
                            if (p == null) return false;
                            return (bool)(p.GetValue(r) ?? false);
                        });

                        maxBatchSize = includeCount;
                        lblMaxBatchSize.Text = maxBatchSize.ToString();

                        // Clamp the numeric textbox if needed
                        if (integerTextBox_MBS.IntegerValue > maxBatchSize)
                        {
                            integerTextBox_MBS.IntegerValue = maxBatchSize;
                            integerTextBox_MBS.Text = maxBatchSize.ToString();
                        }

                        currentBatch = Convert.ToInt32(integerTextBox_MBS.IntegerValue);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to update batch: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }));
            }
            catch
            {
                // swallow; UI shouldn't crash from checkbox toggles
            }
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

        private async void btnScanItems_Click(object sender, EventArgs e)
        {
            if (_currentCardGameId < 0)
            {
                MessageBox.Show("Please fetch sets first.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            btnScanItems.Enabled = false;
            var prevCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;

            try
            {
                var settingsStore = new JsonSettingsStore(AppPaths.SettingsPath);
                var appSettings = settingsStore.Load();
                var connectionFactory = new SqlConnectionFactory(appSettings);

                var yugiohInventory = new AddNewYugiohInventory(connectionFactory, Session.PasswordProvider.GetPasswordAsync);
                var rows = await yugiohInventory.RunAsync(_currentCardGameId, currentBatch);

                var displayRows = RenumberInventoryRowsForDisplay(rows, _invDisplayToActualTempId);

                // Detect issues and store for highlighting
                await EvaluateIssuesAndRefreshAsync(displayRows, connectionFactory);

                // Bind to inventory grid
                if (this.IsHandleCreated && this.InvokeRequired)
                {
                    this.Invoke(() => sfDataGrid_NewInv.DataSource = displayRows);
                }
                else
                {
                    sfDataGrid_NewInv.DataSource = displayRows;
                }

                // After loading items, remove temp sets not represented in inventory
                var releaseCheck = new ReleaseCheck(connectionFactory, Session.PasswordProvider.GetPasswordAsync);
                await releaseCheck.DeleteTempSetsNotInInventoryAsync(_currentCardGameId);
                var remainingSets = await releaseCheck.GetNewCardgameSetTempAsync(_currentCardGameId);

                _newSetsRows = remainingSets.Cast<object>().ToList();
                if (this.IsHandleCreated && this.InvokeRequired)
                {
                    this.Invoke(() => sfDataGrid_NewSets.DataSource = remainingSets);
                }
                else
                {
                    sfDataGrid_NewSets.DataSource = remainingSets;
                }

                // Refresh highlights
                sfDataGrid_NewInv.Refresh();
                sfDataGrid_NewSets.Refresh();

                // Recompute max batch size based on includeInBatch
                var includeCount = _newSetsRows.Count(r =>
                {
                    var p = r.GetType().GetProperty("IncludeInBatch");
                    if (p == null) return false;
                    return (bool)(p.GetValue(r) ?? false);
                });
                maxBatchSize = includeCount;
                if (includeCount < 0) includeCount = 0;
                lblMaxBatchSize.Text = includeCount.ToString();
                if (integerTextBox_MBS.IntegerValue > includeCount)
                {
                    integerTextBox_MBS.IntegerValue = includeCount;
                    integerTextBox_MBS.Text = includeCount.ToString();
                }
                currentBatch = Convert.ToInt32(integerTextBox_MBS.IntegerValue);

                if (displayRows.Count == 0)
                {
                    MessageBox.Show("No items were loaded for the selected batch.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    readyForItems = true;
                    displayedBatch = 1;
                    displayedSetName = "Displaying All Sets";
                    UpdateBatchLabels(displayedSetName);
                    MessageBox.Show($"Loaded {displayRows.Count} items into the batch.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Scan failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnScanItems.Enabled = true;
                Cursor.Current = prevCursor;
            }
        }


        // Navigation for reviewing items

        private async void sfDataGrid_NewSets_CellClick_1(object sender, Syncfusion.WinForms.DataGrid.Events.CellClickEventArgs e)
        {
            try
            {
                var dataItem = e.DataRow?.RowData;
                if (dataItem == null) return;

                var tempIdProp = dataItem.GetType().GetProperty("TempId");
                var setIdProp = dataItem.GetType().GetProperty("SetId");
                var setNameProp = dataItem.GetType().GetProperty("SetName");

                if (tempIdProp == null || setIdProp == null) return;

                selectedBatch = Convert.ToInt32(tempIdProp.GetValue(dataItem) ?? 0);
                selectedSetId = Convert.ToInt32(setIdProp.GetValue(dataItem) ?? 0);
                selectedSetName = setNameProp?.GetValue(dataItem)?.ToString() ?? string.Empty;

                if (readyForItems && selectedBatch > 0)
                {
                    displayedBatch = selectedBatch;
                    displayedSetId = selectedSetId;
                    displayedSetName = selectedSetName;
                    await DisplayBatchItemsAsync().ConfigureAwait(false);
                }
            }
            catch
            {
                // ignore
            }
        }

        private async Task DisplayBatchItemsAsync()
        {
            var rows = await LoadInventoryBatchAsync(displayedBatch).ConfigureAwait(false);
            var name = await GetSetNameForBatchAsync(displayedBatch).ConfigureAwait(false);
            displayedSetName = name;

            void apply()
            {
                sfDataGrid_NewInv.DataSource = rows;
                UpdateBatchLabels(name);
            }

            if (IsHandleCreated && InvokeRequired)
            {
                BeginInvoke((Action)(() => apply()));
            }
            else
            {
                apply();
            }
        }

        private async void btnPrev_Click(object sender, EventArgs e)
        {
            if (!readyForItems || currentBatch <= 0)
            {
                return;
            }

            displayedBatch = displayedBatch <= 1 ? currentBatch : displayedBatch - 1;
            await DisplayBatchItemsAsync().ConfigureAwait(false);
        }

        private async void btnNext_Click(object sender, EventArgs e)
        {
            if (!readyForItems || currentBatch <= 0)
            {
                return;
            }

            displayedBatch = displayedBatch >= currentBatch ? 1 : displayedBatch + 1;
            await DisplayBatchItemsAsync().ConfigureAwait(false);
        }


        // This class represents the selected Item from sfDataGrid_NewInv
        // It can be used for further processing, such as editing or saving to the database
        public sealed class NewTempCardgameInventory_SelectedRow
        {
            public int TempId { get; init; }             // display tempId
            public int ActualTempId { get; init; }       // DB tempId
            public int BatchPosition { get; init; }
            public int CardGameId { get; init; }
            public int SetId { get; init; }
            public int CardId { get; init; }
            public string CardName { get; init; } = string.Empty;
            public string? Abbreviation { get; init; }
            public string? Rarity { get; init; }
            public string? Foil { get; init; }
            public string? ImageUrl { get; init; }
            public string? MktPriceUrl { get; init; }
            public decimal? MktPrice { get; init; }
            public int? AmtInStock { get; init; }
            public bool? Approved { get; init; }
            public bool NeedsReview { get; init; }
            public string? IssueNotes { get; init; }
            public DateTime? DateInserted { get; init; }
        }

        private void sfDataGrid_NewInv_SelectionChanged(object sender, Syncfusion.WinForms.DataGrid.Events.SelectionChangedEventArgs e)
        {
            try
            {
                var item = e.AddedItems?.FirstOrDefault();
                if (item == null)
                {
                    _selectedInventoryRow = null;
                    return;
                }

                if (item is not AddNewYugiohInventory.NewTempCardgameInventoryRow row)
                {
                    _selectedInventoryRow = null;
                    return;
                }

                var displayTempId = row.TempId;
                _invDisplayToActualTempId.TryGetValue(displayTempId, out var actualTempId);

                _selectedInventoryRow = new NewTempCardgameInventory_SelectedRow
                {
                    TempId = displayTempId,
                    ActualTempId = actualTempId == 0 ? displayTempId : actualTempId,
                    BatchPosition = row.BatchPosition,
                    CardGameId = row.CardGameId,
                    SetId = row.SetId,
                    CardId = row.CardId,
                    CardName = row.CardName,
                    Abbreviation = row.Abbreviation,
                    Rarity = row.Rarity,
                    Foil = row.Foil,
                    ImageUrl = row.ImageUrl,
                    MktPriceUrl = row.MktPriceUrl,
                    MktPrice = row.MktPrice,
                    AmtInStock = row.AmtInStock,
                    Approved = row.Approved,
                    NeedsReview = row.NeedsReview,
                    IssueNotes = row.IssueNotes,
                    DateInserted = row.DateInserted
                };
            }
            catch
            {
                _selectedInventoryRow = null;
            }
        }

        private async void sfDataGrid_NewInv_CurrentCellEndEdit(object sender, Syncfusion.WinForms.DataGrid.Events.CurrentCellEndEditEventArgs e)
        {
            try
            {
                var rowData = e.DataRow?.RowData as AddNewYugiohInventory.NewTempCardgameInventoryRow;
                if (rowData == null)
                {
                    return;
                }

                // Resolve actual tempId using mapping
                _invDisplayToActualTempId.TryGetValue(rowData.TempId, out var actualTempId);
                if (actualTempId == 0)
                {
                    // fallback: use display as actual if mapping missing
                    actualTempId = rowData.TempId;
                }

                var edited = new NewTempCardgameInventory_SelectedRow
                {
                    TempId = rowData.TempId,
                    ActualTempId = actualTempId,
                    BatchPosition = rowData.BatchPosition,
                    CardGameId = rowData.CardGameId,
                    SetId = rowData.SetId,
                    CardId = rowData.CardId,
                    CardName = rowData.CardName,
                    Abbreviation = rowData.Abbreviation,
                    Rarity = rowData.Rarity,
                    Foil = rowData.Foil,
                    ImageUrl = rowData.ImageUrl,
                    MktPriceUrl = rowData.MktPriceUrl,
                    MktPrice = rowData.MktPrice,
                    AmtInStock = rowData.AmtInStock,
                    Approved = rowData.Approved,
                    NeedsReview = rowData.NeedsReview,
                    IssueNotes = rowData.IssueNotes,
                    DateInserted = rowData.DateInserted
                };

                _selectedInventoryRow = edited;

                var settingsStore = new JsonSettingsStore(AppPaths.SettingsPath);
                var appSettings = settingsStore.Load();
                var connectionFactory = new SqlConnectionFactory(appSettings);

                var editor = new EditsMadeToInventoryTable(connectionFactory, Session.PasswordProvider.GetPasswordAsync);
                await editor.UpdateInventoryRowAsync(edited).ConfigureAwait(false);

                // Refresh current batch to reflect DB state
                await DisplayBatchItemsAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Save failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SfDataGrid_NewInv_QueryCellStyle(object sender, Syncfusion.WinForms.DataGrid.Events.QueryCellStyleEventArgs e)
        {
            var row = e.DataRow?.RowData as AddNewYugiohInventory.NewTempCardgameInventoryRow;
            if (row == null) return;
            if (_invIssueResult.InventoryCellIssues.TryGetValue(row.TempId, out var cols) &&
                cols.Contains(e.Column.MappingName, StringComparer.OrdinalIgnoreCase))
            {
                e.Style.BackColor = Color.LightCoral;
                e.Style.TextColor = Color.Black;
                return;
            }

            if (_invIssueResult.InventoryCellWarnings.TryGetValue(row.TempId, out var warnCols) &&
                warnCols.Contains(e.Column.MappingName, StringComparer.OrdinalIgnoreCase))
            {
                e.Style.BackColor = Color.LightBlue;
                e.Style.TextColor = Color.Black;
            }
        }

        private void SfDataGrid_NewSets_QueryRowStyle(object sender, Syncfusion.WinForms.DataGrid.Events.QueryRowStyleEventArgs e)
        {
            if (e.RowData == null) return;

            // sets grid rows are ReleaseCheck.NewCardgameSetTempRow
            var setIdProp = e.RowData.GetType().GetProperty("SetId");
            if (setIdProp == null) return;
            var setId = Convert.ToInt32(setIdProp.GetValue(e.RowData) ?? 0);

            if (_invIssueResult.CriticalSetIds.Contains(setId))
            {
                e.Style.BackColor = Color.LightCoral;
                e.Style.TextColor = Color.Black;
                return;
            }

            if (_invIssueResult.GoodSetIds.Contains(setId))
            {
                e.Style.BackColor = Color.LightGreen;
                e.Style.TextColor = Color.Black;
            }
        }

        private async Task EvaluateIssuesAndRefreshAsync(
            IEnumerable<AddNewYugiohInventory.NewTempCardgameInventoryRow> rows,
            SqlConnectionFactory connectionFactory,
            CancellationToken ct = default)
        {
            if (_currentCardGameId <= 0 || _currentGame == null)
            {
                _invIssueResult = new ErrorIdentifyerHelper.InventoryIssueResult();
                return;
            }

            var helper = new ErrorIdentifyerHelper();
            _invIssueResult = await helper.EvaluateInventoryIssuesAsync(
                rows,
                connectionFactory,
                Session.PasswordProvider.GetPasswordAsync,
                _currentGame.DatabaseName,
                _currentCardGameId,
                ct).ConfigureAwait(false);

            // Persist issue notes back to DB
            await helper.UpdateIssueNotesAsync(
                rows,
                _invDisplayToActualTempId,
                connectionFactory,
                Session.PasswordProvider.GetPasswordAsync,
                _currentCardGameId,
                _invIssueResult,
                ct).ConfigureAwait(false);

            RefreshIssueHighlightsOnUi();
        }

        private void RefreshIssueHighlightsOnUi()
        {
            void apply()
            {
                sfDataGrid_NewInv.Refresh();
                sfDataGrid_NewSets.Refresh();
            }

            if (IsHandleCreated && InvokeRequired)
            {
                BeginInvoke((Action)(() => apply()));
            }
            else
            {
                apply();
            }
        }
    }
}
