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

namespace Card_Addiction_POS_System.Forms
{
    public partial class BuySell : SfForm
    {
        private readonly IInventoryService _inventoryService;
        private readonly FindPrice_TCG _tcgPriceFinder;
        public BuySell(IInventoryService inventoryService)
        {
            InitializeComponent();
            _inventoryService = inventoryService;
            _tcgPriceFinder = new FindPrice_TCG();

            // Allow typing in the search box even if Designer set it ReadOnly
            tbSearchBar.ReadOnly = false;
            tbSearchBar.Enabled = true;

            ConfigureInventoryGrid();

            // Ensure we receive selection notifications from the grid
            sfDataGrid_InvLookup.SelectionChanged += sfDataGrid_InvLookup_SelectionChanged;


        }

        public bool IsNavigating { get; set; }
        public virtual string FormTitle { get; set; } = "Sales";

        private void BuySell_Load(object sender, EventArgs e)
        {
            // Avoid running layout/resize code while Visual Studio Designer is rendering.
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
            {
                return;
            }

            // Resize image and adjust layout
            ImageResizing();
            PositionLabels();
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
            // Configure basic grid behavior
            sfDataGrid_InvLookup.AutoGenerateColumns = false;
            sfDataGrid_InvLookup.AllowSorting = true;
            sfDataGrid_InvLookup.AllowFiltering = true;
            sfDataGrid_InvLookup.AllowResizingColumns = true;

            // Let columns fill the available space but don't allow them to shrink below MinimumWidth.
            sfDataGrid_InvLookup.AutoSizeColumnsMode = AutoSizeColumnsMode.Fill;

            // Make the grid read-only and select whole rows on click.
            sfDataGrid_InvLookup.AllowEditing = false;
            sfDataGrid_InvLookup.SelectionMode = GridSelectionMode.Single;
            sfDataGrid_InvLookup.SelectionUnit = SelectionUnit.Row;

            // Clear any existing columns (safe to call multiple times)
            sfDataGrid_InvLookup.Columns.Clear();

            // Card name should take most of the space: give it a larger weight (Width acts as weight in Fill mode).
            sfDataGrid_InvLookup.Columns.Add(new GridTextColumn
            {
                MappingName = nameof(InventoryItem.CardName),
                HeaderText = "Card Name",
                Width = 475,
                MinimumWidth = 200
            });

            // Rarity
            sfDataGrid_InvLookup.Columns.Add(new GridTextColumn
            {
                MappingName = nameof(InventoryItem.Rarity),
                HeaderText = "Rarity",
                Width = 225,
                MinimumWidth = 100
            });

            // SetId: integer display, no decimals, no grouping/comma separator
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

            // Market price: currency with dollar sign and two decimals
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

            // AmtInStock: integer without decimals or grouping
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

            // Price up-to-date checkbox
            sfDataGrid_InvLookup.Columns.Add(new GridCheckBoxColumn
            {
                MappingName = nameof(InventoryItem.PriceUp2Date),
                HeaderText = "Up 2 Date",
                Width = 1,
                MinimumWidth = 100
            });

            // Hidden columns (keep them but not visible)
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
        }

        private void BuySell_FormClosed(object sender, FormClosedEventArgs e)
        {
            _tcgPriceFinder?.Dispose();    // clean up browser when done
            base.OnFormClosed(e);

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
            // Basic validation
            var cardGameKey = cbCardGame.SelectedItem as string ?? cbCardGame.Text;
            if (string.IsNullOrWhiteSpace(cardGameKey))
            {
                MessageBox.Show("Please select a card game.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var searchText = tbSearchBar.Text ?? string.Empty;

            // UI state (must run on UI thread)
            btnSearch.Enabled = false;
            var previousCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;

            try
            {
                // DO NOT use ConfigureAwait(false) here — we need to come back to the UI thread
                var results = await _inventoryService.SearchInventoryAsync(cardGameKey, searchText);

                // Safe to update controls because we're back on the UI thread
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

        // New: lookup selected item's price on TCGPlayer and update the model/UI
        private async void LookupPrice(object? sender, EventArgs e)
        {
            if (_selectedInventoryItem == null)
            {
                MessageBox.Show("Please select an item first.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var url = _selectedInventoryItem.MktPriceUrl;
            if (string.IsNullOrWhiteSpace(url))
            {
                MessageBox.Show("Selected item does not have a market price URL.", "No URL", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            btnFinalizeSale.Enabled = false;
            var previousCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;

            try
            {
                // Run Selenium lookup on a background thread using a short-lived finder instance
                var oldPrice = _selectedInventoryItem.MktPrice;
                decimal newPrice = await Task.Run(() =>
                {
                    using var finder = new FindPrice_TCG();
                    return finder.GetMarketPrice(url);
                }).ConfigureAwait(false);

                // Marshal back to UI thread to update UI
                if (this.IsHandleCreated && this.InvokeRequired)
                {
                    this.Invoke(() =>
                    {
                        _selectedInventoryItem.MktPrice = newPrice;
                        tbPrice.DecimalValue = newPrice;
                        tbPrice.Text = newPrice.ToString("C2", CultureInfo.GetCultureInfo("en-US"));
                        MessageBox.Show($"Price lookup succeeded.\n\nOld: {oldPrice.ToString("C2", CultureInfo.GetCultureInfo("en-US"))}\nNew: {newPrice.ToString("C2", CultureInfo.GetCultureInfo("en-US"))}",
                            "Price Updated", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    });
                }
                else
                {
                    _selectedInventoryItem.MktPrice = newPrice;
                    tbPrice.DecimalValue = newPrice;
                    tbPrice.Text = newPrice.ToString("C2", CultureInfo.GetCultureInfo("en-US"));
                    MessageBox.Show($"Price lookup succeeded.\n\nOld: {oldPrice.ToString("C2", CultureInfo.GetCultureInfo("en-US"))}\nNew: {newPrice.ToString("C2", CultureInfo.GetCultureInfo("en-US"))}",
                        "Price Updated", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                // Show friendly error; real app may log details
                if (this.IsHandleCreated && this.InvokeRequired)
                {
                    this.Invoke(() =>
                    {
                        MessageBox.Show($"Price lookup failed: {ex.Message}", "Lookup Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    });
                }
                else
                {
                    MessageBox.Show($"Price lookup failed: {ex.Message}", "Lookup Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            finally
            {
                btnFinalizeSale.Enabled = true;
                Cursor.Current = previousCursor;
            }
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

        private void btnFinalizeSale_Click(object sender, EventArgs e)
        {

        }

        // Updated: Add-to-cart now looks up the price, persists it, refreshes the grid and restores selection.
        private async void btnAddCt_Click(object sender, EventArgs e)
        {
            if (_selectedInventoryItem == null)
            {
                MessageBox.Show("Please select an item first.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var cardGameKey = cbCardGame.SelectedItem as string ?? cbCardGame.Text;
            if (string.IsNullOrWhiteSpace(cardGameKey))
            {
                MessageBox.Show("Please select a card game.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var url = _selectedInventoryItem.MktPriceUrl;
            if (string.IsNullOrWhiteSpace(url))
            {
                MessageBox.Show("Selected item does not have a market price URL.", "No URL", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Preserve identity of selected item to re-select after refresh
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
                });

                // 2) Persist to database
                await _inventoryService.UpdatePriceAsync(cardGameKey, selectedCardId, newPrice);

                // 3) Refresh the grid results with the same filter
                var searchText = tbSearchBar.Text ?? string.Empty;
                var refreshed = await _inventoryService.SearchInventoryAsync(cardGameKey, searchText);

                // Bind refreshed results (we are on UI thread here)
                sfDataGrid_InvLookup.DataSource = refreshed ?? null;

                // 4) Restore selection by CardId
                if (refreshed != null)
                {
                    var idx = refreshed.ToList().FindIndex(x => x.CardId == selectedCardId);
                    if (idx >= 0)
                    {
                        // SelectedIndex API available on SfDataGrid
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

                MessageBox.Show($"Price updated to {newPrice.ToString("C2", CultureInfo.GetCultureInfo("en-US"))}.",
                    "Price Updated", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Operation failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnAddCt.Enabled = true;
                Cursor.Current = prevCursor;
            }
        }
        // Pricing




    }
}
