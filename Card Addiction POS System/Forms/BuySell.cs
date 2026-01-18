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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Card_Addiction_POS_System.Functions.Inventory.SearchInventoryDB;

namespace Card_Addiction_POS_System.Forms
{
    public partial class BuySell : SfForm
    {
        private readonly IInventoryService _inventoryService;
        public BuySell(IInventoryService inventoryService)
        {
            InitializeComponent();
            _inventoryService = inventoryService;

            ConfigureInventoryGrid();
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
                int spacing = 20;

                // Align lblInStock to be 20px to the right of tLP_img, and same top
                lblInStock.Location = new Point(tLP_img.Right + spacing, tLP_img.Top);

                // Position lblMarketPrice below lblInStock
                lblMktPrice.Location = new Point(lblInStock.Left, lblInStock.Bottom + 10);

                //Position tbPrice text box
                tbPrice.Location = new Point(lblMktPrice.Left, lblMktPrice.Bottom + 10);

                //Position tbAmtTraded text box
                tbAmtTraded.Location = new Point(lblMktPrice.Right + 25, lblMktPrice.Bottom + 10);

                //Position add2cart button
                btnAddCt.Location = new Point(tbPrice.Left, tbPrice.Bottom + 10);
                //Sale Info label stays where its at
            }
        }

        private void ConfigureInventoryGrid()
        {
            sfDataGrid_InvLookup.AutoGenerateColumns = false;
            sfDataGrid_InvLookup.AllowSorting = true;
            sfDataGrid_InvLookup.AllowFiltering = true;

            sfDataGrid_InvLookup.Columns.Add(new GridTextColumn
            {
                MappingName = nameof(InventoryItem.CardName),
                HeaderText = "Card Name",
                Width = 250
            });

            sfDataGrid_InvLookup.Columns.Add(new GridTextColumn
            {
                MappingName = nameof(InventoryItem.Rarity),
                HeaderText = "Rarity"
            });

            sfDataGrid_InvLookup.Columns.Add(new GridNumericColumn
            {
                MappingName = nameof(InventoryItem.SetId),
                HeaderText = "Set",
                Width = 60
            });

            sfDataGrid_InvLookup.Columns.Add(new GridNumericColumn
            {
                MappingName = nameof(InventoryItem.MktPrice),
                HeaderText = "Market Price",
                Width = 80,
                FormatMode = FormatMode.Currency,
                NumberFormatInfo = new System.Globalization.NumberFormatInfo
                {
                    CurrencyDecimalDigits = 2
                }
            });

            sfDataGrid_InvLookup.Columns.Add(new GridNumericColumn
            {
                MappingName = nameof(InventoryItem.AmtInStock),
                HeaderText = "In Stock",
                Width = 60
            });

            sfDataGrid_InvLookup.Columns.Add(new GridCheckBoxColumn
            {
                MappingName = nameof(InventoryItem.PriceUp2Date),
                HeaderText = "Up 2 Date",
                Width = 60
            });

            // Hidden columns
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

        
    }
}
