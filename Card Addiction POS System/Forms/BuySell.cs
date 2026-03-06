using Card_Addiction_POS_System.Functions.Inventory;
using Card_Addiction_POS_System.Functions.Models;
using Syncfusion.WinForms.Controls;
using Syncfusion.WinForms.DataGrid;
using Syncfusion.WinForms.DataGrid.Enums;
using Syncfusion.WinForms.Input.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Card_Addiction_POS_System.Functions.Inventory.SearchInventoryDB;
using Syncfusion.WinForms.DataGrid.Events;
using Card_Addiction_POS_System.Functions.Pricing;
using System.Net.Http;
using Card_Addiction_POS_System.Data.Settings;
using Card_Addiction_POS_System.Data.SQLServer;
using Card_Addiction_POS_System.Security;
using Card_Addiction_POS_System.Functions.Sales;
using Card_Addiction_POS_System.LogIn.UserInfo;
using Microsoft.Data.SqlClient;
using Card_Addiction_POS_System.Forms.Controls; // <-- added to reference CustomConfirm

namespace Card_Addiction_POS_System.Forms
{
    public partial class BuySell : SfForm
    {
        private readonly IInventoryService _inventoryService;

        // In-memory cart bound to the cart grid.
        private readonly BindingList<TransactionLineItem> _cartBinding = new();

        // Sale id reserver/holder
        private DetermineSaleId? _saleIdDeterminer;
        private int? _reservedSaleId;

        public BuySell(IInventoryService inventoryService)
        {
            InitializeComponent();
            _inventoryService = inventoryService;

            // Allow typing in the search box even if Designer set it ReadOnly
            tbSearchBar.ReadOnly = false;
            tbSearchBar.Enabled = true;

            ConfigureInventoryGrid();

            // Ensure we receive selection notifications from the grid
            sfDataGrid_InvLookup.SelectionChanged += sfDataGrid_InvLookup_SelectionChanged;

            // Configure cart grid to show current cart contents
            // Use a BindingList so the grid updates when items are added/removed.
            sfDataGrid_Cart.AutoGenerateColumns = true;
            sfDataGrid_Cart.AllowEditing = false;
            sfDataGrid_Cart.DataSource = _cartBinding;
        }

        public bool IsNavigating { get; set; }
        public virtual string FormTitle { get; set; } = "Sales";

        // Make Load async so we can reserve a sale id after designer-time check.
        private async void BuySell_Load(object sender, EventArgs e)
        {
            // Avoid running layout/resize code while Visual Studio Designer is rendering.
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
            {
                return;
            }

            // Resize image and adjust layout
            ImageResizing();
            PositionLabels();

            // Reserve or reuse sale id for this session/form on the UI context.
            // NOTE: do NOT ConfigureAwait(false) here so continuation runs on UI thread.
            await ReserveSaleIdAsync();
        }

        private async Task ReserveSaleIdAsync()
        {
            try
            {
                var settingsStore = new JsonSettingsStore(AppPaths.SettingsPath);
                var appSettings = settingsStore.Load();
                var connectionFactory = new SqlConnectionFactory(appSettings);

                // Create determiner and reserve or reuse next sale id
                _saleIdDeterminer = new DetermineSaleId(connectionFactory, Session.PasswordProvider.GetPasswordAsync);
                _reservedSaleId = await _saleIdDeterminer.ReserveNextSaleIdAsync();

                // Update UI on UI thread
                if (this.IsHandleCreated && this.InvokeRequired)
                {
                    this.Invoke(() => lblSaleInfo.Text = $"Sale ID: {_reservedSaleId}");
                }
                else
                {
                    lblSaleInfo.Text = $"Sale ID: {_reservedSaleId}";
                }
            }
            catch (Exception ex)
            {
                // Non-fatal: inform user and continue
                if (this.IsHandleCreated && this.InvokeRequired)
                {
                    this.Invoke(() => MessageBox.Show($"Warning reserving sale id: {ex.Message}", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning));
                    this.Invoke(() => lblSaleInfo.Text = "Sale ID: N/A");
                }
                else
                {
                    MessageBox.Show($"Warning reserving sale id: {ex.Message}", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    lblSaleInfo.Text = "Sale ID: N/A";
                }

                _reservedSaleId = null;
            }
        }

        // Called after a successful finalize to reset the UI for the next sale but keep current search/results.
        private void ResetFormForNextSale()
        {
            // Clear cart
            _cartBinding.Clear();

            // Reset selection & detail UI
            _selectedInventoryItem = null;
            imgCardUrl.Image = null;
            tbPrice.DecimalValue = 0m;
            tbPrice.Text = string.Empty;
            tbAmtTraded.IntegerValue = 1L;
            tbAmtTraded.Text = "1";
            lblAmtInStock.Text = "N/A";

            // Keep search results and current tab intact
            lblSaleInfo.Text = _reservedSaleId.HasValue ? $"Sale ID: {_reservedSaleId} (reserved)" : "Sale ID: N/A";
        }

        // Add helper mapping used when updating priceUp2Date directly:
        private static readonly Dictionary<string, string> _uiCardGameToTable = new()
        {
            ["Yugioh"] = "YugiohInventory",
            ["Magic"] = "MagicInventory",
            ["Pokemon"] = "PokemonInventory"
        };

        // Price lookup method — updated to use SelectCardGameControl numeric id and database name.
        private async Task<decimal?> PriceLookupAndUpdate()
        {
            if (_selectedInventoryItem == null)
            {
                MessageBox.Show("Please select an item first.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return null;
            }

            // Read typed selection from the new user control.
            var selectedGame = selectCardGameControl1.SelectedGame;
            if (selectedGame == null)
            {
                MessageBox.Show("Please select a card game.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return null;
            }

            var cardGameId = selectedGame.CardGameId;

            var url = _selectedInventoryItem.MktPriceUrl;
            if (string.IsNullOrWhiteSpace(url))
            {
                MessageBox.Show("Selected item does not have a market price URL.", "No URL", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return null;
            }

            var selectedCardId = _selectedInventoryItem.CardId;

            btnAddCt.Enabled = false;
            var prevCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;

            try
            {
                // 1) Lookup new price on background thread
                decimal newPrice = await Task.Run(() =>
                {
                    using var finder = new FindPrice_TCG();
                    return finder.GetMarketPrice(url);
                }).ConfigureAwait(false);

                // 2) Persist price to DB via inventory service (now accepts cardGameId)
                await _inventory_service_update_guard(cardGameId, selectedCardId, newPrice).ConfigureAwait(false);

                // 3) Mark the row as up-to-date in DB using cardGameId
                await UpdatePriceUp2DateInDbAsync(cardGameId, selectedCardId, true).ConfigureAwait(false);

                // 4) Refresh the grid results with the same filter (back to UI)
                var searchText = tbSearchBar.Text ?? string.Empty;
                var refreshed = await _inventoryService.SearchInventoryAsync(cardGameId, searchText).ConfigureAwait(false);

                var priceUpdatedMessage =
                    $"Price updated to {newPrice.ToString("C2", CultureInfo.GetCultureInfo("en-US"))}.\n\n{FindPrice_TCG.PostLookupReviewPrompt}";

                // Marshal UI updates (unchanged logic)
                if (this.IsHandleCreated && this.InvokeRequired)
                {
                    this.Invoke(() =>
                    {
                        sfDataGrid_InvLookup.DataSource = refreshed ?? null;
                        if (refreshed != null)
                        {
                            var idx = refreshed.ToList().FindIndex(x => x.CardId == selectedCardId);
                            if (idx >= 0) sfDataGrid_InvLookup.SelectedIndex = idx;
                        }

                        _selectedInventoryItem = refreshed?.FirstOrDefault(r => r.CardId == selectedCardId) ?? _selectedInventoryItem;
                        if (_selectedInventoryItem != null)
                        {
                            tbPrice.DecimalValue = _selectedInventoryItem.MktPrice;
                            tbPrice.Text = _selectedInventoryItem.MktPrice.ToString("C2", CultureInfo.GetCultureInfo("en-US"));
                        }

                        MessageBox.Show(priceUpdatedMessage, "Price Updated", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    });
                }
                else
                {
                    sfDataGrid_InvLookup.DataSource = refreshed ?? null;
                    if (refreshed != null)
                    {
                        var idx = refreshed.ToList().FindIndex(x => x.CardId == selectedCardId);
                        if (idx >= 0) sfDataGrid_InvLookup.SelectedIndex = idx;
                    }

                    _selectedInventoryItem = refreshed?.FirstOrDefault(r => r.CardId == selectedCardId) ?? _selectedInventoryItem;
                    if (_selectedInventoryItem != null)
                    {
                        tbPrice.DecimalValue = _selectedInventoryItem.MktPrice;
                        tbPrice.Text = _selectedInventoryItem.MktPrice.ToString("C2", CultureInfo.GetCultureInfo("en-US"));
                    }

                    MessageBox.Show(priceUpdatedMessage, "Price Updated", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                return newPrice;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Operation failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            finally
            {
                if (this.IsHandleCreated && this.InvokeRequired)
                {
                    this.Invoke(() =>
                    {
                        btnAddCt.Enabled = true;
                        Cursor.Current = prevCursor;
                    });
                }
                else
                {
                    btnAddCt.Enabled = true;
                    Cursor.Current = prevCursor;
                }
            }

            async Task _inventory_service_update_guard(int gameId, int cardId, decimal price)
            {
                // Forward to inventory service (signature changed to accept int cardGameId)
                await _inventoryService.UpdatePriceAsync(gameId, cardId, price);
            }
        }

        // Inline helper to set priceUp2Date for a given card row (now accepts cardGameId).
        private async Task UpdatePriceUp2DateInDbAsync(int cardGameId, int cardId, bool up2Date)
        {
            // Resolve table name using central mapping (DatabaseName + "Inventory")
            if (!SelectedCardGameLogic.TryGetById(cardGameId, out var game))
                throw new ArgumentException("Invalid card game id", nameof(cardGameId));

            var tableName = string.Concat(game.DatabaseName, "Inventory");

            var settingsStore = new JsonSettingsStore(AppPaths.SettingsPath);
            var appSettings = settingsStore.Load();
            var connectionFactory = new SqlConnectionFactory(appSettings);

            var password = await Session.PasswordProvider.GetPasswordAsync().ConfigureAwait(false);
            using var conn = connectionFactory.CreateForCurrentUser(password);
            password = string.Empty;

            await conn.OpenAsync().ConfigureAwait(false);

            var sql = $@"UPDATE {tableName} SET priceUp2Date = @up2 WHERE cardId = @cardId;";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@up2", SqlDbType.Bit).Value = up2Date;
            cmd.Parameters.Add("@cardId", SqlDbType.Int).Value = cardId;

            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
        }

        private async void btnFinalizeSale_Click(object sender, EventArgs e)
        {
            if (_cartBinding.Count == 0)
            {
                MessageBox.Show("Cart is empty. Add items before finalizing.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var settingsStore = new JsonSettingsStore(AppPaths.SettingsPath);
            var appSettings = settingsStore.Load();
            var connectionFactory = new SqlConnectionFactory(appSettings);
            var workflow = new SaleWorkflowService(connectionFactory, Session.PasswordProvider.GetPasswordAsync);

            // Do not use the legacy placeholder employee id (1) as the default.
            // Start with null so Sale row is not incorrectly attributed before PIN verification.
            int? employeeIdInt = null;
            var station = new DetermineStation();
            int registerIdInt = station.registerId();

            btnFinalizeSale.Enabled = false;
            var prevCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;

            try
            {
                // Determine whether we should create a new sale row or reuse an existing reserved one.
                int saleId;
                var providedSaleId = _reservedSaleId;

                if (providedSaleId.HasValue)
                {
                    // Check if the reserved sale row already exists.
                    // NOTE: do NOT use ConfigureAwait(false) here so continuation runs on UI thread.
                    var existingStatus = await workflow.GetSaleOrderStatusAsync(providedSaleId.Value);

                    if (existingStatus == null)
                    {
                        // No existing row -> create with providedSaleId
                        saleId = await workflow.CreateSaleAsync(DateTimeOffset.Now,
                            providedSaleId: providedSaleId,
                            registerId: (byte)registerIdInt,
                            employeeId: employeeIdInt.HasValue ? (byte?)employeeIdInt.Value : null,
                            orderStatus: OrderStatus.TakingOrder.ToString());

                        // Keep _reservedSaleId set to the same value
                        _reservedSaleId = saleId;
                    }
                    else
                    {
                        // Row exists -> reuse it. Ensure its status is TakingOrder so UI behavior is consistent.
                        saleId = providedSaleId.Value;
                        await workflow.UpdateSaleStatusAsync(saleId, OrderStatus.TakingOrder.ToString());
                    }
                }
                else
                {
                    // If your DB requires a saleId (not identity), we must have reserved one.
                    // Guard here to avoid inserting a NULL saleId.
                    MessageBox.Show("No reserved Sale ID available. Please retry (press Home then return to Sales if necessary).", "Missing Sale ID", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Show custom dialogs on the UI thread.
                bool userReturned;
                if (this.IsHandleCreated && this.InvokeRequired)
                {
                    userReturned = (bool)this.Invoke(new Func<bool>(() =>
                        CustomConfirm.ShowTwoButton(this,
                            "Gather Items",
                            "Items selected. Go to the back and obtain the items now.\n\nPress Return to go back and edit the order, or Next to continue to processing.",
                            "Return",
                            "Next")));
                }
                else
                {
                    userReturned = CustomConfirm.ShowTwoButton(this,
                        "Gather Items",
                        "Items selected. Go to the back and obtain the items now.\n\nPress Return to go back and edit the order, or Next to continue to processing.",
                        "Return",
                        "Next");
                }

                if (userReturned)
                {
                    await workflow.UpdateSaleStatusAsync(saleId, OrderStatus.TakingOrder.ToString());

                    if (this.IsHandleCreated && this.InvokeRequired)
                    {
                        this.Invoke(() => MessageBox.Show("Order set back to 'TakingOrder'. You may modify the cart.", "Returned", MessageBoxButtons.OK, MessageBoxIcon.Information));
                    }
                    else
                    {
                        MessageBox.Show("Order set back to 'TakingOrder'. You may modify the cart.", "Returned", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }

                    return;
                }

                // Next pressed -> set Processing
                await workflow.UpdateSaleStatusAsync(saleId, OrderStatus.Processing.ToString());

                // Prompt for employee PIN to determine which employee will be recorded as processing this payment.
                var pinPrompter = new DetermineCurrentUser(connectionFactory, Session.PasswordProvider.GetPasswordAsync);
                int? processEmployeeId = await pinPrompter.PromptAndVerifyEmployeePinAsync(this);
                if (!processEmployeeId.HasValue)
                {
                    // Authentication cancelled/failed: set order back to TakingOrder and inform user.
                    await workflow.UpdateSaleStatusAsync(saleId, OrderStatus.TakingOrder.ToString());
                    if (this.IsHandleCreated && this.InvokeRequired)
                    {
                        this.Invoke(() => MessageBox.Show("PIN verification failed or cancelled. Returning to edit.", "Authentication", MessageBoxButtons.OK, MessageBoxIcon.Warning));
                    }
                    else
                    {
                        MessageBox.Show("PIN verification failed or cancelled. Returning to edit.", "Authentication", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }

                    return;
                }

                // Use this employee id for subsequent processing and persist it on the sale row immediately.
                employeeIdInt = processEmployeeId.Value;

                // IMPORTANT: do NOT ConfigureAwait(false) here — we need to continue on UI thread afterwards.
                await workflow.UpdateSaleEmployeeAsync(saleId, (byte)employeeIdInt);

                // Second dialog: Return / Process (also run on UI thread)
                bool returnToEdit;
                if (this.IsHandleCreated && this.InvokeRequired)
                {
                    returnToEdit = (bool)this.Invoke(new Func<bool>(() =>
                        CustomConfirm.ShowTwoButton(this,
                            "Process Payment",
                            "Begin processing payment now.\n\nPress Return to go back and edit the order, or Process to complete the sale.",
                            "Return",
                            "Process")));
                }
                else
                {
                    returnToEdit = CustomConfirm.ShowTwoButton(this,
                        "Process Payment",
                        "Begin processing payment now.\n\nPress Return to go back and edit the order, or Process to complete the sale.",
                        "Return",
                        "Process");
                }

                if (returnToEdit)
                {
                    await workflow.UpdateSaleStatusAsync(saleId, OrderStatus.TakingOrder.ToString());

                    if (this.IsHandleCreated && this.InvokeRequired)
                    {
                        this.Invoke(() => MessageBox.Show("Order set back to 'TakingOrder'. You may modify the cart.", "Returned", MessageBoxButtons.OK, MessageBoxIcon.Information));
                    }
                    else
                    {
                        MessageBox.Show("Order set back to 'TakingOrder'. You may modify the cart.", "Returned", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    return;
                }

                // Process pressed -> set ReadyForPickup and save transaction lines
                await workflow.UpdateSaleStatusAsync(saleId, OrderStatus.ReadyForPickup.ToString());

                // Save transaction lines and update inventory
                await workflow.SaveTransactionLinesAsync(saleId, _cartBinding.ToList());

                // On success, set FinishedAndPaid
                await workflow.UpdateSaleStatusAsync(saleId, OrderStatus.FinishedAndPaid.ToString());

                if (this.IsHandleCreated && this.InvokeRequired)
                {
                    this.Invoke(() => MessageBox.Show("Sale completed and paid. Clearing cart.", "Finished", MessageBoxButtons.OK, MessageBoxIcon.Information));
                }
                else
                {
                    MessageBox.Show("Sale completed and paid. Clearing cart.", "Finished", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                // Reset UI for next sale -> marshal to UI thread explicitly to be safe
                if (this.IsHandleCreated && this.InvokeRequired)
                {
                    this.Invoke(() => ResetFormForNextSale());
                }
                else
                {
                    ResetFormForNextSale();
                }

                // Reserve next sale id (await on UI thread)
                await ReserveSaleIdAsync();
            }
            catch (Exception ex)
            {
                // Attempt to set status back to TakingOrder when errors occur (best-effort)
                try
                {
                    if (_reservedSaleId.HasValue)
                    {
                        var settings = new JsonSettingsStore(AppPaths.SettingsPath).Load();
                        var cf = new SqlConnectionFactory(settings);
                        var wf = new SaleWorkflowService(cf, Session.PasswordProvider.GetPasswordAsync);
                        // best-effort, ignore errors
                        await wf.UpdateSaleStatusAsync(_reservedSaleId.Value, OrderStatus.TakingOrder.ToString());
                    }
                }
                catch { }

                if (this.IsHandleCreated && this.InvokeRequired)
                {
                    this.Invoke(() => MessageBox.Show($"Finalize failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error));
                }
                else
                {
                    MessageBox.Show($"Finalize failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            finally
            {
                // Ensure UI updates run on UI thread
                if (this.IsHandleCreated && this.InvokeRequired)
                {
                    this.Invoke(() =>
                    {
                        btnFinalizeSale.Enabled = true;
                        Cursor.Current = prevCursor;
                    });
                }
                else
                {
                    btnFinalizeSale.Enabled = true;
                    Cursor.Current = prevCursor;
                }
            }
        }

        private void ImageResizing()
        {
            double imgHeight = tLP_img.Height;
            double imgWantedWidth = imgHeight * 0.84;
            int imgWidth = Convert.ToInt32(imgWantedWidth);

            tLP_img.Width = imgWidth;
        }
        private void PositionLabels()
        {
            if (this.Width >= 1200)
            {
                //int spacing = 20;

                //// Align lblInStock to be 20px to the right of tLP_img, and same top
                //lblInStock.Location = new Point(tLP_img.Right + spacing, tLP_img.Top);

                //// Position lblMarketPrice below lblInStock
                //lblMktPrice.Location = new Point(lblInStock.Left, lblInStock.Bottom + 10);

                ////Position tbPrice text box
                //tbPrice.Location = new Point(lblMktPrice.Left, lblMktPrice.Bottom + 10);

                ////Position tbAmtTraded text box
                //tbAmtTraded.Location = new Point(lblMktPrice.Right + 25, lblMktPrice.Bottom + 10);

                ////Position add2cart button
                //btnAddCt.Location = new Point(tbPrice.Left, tbPrice.Bottom + 10);
                ////Sale Info label stays where its at
            }
        }

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
                Width = 400,
                MinimumWidth = 200
            });

            sfDataGrid_InvLookup.Columns.Add(new GridTextColumn
            {
                MappingName = nameof(InventoryItem.Abbreviation),
                HeaderText = "Abbrev",
                Width = 120,
                MinimumWidth = 80
            });

            sfDataGrid_InvLookup.Columns.Add(new GridTextColumn
            {
                MappingName = nameof(InventoryItem.Rarity),
                HeaderText = "Rarity",
                Width = 200,
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
                FormatMode = FormatMode.Currency,
                NumberFormatInfo = new NumberFormatInfo
                {
                    CurrencyDecimalDigits = 2,
                    CurrencySymbol = "$"
                }
            });

            // Changed: show text status ("Not Inv", "Sold Out", or quantity)
            sfDataGrid_InvLookup.Columns.Add(new GridTextColumn
            {
                MappingName = nameof(InventoryItem.AmtInStockDisplay),
                HeaderText = "In Stock",
                Width = 1,
                MinimumWidth = 100
            });

            sfDataGrid_InvLookup.Columns.Add(new GridCheckBoxColumn
            {
                MappingName = nameof(InventoryItem.PriceUp2Date),
                HeaderText = "Up 2 Date",
                Width = 1,
                MinimumWidth = 100
            });

            sfDataGrid_InvLookup.Columns.Add(new GridTextColumn
            {
                MappingName = nameof(InventoryItem.ImageUrl),
                Visible = false
            });

            sfDataGrid_InvLookup.Columns.Add(new GridTextColumn
            {
                MappingName = nameof(InventoryItem.MktPriceUrl),
                Visible = false
            });

            sfDataGrid_InvLookup.Columns.Add(new GridNumericColumn
            {
                MappingName = nameof(InventoryItem.CardId),
                Visible = false
            });

            // Keep numeric value available if needed for future sort/filter logic
            sfDataGrid_InvLookup.Columns.Add(new GridNumericColumn
            {
                MappingName = nameof(InventoryItem.AmtInStock),
                Visible = false
            });
        }

        private void BuySell_FormClosed(object sender, FormClosedEventArgs e)
        {
            // Do NOT call base.OnFormClosed(e) here. This method is a handler wired to the FormClosed event.
            // Calling base.OnFormClosed(e) from an event handler can re-trigger the event and cause recursion.
            //
            // If you need to override OnFormClosed, implement:
            // protected override void OnFormClosed(FormClosedEventArgs e) { /* cleanup */ base.OnFormClosed(e); }
            //
            // Clean up any resources you own here (no long-lived finder field remains).
            // If you later add disposable resources, dispose them here safely.

            if (IsNavigating)
            {
                return; // Do nothing if navigating to another form
            }
            else
            {
                Application.Exit(); // Exit the application if not navigating
            }
        }

        private void tbSearchBar_TextChanged(object sender, EventArgs e)
        {

        }
        private void tbSearchBar_KeyDown(object sender, KeyEventArgs e)
        {
            // If enter key is pressed, trigger the btnSearch click event
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true; // Prevent the 'ding' sound
                btnSearch.PerformClick();
            }
        }

        private async void btnSearch_Click(object sender, EventArgs e)
        {
            // Validate inventory service if this form was constructed without DI (designer)
            if (_inventoryService == null)
            {
                MessageBox.Show("Inventory service is not initialized. Open Buy/Sell from the HomePage so the application can create a database service.", "Service Unavailable", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Read numeric CardGameId from the new SelectCardGameControl
            var cardGameId = selectCardGameControl1.SelectedCardGameId;
            if (cardGameId < 0)
            {
                MessageBox.Show("Please select a card game.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var searchText = tbSearchBar.Text ?? string.Empty;

            // UI: disable search and show wait cursor
            btnSearch.Enabled = false;
            var previousCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;

            try
            {
                // Call the inventory service passing numeric cardGameId
                var results = await _inventoryService.SearchInventoryAsync(cardGameId, searchText);

                // Bind results to grid on UI thread
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
        }

        // Stuff for handling selecting individual items in the grid
        private InventoryItem? _selectedInventoryItem;

        private void sfDataGrid_InvLookup_Click(object sender, EventArgs e)
        {

        }

        private void sfDataGrid_InvLookup_CellClick(object sender, Syncfusion.WinForms.DataGrid.Events.CellClickEventArgs e)
        {

        }

        private void sfDataGrid_InvLookup_SelectionChanged(object sender, Syncfusion.WinForms.DataGrid.Events.SelectionChangedEventArgs e)
        {
            // On click:

            // Gather relevant info about the selected item (CardName, MktPrice, ImageUrl, etc.)
            GrabSelectedInventoryItemData();

            // Load the card image asynchronously (if image URL is available)
            if (_selectedInventoryItem != null)
            {
                _ = LoadCardImageAsync_TCGPlayer(_selectedInventoryItem.ImageUrl);
            }
            else
            {
                imgCardUrl.Image = null;
            }

        }
        // Assemble InventoryItem data:
        private void GrabSelectedInventoryItemData()
        {
            // Use SelectedItem when you have single selection mode
            var item = sfDataGrid_InvLookup.SelectedItem as InventoryItem;
            if (item == null)
            {
                _selectedInventoryItem = null;
                tbPrice.Text = string.Empty;
                tbPrice.DecimalValue = 0m;
                lblAmtInStock.Text = "N/A";
                return;
            }

            _selectedInventoryItem = item;

            // Display market price using US Dollar formatting and keep numeric value set.
            // Text is formatted for display; DecimalValue keeps the numeric value for calculations.
            tbPrice.Text = item.MktPrice.ToString("C2", CultureInfo.GetCultureInfo("en-US"));
            tbPrice.DecimalValue = item.MktPrice;
            // Also Display the amount in stock value
            lblAmtInStock.Text = $"{item.AmtInStock}";
        }

        // Image
        private static readonly HttpClient httpClient = new HttpClient();
        private void imgCardUrl_Click(object sender, EventArgs e)
        {

        }


        private async Task LoadCardImageAsync_TCGPlayer(string imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                imgCardUrl.Image = null;
                return;
            }

            try
            {
                // Spoof headers to avoid 403 blocks
                httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
                httpClient.DefaultRequestHeaders.Referrer = new Uri("https://www.tcgplayer.com/");
                httpClient.DefaultRequestHeaders.Accept.ParseAdd("image/webp,image/apng,image/*,*/*;q=0.8");

                // Request
                var stream = await httpClient.GetStreamAsync(imageUrl);

                // Load image from stream
                using (stream)
                {
                    imgCardUrl.Image = Image.FromStream(stream);
                }
            }
            catch (Exception ex)
            {
                imgCardUrl.Image = null;
                MessageBox.Show($"Unable to load image.\n\nError: {ex.Message}",
                    "Image Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Updated: Add-to-cart now looks up the price only if priceUp2Date == false.
        private async void btnAddCt_Click(object sender, EventArgs e)
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

            // If price is stale, refresh it and STOP here so user can re-confirm/edit values.
            if (!_selectedInventoryItem.PriceUp2Date)
            {
                await PriceLookupAndUpdate();
                return;
            }

            decimal agreedPrice = tbPrice.DecimalValue;
            decimal timeMktPrice = _selectedInventoryItem.MktPrice;

            int amtTraded;
            try
            {
                amtTraded = Convert.ToInt32(tbAmtTraded.IntegerValue);
                if (amtTraded <= 0)
                {
                    MessageBox.Show("Please enter a positive amount.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
            catch
            {
                MessageBox.Show("Invalid amount entered.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int cardGameId = selectedGame.CardGameId;

            var line = new TransactionLineItem
            {
                CardGameId = cardGameId,
                CardId = _selectedInventoryItem.CardId,
                ConditionId = _selectedInventoryItem.ConditionId,
                CardName = _selectedInventoryItem.CardName,
                Rarity = _selectedInventoryItem.Rarity,
                SetId = _selectedInventoryItem.SetId,
                TimeMktPrice = timeMktPrice,
                AgreedPrice = agreedPrice,
                AmtTraded = amtTraded,
                AmtInStock = _selectedInventoryItem.AmtInStock,
                BuyOrSell = true
            };

            _cartBinding.Add(line);
            btnFinalizeSale.Visible = true;
        }

        private void headerControl1_Load(object sender, EventArgs e)
        {

        }
        // Pricing


        private void sfDataGrid_Cart_Click(object sender, EventArgs e)
        {

        }

        private void tbPrice_TextChanged(object sender, EventArgs e)
        {

        }

       
    }
}