using Card_Addiction_POS_System.Functions.Inventory;
using Card_Addiction_POS_System.Functions.Models;
using Card_Addiction_POS_System.Functions.Pricing;
using Card_Addiction_POS_System.Data.Settings;
using Card_Addiction_POS_System.Data.SQLServer;
using Card_Addiction_POS_System.Security;
using Syncfusion.WinForms.Controls;
using Syncfusion.WinForms.DataGrid;
using Syncfusion.WinForms.DataGrid.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Card_Addiction_POS_System.Forms
{
    public partial class InventoryMgmt : SfForm
    {
        private readonly IInventoryService? _inventoryService;
        private InventoryItem? _selectedInventoryItem;
        private static readonly HttpClient httpClient = new HttpClient();

        public bool IsNavigating { get; set; }
        public virtual string FormTitle { get; set; } = "Inventory Management";

        // Designer constructor (keeps existing behavior)
        public InventoryMgmt()
        {
            InitializeComponent();
            // Safe defaults so designer-instantiated form doesn't crash if used without service
            WireUiHandlers();
        }

        // DI-friendly constructor used by HomePage (or other callers) to provide the inventory service
        public InventoryMgmt(IInventoryService inventoryService)
        {
            InitializeComponent();
            _inventoryService = inventoryService;

            // Mirror behavior from BuySell: enable typing and configure grid/handlers
            tbSearchBar.ReadOnly = false;
            tbSearchBar.Enabled = true;

            ConfigureInventoryGrid();

            // Ensure selection notifications arrive
            sfDataGrid_InvLookup.SelectionChanged += sfDataGrid_InvLookup_SelectionChanged;

            WireUiHandlers();
        }

        private void WireUiHandlers()
        {
            // Wire keyboard and click handlers used for searching and selection
            tbSearchBar.KeyDown += tbSearchBar_KeyDown;
            btnSearch.Click += btnSearch_Click;
            // The designer already wires sfDataGrid_InvLookup.SelectionChanged to sfDataGrid_InvLookup_SelectionChanged,
            // but subscribing again is safe when using the DI constructor. Click is wired in designer too.
        }

        private void InventoryMgmt_Load(object sender, EventArgs e)
        {
        }

        private void InventoryMgmt_FormClosed(object sender, FormClosedEventArgs e)
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

        // Configure grid columns & behavior (same approach as BuySell)
        private void ConfigureInventoryGrid()
        {
            sfDataGrid_InvLookup.AutoGenerateColumns = false;
            sfDataGrid_InvLookup.AllowSorting = true;
            sfDataGrid_InvLookup.AllowFiltering = true;
            sfDataGrid_InvLookup.AllowResizingColumns = true;
            sfDataGrid_InvLookup.AutoSizeColumnsMode = AutoSizeColumnsMode.Fill;
            sfDataGrid_InvLookup.AllowEditing = false;
            sfDataGrid_InvLookup.SelectionMode = GridSelectionMode.Single;
            sfDataGrid_InvLookup.SelectionUnit = SelectionUnit.Row;
            sfDataGrid_InvLookup.Columns.Clear();

            sfDataGrid_InvLookup.Columns.Add(new GridTextColumn
            {
                MappingName = nameof(InventoryItem.CardName),
                HeaderText = "Card Name",
                Width = 500,
                MinimumWidth = 200
            });

            sfDataGrid_InvLookup.Columns.Add(new GridTextColumn
            {
                MappingName = nameof(InventoryItem.Rarity),
                HeaderText = "Rarity",
                Width = 250,
                MinimumWidth = 100
            });

            sfDataGrid_InvLookup.Columns.Add(new GridNumericColumn
            {
                MappingName = nameof(InventoryItem.SetId),
                HeaderText = "Set",
                Width = 1,
                MinimumWidth = 100,
                NumberFormatInfo = new NumberFormatInfo
                {
                    NumberDecimalDigits = 0,
                    NumberGroupSeparator = string.Empty
                }
            });

            sfDataGrid_InvLookup.Columns.Add(new GridNumericColumn
            {
                MappingName = nameof(InventoryItem.MktPrice),
                HeaderText = "Market Price",
                Width = 1,
                MinimumWidth = 120,
                NumberFormatInfo = new NumberFormatInfo
                {
                    CurrencyDecimalDigits = 2,
                    CurrencySymbol = "$",
                    NumberDecimalDigits = 2
                }
            });

            sfDataGrid_InvLookup.Columns.Add(new GridNumericColumn
            {
                MappingName = nameof(InventoryItem.AmtInStock),
                HeaderText = "In Stock",
                Width = 1,
                MinimumWidth = 100,
                NumberFormatInfo = new NumberFormatInfo
                {
                    NumberDecimalDigits = 0,
                    NumberGroupSeparator = string.Empty
                }
            });

            sfDataGrid_InvLookup.Columns.Add(new GridCheckBoxColumn
            {
                MappingName = nameof(InventoryItem.PriceUp2Date),
                HeaderText = "Up 2 Date",
                Width = 1,
                MinimumWidth = 100
            });

            // Hidden technical columns: ImageUrl, MktPriceUrl, CardId
            sfDataGrid_InvLookup.Columns.Add(new GridTextColumn { MappingName = nameof(InventoryItem.ImageUrl), Visible = false });
            sfDataGrid_InvLookup.Columns.Add(new GridTextColumn { MappingName = nameof(InventoryItem.MktPriceUrl), Visible = false });
            sfDataGrid_InvLookup.Columns.Add(new GridNumericColumn { MappingName = nameof(InventoryItem.CardId), Visible = false });
        }

        // Event handlers for selection logic (wired in designer)
        private void sfDataGrid_InvLookup_SelectionChanged(object sender, Syncfusion.WinForms.DataGrid.Events.SelectionChangedEventArgs e)
        {
            GrabSelectedInventoryItemData();

            if (_selectedInventoryItem != null)
            {
                _ = LoadCardImageAsync_TCGPlayer(_selectedInventoryItem.ImageUrl);
            }
            else
            {
                imgCardUrl.Image = null;
            }
        }

        private void sfDataGrid_InvLookup_Click(object sender, EventArgs e)
        {
            // Intentionally left empty - selection handled in SelectionChanged
        }

        // Assemble InventoryItem data and populate tbPrice & tbAmtTraded
        private void GrabSelectedInventoryItemData()
        {
            var item = sfDataGrid_InvLookup.SelectedItem as InventoryItem;
            if (item == null)
            {
                _selectedInventoryItem = null;
                tbPrice.Text = string.Empty;
                tbPrice.DecimalValue = 0m;
                tbAmtTraded.IntegerValue = 1L;
                tbAmtTraded.Text = "1";
                return;
            }

            _selectedInventoryItem = item;

            // Display market price and keep numeric value for calculations
            tbPrice.Text = item.MktPrice.ToString("C2", CultureInfo.GetCultureInfo("en-US"));
            tbPrice.DecimalValue = item.MktPrice;

            // Display amount in stock in the IntegerTextBox
            tbAmtTraded.IntegerValue = item.AmtInStock;
            tbAmtTraded.Text = item.AmtInStock.ToString();
        }

        // Search UI: enter key triggers search
        private void tbSearchBar_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                btnSearch.PerformClick();
            }
        }

        // Search button: query DB via injected service
        private void btnSearch_Click_1(object sender, EventArgs e)
        {

        }
        private async void btnSearch_Click(object? sender, EventArgs e)
        {
            if (_inventoryService == null)
            {
                MessageBox.Show("Inventory service is not initialized. Open Inventory from the HomePage so the application can create a database service.", "Service Unavailable", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Read numeric CardGameId from the reusable SelectCardGameControl
            var cardGameId = selectCardGameControl1.SelectedCardGameId;
            if (cardGameId < 0)
            {
                MessageBox.Show("Please select a card game.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var searchText = tbSearchBar.Text ?? string.Empty;

            btnSearch.Enabled = false;
            var previousCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;

            try
            {
                var results = await _inventory_service_call_guard();

                // Set DataSource on UI thread
                sfDataGrid_InvLookup.DataSource = results ?? null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Search failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnSearch.Enabled = true;
                Cursor.Current = previousCursor;
            }

            async Task<IReadOnlyList<InventoryItem>?> _inventory_service_call_guard()
            {
                // Keep the null-check in a separate method to keep the outer method tidy while satisfying analyzer.
                if (_inventoryService == null) return null;
                return await _inventoryService.SearchInventoryAsync(cardGameId, searchText);
            }
        }

        // Image loader (same approach as BuySell — spoof headers to reduce 403)
        private async Task LoadCardImageAsync_TCGPlayer(string? imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                imgCardUrl.Image = null;
                return;
            }

            try
            {
                httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
                httpClient.DefaultRequestHeaders.Referrer = new Uri("https://www.tcgplayer.com/");
                httpClient.DefaultRequestHeaders.Accept.ParseAdd("image/webp,image/apng,image/*,*/*;q=0.8");

                var stream = await httpClient.GetStreamAsync(imageUrl);

                using (stream)
                {
                    imgCardUrl.Image = Image.FromStream(stream);
                }
            }
            catch (Exception ex)
            {
                imgCardUrl.Image = null;
                MessageBox.Show($"Unable to load image.\n\nError: {ex.Message}", "Image Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Updated: fetch price using FindPrice_TCG, persist to DB, refresh grid and update UI
        private async void btnFetch_Click(object sender, EventArgs e)
        {
            if (_inventory_service_unavailable_guard())
            {
                MessageBox.Show("Inventory service is not initialized. Open Inventory from the HomePage so the application can create a database service.", "Service Unavailable", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_selectedInventoryItem == null)
            {
                MessageBox.Show("Please select an item first.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var url = _selected_inventory_item_url_guard();
            if (string.IsNullOrWhiteSpace(url))
            {
                MessageBox.Show("Selected item does not have a market price URL.", "No URL", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Use numeric CardGameId from the control
            var cardGameId = selectCardGameControl1.SelectedCardGameId;
            if (cardGameId < 0)
            {
                MessageBox.Show("Please select a card game.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Preserve identity of selected item to re-select after refresh
            var selectedCardId = _selectedInventoryItem.CardId;

            btnFetch.Enabled = false;
            var prevCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;

            try
            {
                // 1) Lookup new price on background thread using the provided finder
                decimal newPrice = await Task.Run(() =>
                {
                    using var finder = new FindPrice_TCG();
                    return finder.GetMarketPrice(url);
                });

                // 2) Persist new price to database (pass numeric cardGameId)
                await _inventoryService.UpdatePriceAsync(cardGameId, selectedCardId, newPrice);

                // 3) Refresh current search results (preserve filter)
                var searchText = tbSearchBar.Text ?? string.Empty;
                var refreshed = await _inventoryService.SearchInventoryAsync(cardGameId, searchText);

                // 4) Rebind grid and restore selection by CardId
                sfDataGrid_InvLookup.DataSource = refreshed ?? null;

                if (refreshed != null)
                {
                    var idx = refreshed.ToList().FindIndex(x => x.CardId == selectedCardId);
                    if (idx >= 0)
                    {
                        sfDataGrid_InvLookup.SelectedIndex = idx;
                    }
                }

                // 5) Update UI and inform user
                _selectedInventoryItem = refreshed?.FirstOrDefault(r => r.CardId == selectedCardId);
                if (_selectedInventoryItem != null)
                {
                    tbPrice.DecimalValue = _selectedInventoryItem.MktPrice;
                    tbPrice.Text = _selectedInventoryItem.MktPrice.ToString("C2", CultureInfo.GetCultureInfo("en-US"));
                }
                else
                {
                    // Fallback: set to newPrice if item not found in refreshed results
                    tbPrice.DecimalValue = newPrice;
                    tbPrice.Text = newPrice.ToString("C2", CultureInfo.GetCultureInfo("en-US"));
                }

                MessageBox.Show($"Price updated to {newPrice.ToString("C2", CultureInfo.GetCultureInfo("en-US"))}.", "Price Updated", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fetch failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnFetch.Enabled = true;
                Cursor.Current = prevCursor;
            }

            string _selected_inventory_item_url_guard() => _selectedInventoryItem?.MktPriceUrl ?? string.Empty;

            bool _inventory_service_unavailable_guard() => _inventoryService == null;
        }

        // UPDATED: Update amtInStock using the helper class, then refresh the grid while preserving selection
        private async void btnUpdateAmt_Click(object sender, EventArgs e)
        {
            if (_selectedInventoryItem == null)
            {
                MessageBox.Show("Please select an item first.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_inventoryService == null)
            {
                MessageBox.Show("Inventory service is not initialized. Open Inventory from the HomePage so the application can create a database service.", "Service Unavailable", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Use numeric CardGameId from the control
            var cardGameId = selectCardGameControl1.SelectedCardGameId;
            if (cardGameId < 0)
            {
                MessageBox.Show("Please select a card game.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Read desired amount from tbAmtTraded (IntegerValue is a long)
            int newAmount;
            try
            {
                newAmount = Convert.ToInt32(tbAmtTraded.IntegerValue);
                if (newAmount < 0) throw new ArgumentOutOfRangeException(nameof(newAmount), "Amount cannot be negative.");
            }
            catch (Exception)
            {
                MessageBox.Show("Please enter a valid non-negative integer for the amount.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var selectedCardId = _selectedInventoryItem.CardId;

            // Prepare connection factory using the same settings pattern used elsewhere
            var settingsStore = new JsonSettingsStore(AppPaths.SettingsPath);
            var appSettings = settingsStore.Load();
            var connectionFactory = new SqlConnectionFactory(appSettings);

            var updater = new AmtInStock_UpdateDB(connectionFactory, Session.PasswordProvider.GetPasswordAsync);

            btnUpdateAmt.Enabled = false;
            var prevCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;

            try
            {
                // 1) Persist new amount (await without ConfigureAwait so continuation returns to UI thread)
                await updater.UpdateAmountAsync(cardGameId, selectedCardId, newAmount);

                // 2) Refresh the grid results using existing filter and restore selection position
                //    (run on UI thread — no Invoke required because await returned to context)
                var searchText = tbSearchBar.Text ?? string.Empty;
                var refreshed = await _inventoryService.SearchInventoryAsync(cardGameId, searchText);

                sfDataGrid_InvLookup.DataSource = refreshed ?? null;

                if (refreshed != null)
                {
                    var idx = refreshed.ToList().FindIndex(x => x.CardId == selectedCardId);
                    if (idx >= 0)
                    {
                        sfDataGrid_InvLookup.SelectedIndex = idx;
                    }
                }

                // Update local selection and UI values
                _selectedInventoryItem = refreshed?.FirstOrDefault(r => r.CardId == selectedCardId) ?? _selectedInventoryItem;
                tbAmtTraded.IntegerValue = _selectedInventoryItem?.AmtInStock ?? newAmount;
                tbAmtTraded.Text = (_selectedInventoryItem?.AmtInStock ?? newAmount).ToString();

                MessageBox.Show("Amount updated successfully.", "Updated", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Update failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Ensure UI changes run on the UI thread
                if (this.IsHandleCreated && this.InvokeRequired)
                {
                    this.Invoke(() =>
                    {
                        btnUpdateAmt.Enabled = true;
                        Cursor.Current = prevCursor;
                    });
                }
                else
                {
                    btnUpdateAmt.Enabled = true;
                    Cursor.Current = prevCursor;
                }
            }
        }

        private void btnAddInventory_Click(object sender, EventArgs e)
        {
            // Follow the same navigation pattern used in HomePage:
            // - Mark this form as navigating so FormClosed does not exit the app.
            // - Create the app SqlConnectionFactory using stored AppSettings (keeps consistency with other navigation paths).
            // - Open the AddInventory form and close this form.

            IsNavigating = true;

            // Load saved server settings (AppSettings does NOT store SQL passwords)
            var settingsStore = new JsonSettingsStore(AppPaths.SettingsPath);
            var appSettings = settingsStore.Load();

            // Create the SqlConnectionFactory using those settings so future forms can use it if needed.
            var connectionFactory = new SqlConnectionFactory(appSettings);

            // AddInventory currently uses parameterless ctor; instantiate and show it.
            var addInventoryForm = new AddInventory();

            // Optional: attach any cleanup or handlers here. Forms launched from this flow commonly
            // rely on Session.PasswordProvider for credentials when needed.
            addInventoryForm.FormClosed += (s, args) => { /* optional cleanup when leaving AddInventory */ };

            addInventoryForm.Show();
            this.Close();
        }

        private void btnUpdateAll_Click(object sender, EventArgs e)
        {
            if (_selectedInventoryItem == null)
            {
                MessageBox.Show("Please select an item first.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var amountChanged = tbAmtTraded.IntegerValue != _selectedInventoryItem.AmtInStock;
            var priceChanged = tbPrice.DecimalValue != _selectedInventoryItem.MktPrice;

            if (!amountChanged && !priceChanged)
            {
                MessageBox.Show("No changes detected to update.", "Nothing to Update", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Update amount first (if changed)
            if (amountChanged)
            {
                btnUpdateAmt_Click(this, EventArgs.Empty);
            }

            // Update price second (if changed)
            if (priceChanged)
            {
                btnSetPrice_Click(this, EventArgs.Empty);
            }
        }

        private async void btnSetPrice_Click(object sender, EventArgs e)
        {
            if (_selectedInventoryItem == null)
            {
                MessageBox.Show("Please select an item first.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var selectedGame = selectCardGameControl1.SelectedGame;
            if (selectedGame == null)
            {
                MessageBox.Show("Please select a card game.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var cardGameId = selectedGame.CardGameId;
            var cardId = _selectedInventoryItem.CardId;
            var cardName = _selectedInventoryItem.CardName;
            var newMktPrice = tbPrice.DecimalValue;

            if (newMktPrice < 0m)
            {
                MessageBox.Show("Please enter a valid non-negative market price.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var settingsStore = new JsonSettingsStore(AppPaths.SettingsPath);
            var appSettings = settingsStore.Load();
            var connectionFactory = new SqlConnectionFactory(appSettings);
            var updater = new SetCustomMktPrice_Inventory(connectionFactory, Session.PasswordProvider.GetPasswordAsync);

            btnSetPrice.Enabled = false;
            var prevCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;

            try
            {
                var successMessage = await updater.SetCustomPriceAsync(cardGameId, cardId, cardName, newMktPrice);

                if (_inventoryService != null)
                {
                    var searchText = tbSearchBar.Text ?? string.Empty;
                    var refreshed = await _inventoryService.SearchInventoryAsync(cardGameId, searchText);

                    sfDataGrid_InvLookup.DataSource = refreshed ?? null;

                    if (refreshed != null)
                    {
                        var idx = refreshed.ToList().FindIndex(x => x.CardId == cardId);
                        if (idx >= 0)
                        {
                            sfDataGrid_InvLookup.SelectedIndex = idx;
                        }

                        _selectedInventoryItem = refreshed.FirstOrDefault(x => x.CardId == cardId) ?? _selectedInventoryItem;
                    }
                }
                else
                {
                    _selectedInventoryItem.MktPrice = newMktPrice;
                    _selectedInventoryItem.PriceUp2Date = true;
                }

                tbPrice.DecimalValue = newMktPrice;
                tbPrice.Text = newMktPrice.ToString("C2", CultureInfo.GetCultureInfo("en-US"));

                MessageBox.Show(successMessage, "Price Updated", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Set price failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnSetPrice.Enabled = true;
                Cursor.Current = prevCursor;
            }
        }

        private async void btnItemNotInv_Click(object sender, EventArgs e)
        {
            if (_selectedInventoryItem == null)
            {
                MessageBox.Show("Please select an item first.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var selectedGame = selectCardGameControl1.SelectedGame;
            if (selectedGame == null)
            {
                MessageBox.Show("Please select a card game.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var cardGameId = selectedGame.CardGameId;
            var cardId = _selectedInventoryItem.CardId;
            var cardName = _selectedInventoryItem.CardName;

            var settingsStore = new JsonSettingsStore(AppPaths.SettingsPath);
            var appSettings = settingsStore.Load();
            var connectionFactory = new SqlConnectionFactory(appSettings);
            var updater = new SetAmtInStockToNull(connectionFactory, Session.PasswordProvider.GetPasswordAsync);

            btnItemNotInv.Enabled = false;
            var prevCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;

            try
            {
                var successMessage = await updater.SetNotInInventoryAsync(cardGameId, cardId, cardName);

                if (_inventoryService != null)
                {
                    var searchText = tbSearchBar.Text ?? string.Empty;
                    var refreshed = await _inventoryService.SearchInventoryAsync(cardGameId, searchText);

                    sfDataGrid_InvLookup.DataSource = refreshed ?? null;

                    if (refreshed != null)
                    {
                        var idx = refreshed.ToList().FindIndex(x => x.CardId == cardId);
                        if (idx >= 0)
                        {
                            sfDataGrid_InvLookup.SelectedIndex = idx;
                        }

                        _selectedInventoryItem = refreshed.FirstOrDefault(x => x.CardId == cardId) ?? _selectedInventoryItem;
                    }
                }
                else
                {
                    // InventoryItem.AmtInStock is int, so represent "Not Inv"/NULL as 0 in UI model fallback.
                    _selectedInventoryItem.AmtInStock = 0;
                }

                tbAmtTraded.IntegerValue = _selectedInventoryItem?.AmtInStock ?? 0;
                tbAmtTraded.Text = (_selectedInventoryItem?.AmtInStock ?? 0).ToString();

                MessageBox.Show(successMessage, "Updated", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Set Not Inv failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnItemNotInv.Enabled = true;
                Cursor.Current = prevCursor;
            }
        }

        private async void btnUpdateCaseAmt_Click(object sender, EventArgs e)
        {
            if (_selectedInventoryItem == null)
            {
                MessageBox.Show("Please select an item first.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_inventoryService == null)
            {
                MessageBox.Show("Inventory service is not initialized. Open Inventory from the HomePage so the application can create a database service.", "Service Unavailable", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var cardGameId = selectCardGameControl1.SelectedCardGameId;
            if (cardGameId < 0)
            {
                MessageBox.Show("Please select a card game.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int newAmountInCase;
            try
            {
                newAmountInCase = Convert.ToInt32(intTB_AmtInCase.IntegerValue);
                if (newAmountInCase < 0) throw new ArgumentOutOfRangeException(nameof(newAmountInCase), "Amount in case cannot be negative.");
            }
            catch (Exception)
            {
                MessageBox.Show("Please enter a valid non-negative integer for amount in case.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var selectedCardId = _selectedInventoryItem.CardId;

            var settingsStore = new JsonSettingsStore(AppPaths.SettingsPath);
            var appSettings = settingsStore.Load();
            var connectionFactory = new SqlConnectionFactory(appSettings);

            var updater = new AmtInCase_UpdateDB(connectionFactory, Session.PasswordProvider.GetPasswordAsync);

            btnUpdateCaseAmt.Enabled = false;
            var prevCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;

            try
            {
                await updater.UpdateAmountAsync(cardGameId, selectedCardId, newAmountInCase);

                var searchText = tbSearchBar.Text ?? string.Empty;
                var refreshed = await _inventoryService.SearchInventoryAsync(cardGameId, searchText);

                sfDataGrid_InvLookup.DataSource = refreshed ?? null;

                if (refreshed != null)
                {
                    var idx = refreshed.ToList().FindIndex(x => x.CardId == selectedCardId);
                    if (idx >= 0)
                    {
                        sfDataGrid_InvLookup.SelectedIndex = idx;
                    }

                    _selectedInventoryItem = refreshed.FirstOrDefault(r => r.CardId == selectedCardId) ?? _selectedInventoryItem;
                }

                intTB_AmtInCase.IntegerValue = newAmountInCase;
                intTB_AmtInCase.Text = newAmountInCase.ToString();

                MessageBox.Show("Case amount updated successfully.", "Updated", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Update failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (this.IsHandleCreated && this.InvokeRequired)
                {
                    this.Invoke(() =>
                    {
                        btnUpdateCaseAmt.Enabled = true;
                        Cursor.Current = prevCursor;
                    });
                }
                else
                {
                    btnUpdateCaseAmt.Enabled = true;
                    Cursor.Current = prevCursor;
                }
            }
        }
    }
}
