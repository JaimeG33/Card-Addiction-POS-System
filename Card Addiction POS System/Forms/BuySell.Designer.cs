namespace Card_Addiction_POS_System.Forms
{
    partial class BuySell
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            headerControl1 = new Card_Addiction_POS_System.LogIn.Controls.HeaderControl();
            tableLayoutPanel1 = new TableLayoutPanel();
            tableLayoutPanel2 = new TableLayoutPanel();
            tLP_img = new TableLayoutPanel();
            imgCardUrl = new PictureBox();
            panel1 = new Panel();
            lblAmt = new Label();
            lblAmtInStock = new Label();
            btnFinalizeSale = new Button();
            lblSaleInfo = new Label();
            btnAddCt = new Button();
            tbAmtTraded = new Syncfusion.Windows.Forms.Tools.IntegerTextBox();
            tbPrice = new Syncfusion.Windows.Forms.Tools.CurrencyTextBox();
            lblMktPrice = new Label();
            lblInStock = new Label();
            tableLayoutPanel4 = new TableLayoutPanel();
            sfDataGrid_Cart = new Syncfusion.WinForms.DataGrid.SfDataGrid();
            sfDataGrid_CartSummary = new Syncfusion.WinForms.DataGrid.SfDataGrid();
            tableLayoutPanel3 = new TableLayoutPanel();
            flowLayoutPanel1 = new FlowLayoutPanel();
            selectCardGameControl1 = new Card_Addiction_POS_System.Forms.Controls.SelectCardGameControl();
            tbSearchBar = new TextBox();
            btnSearch = new Button();
            sfDataGrid_InvLookup = new Syncfusion.WinForms.DataGrid.SfDataGrid();
            btnPrintCart = new Button();
            tableLayoutPanel1.SuspendLayout();
            tableLayoutPanel2.SuspendLayout();
            tLP_img.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)imgCardUrl).BeginInit();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)tbAmtTraded).BeginInit();
            ((System.ComponentModel.ISupportInitialize)tbPrice).BeginInit();
            tableLayoutPanel4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)sfDataGrid_Cart).BeginInit();
            ((System.ComponentModel.ISupportInitialize)sfDataGrid_CartSummary).BeginInit();
            tableLayoutPanel3.SuspendLayout();
            flowLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)sfDataGrid_InvLookup).BeginInit();
            SuspendLayout();
            // 
            // headerControl1
            // 
            headerControl1.BackColor = SystemColors.ActiveBorder;
            headerControl1.Dock = DockStyle.Top;
            headerControl1.Location = new Point(2, 2);
            headerControl1.Margin = new Padding(3, 5, 3, 5);
            headerControl1.MaximumSize = new Size(0, 147);
            headerControl1.Name = "headerControl1";
            headerControl1.Size = new Size(1360, 147);
            headerControl1.TabIndex = 0;
            headerControl1.Load += headerControl1_Load;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
            tableLayoutPanel1.Controls.Add(tableLayoutPanel2, 0, 0);
            tableLayoutPanel1.Controls.Add(tableLayoutPanel3, 1, 0);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(2, 149);
            tableLayoutPanel1.Margin = new Padding(3, 4, 3, 4);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 1;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Size = new Size(1360, 701);
            tableLayoutPanel1.TabIndex = 1;
            // 
            // tableLayoutPanel2
            // 
            tableLayoutPanel2.ColumnCount = 1;
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel2.Controls.Add(tLP_img, 0, 0);
            tableLayoutPanel2.Controls.Add(tableLayoutPanel4, 0, 1);
            tableLayoutPanel2.Dock = DockStyle.Fill;
            tableLayoutPanel2.Location = new Point(3, 4);
            tableLayoutPanel2.Margin = new Padding(3, 4, 3, 4);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 2;
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Absolute, 27F));
            tableLayoutPanel2.Size = new Size(538, 693);
            tableLayoutPanel2.TabIndex = 0;
            // 
            // tLP_img
            // 
            tLP_img.ColumnCount = 2;
            tLP_img.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tLP_img.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tLP_img.Controls.Add(imgCardUrl, 0, 0);
            tLP_img.Controls.Add(panel1, 1, 0);
            tLP_img.Dock = DockStyle.Fill;
            tLP_img.Location = new Point(3, 4);
            tLP_img.Margin = new Padding(3, 4, 3, 4);
            tLP_img.Name = "tLP_img";
            tLP_img.Padding = new Padding(3, 4, 3, 4);
            tLP_img.RowCount = 1;
            tLP_img.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tLP_img.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tLP_img.Size = new Size(532, 338);
            tLP_img.TabIndex = 0;
            // 
            // imgCardUrl
            // 
            imgCardUrl.BorderStyle = BorderStyle.FixedSingle;
            imgCardUrl.Dock = DockStyle.Fill;
            imgCardUrl.Location = new Point(6, 8);
            imgCardUrl.Margin = new Padding(3, 4, 3, 4);
            imgCardUrl.Name = "imgCardUrl";
            imgCardUrl.Size = new Size(257, 322);
            imgCardUrl.SizeMode = PictureBoxSizeMode.StretchImage;
            imgCardUrl.TabIndex = 0;
            imgCardUrl.TabStop = false;
            imgCardUrl.Click += imgCardUrl_Click;
            // 
            // panel1
            // 
            panel1.Controls.Add(btnPrintCart);
            panel1.Controls.Add(lblAmt);
            panel1.Controls.Add(lblAmtInStock);
            panel1.Controls.Add(btnFinalizeSale);
            panel1.Controls.Add(lblSaleInfo);
            panel1.Controls.Add(btnAddCt);
            panel1.Controls.Add(tbAmtTraded);
            panel1.Controls.Add(tbPrice);
            panel1.Controls.Add(lblMktPrice);
            panel1.Controls.Add(lblInStock);
            panel1.Dock = DockStyle.Fill;
            panel1.Location = new Point(269, 8);
            panel1.Margin = new Padding(3, 4, 3, 4);
            panel1.Name = "panel1";
            panel1.Size = new Size(257, 322);
            panel1.TabIndex = 1;
            // 
            // lblAmt
            // 
            lblAmt.AutoSize = true;
            lblAmt.Location = new Point(135, 73);
            lblAmt.Name = "lblAmt";
            lblAmt.Size = new Size(65, 20);
            lblAmt.TabIndex = 8;
            lblAmt.Text = "Amount:";
            // 
            // lblAmtInStock
            // 
            lblAmtInStock.AutoSize = true;
            lblAmtInStock.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblAmtInStock.Location = new Point(133, 9);
            lblAmtInStock.Name = "lblAmtInStock";
            lblAmtInStock.Size = new Size(0, 28);
            lblAmtInStock.TabIndex = 7;
            // 
            // btnFinalizeSale
            // 
            btnFinalizeSale.Location = new Point(134, 260);
            btnFinalizeSale.Margin = new Padding(3, 4, 3, 4);
            btnFinalizeSale.Name = "btnFinalizeSale";
            btnFinalizeSale.Size = new Size(98, 31);
            btnFinalizeSale.TabIndex = 6;
            btnFinalizeSale.Text = "Finalize Sale";
            btnFinalizeSale.UseVisualStyleBackColor = true;
            btnFinalizeSale.Click += btnFinalizeSale_Click;
            // 
            // lblSaleInfo
            // 
            lblSaleInfo.AutoSize = true;
            lblSaleInfo.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblSaleInfo.Location = new Point(14, 263);
            lblSaleInfo.Name = "lblSaleInfo";
            lblSaleInfo.Size = new Size(80, 23);
            lblSaleInfo.TabIndex = 5;
            lblSaleInfo.Text = "Sale Info:";
            // 
            // btnAddCt
            // 
            btnAddCt.Location = new Point(14, 152);
            btnAddCt.Margin = new Padding(3, 4, 3, 4);
            btnAddCt.Name = "btnAddCt";
            btnAddCt.Size = new Size(98, 31);
            btnAddCt.TabIndex = 4;
            btnAddCt.Text = "Add to Cart";
            btnAddCt.UseVisualStyleBackColor = true;
            btnAddCt.Click += btnAddCt_Click;
            // 
            // tbAmtTraded
            // 
            tbAmtTraded.AccessibilityEnabled = true;
            tbAmtTraded.BeforeTouchSize = new Size(114, 27);
            tbAmtTraded.IntegerValue = 1L;
            tbAmtTraded.Location = new Point(151, 97);
            tbAmtTraded.Margin = new Padding(3, 4, 3, 4);
            tbAmtTraded.Name = "tbAmtTraded";
            tbAmtTraded.Size = new Size(33, 27);
            tbAmtTraded.TabIndex = 3;
            tbAmtTraded.Text = "1";
            // 
            // tbPrice
            // 
            tbPrice.AccessibilityEnabled = true;
            tbPrice.BeforeTouchSize = new Size(114, 27);
            tbPrice.DecimalValue = new decimal(new int[] { 100, 0, 0, 131072 });
            tbPrice.Location = new Point(14, 97);
            tbPrice.Margin = new Padding(3, 4, 3, 4);
            tbPrice.Name = "tbPrice";
            tbPrice.Size = new Size(114, 27);
            tbPrice.TabIndex = 2;
            tbPrice.Text = "$1.00";
            tbPrice.TextChanged += tbPrice_TextChanged;
            // 
            // lblMktPrice
            // 
            lblMktPrice.AutoSize = true;
            lblMktPrice.Location = new Point(14, 73);
            lblMktPrice.Name = "lblMktPrice";
            lblMktPrice.Size = new Size(94, 20);
            lblMktPrice.TabIndex = 1;
            lblMktPrice.Text = "Market Price:";
            // 
            // lblInStock
            // 
            lblInStock.AutoSize = true;
            lblInStock.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblInStock.Location = new Point(6, 12);
            lblInStock.Name = "lblInStock";
            lblInStock.Size = new Size(141, 23);
            lblInStock.TabIndex = 0;
            lblInStock.Text = "Amount In Stock:";
            // 
            // tableLayoutPanel4
            // 
            tableLayoutPanel4.ColumnCount = 1;
            tableLayoutPanel4.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel4.Controls.Add(sfDataGrid_Cart, 0, 0);
            tableLayoutPanel4.Controls.Add(sfDataGrid_CartSummary, 0, 1);
            tableLayoutPanel4.Dock = DockStyle.Fill;
            tableLayoutPanel4.Location = new Point(3, 350);
            tableLayoutPanel4.Margin = new Padding(3, 4, 3, 4);
            tableLayoutPanel4.Name = "tableLayoutPanel4";
            tableLayoutPanel4.RowCount = 2;
            tableLayoutPanel4.RowStyles.Add(new RowStyle(SizeType.Percent, 65F));
            tableLayoutPanel4.RowStyles.Add(new RowStyle(SizeType.Percent, 35F));
            tableLayoutPanel4.Size = new Size(532, 339);
            tableLayoutPanel4.TabIndex = 1;
            // 
            // sfDataGrid_Cart
            // 
            sfDataGrid_Cart.AccessibleName = "Table";
            sfDataGrid_Cart.BackColor = Color.SkyBlue;
            sfDataGrid_Cart.Dock = DockStyle.Fill;
            sfDataGrid_Cart.Location = new Point(3, 4);
            sfDataGrid_Cart.Margin = new Padding(3, 4, 3, 4);
            sfDataGrid_Cart.Name = "sfDataGrid_Cart";
            sfDataGrid_Cart.PreviewRowHeight = 35;
            sfDataGrid_Cart.Size = new Size(526, 212);
            sfDataGrid_Cart.Style.BorderColor = Color.FromArgb(100, 100, 100);
            sfDataGrid_Cart.Style.DragPreviewRowStyle.Font = new Font("Segoe UI", 9F);
            sfDataGrid_Cart.Style.DragPreviewRowStyle.RowCountIndicatorTextColor = Color.FromArgb(255, 255, 255);
            sfDataGrid_Cart.TabIndex = 1;
            sfDataGrid_Cart.Text = "sfDataGrid1";
            sfDataGrid_Cart.Click += sfDataGrid_Cart_Click;
            // 
            // sfDataGrid_CartSummary
            // 
            sfDataGrid_CartSummary.AccessibleName = "Table";
            sfDataGrid_CartSummary.BackColor = Color.White;
            sfDataGrid_CartSummary.Dock = DockStyle.Fill;
            sfDataGrid_CartSummary.Location = new Point(3, 224);
            sfDataGrid_CartSummary.Margin = new Padding(3, 4, 3, 4);
            sfDataGrid_CartSummary.Name = "sfDataGrid_CartSummary";
            sfDataGrid_CartSummary.PreviewRowHeight = 35;
            sfDataGrid_CartSummary.Size = new Size(526, 111);
            sfDataGrid_CartSummary.Style.BorderColor = Color.FromArgb(100, 100, 100);
            sfDataGrid_CartSummary.Style.DragPreviewRowStyle.Font = new Font("Segoe UI", 9F);
            sfDataGrid_CartSummary.Style.DragPreviewRowStyle.RowCountIndicatorTextColor = Color.FromArgb(255, 255, 255);
            sfDataGrid_CartSummary.TabIndex = 2;
            sfDataGrid_CartSummary.Text = "Cart Summary";
            // 
            // tableLayoutPanel3
            // 
            tableLayoutPanel3.ColumnCount = 1;
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel3.Controls.Add(flowLayoutPanel1, 0, 0);
            tableLayoutPanel3.Controls.Add(sfDataGrid_InvLookup, 0, 1);
            tableLayoutPanel3.Dock = DockStyle.Fill;
            tableLayoutPanel3.Location = new Point(547, 4);
            tableLayoutPanel3.Margin = new Padding(3, 4, 3, 4);
            tableLayoutPanel3.Name = "tableLayoutPanel3";
            tableLayoutPanel3.RowCount = 2;
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Percent, 80F));
            tableLayoutPanel3.Size = new Size(810, 693);
            tableLayoutPanel3.TabIndex = 1;
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.Controls.Add(selectCardGameControl1);
            flowLayoutPanel1.Controls.Add(tbSearchBar);
            flowLayoutPanel1.Controls.Add(btnSearch);
            flowLayoutPanel1.Dock = DockStyle.Fill;
            flowLayoutPanel1.Location = new Point(3, 4);
            flowLayoutPanel1.Margin = new Padding(3, 4, 3, 4);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(804, 130);
            flowLayoutPanel1.TabIndex = 0;
            // 
            // selectCardGameControl1
            // 
            selectCardGameControl1.Location = new Point(3, 5);
            selectCardGameControl1.Margin = new Padding(3, 5, 3, 5);
            selectCardGameControl1.Name = "selectCardGameControl1";
            selectCardGameControl1.SelectedCardGameId = -1;
            selectCardGameControl1.Size = new Size(198, 35);
            selectCardGameControl1.TabIndex = 3;
            // 
            // tbSearchBar
            // 
            tbSearchBar.Dock = DockStyle.Bottom;
            tbSearchBar.Location = new Point(3, 53);
            tbSearchBar.Margin = new Padding(3, 4, 3, 4);
            tbSearchBar.Name = "tbSearchBar";
            tbSearchBar.Size = new Size(695, 27);
            tbSearchBar.TabIndex = 1;
            tbSearchBar.TextChanged += tbSearchBar_TextChanged;
            tbSearchBar.KeyDown += tbSearchBar_KeyDown;
            // 
            // btnSearch
            // 
            btnSearch.Location = new Point(704, 49);
            btnSearch.Margin = new Padding(3, 4, 3, 4);
            btnSearch.Name = "btnSearch";
            btnSearch.Size = new Size(86, 31);
            btnSearch.TabIndex = 2;
            btnSearch.Text = "Search";
            btnSearch.UseVisualStyleBackColor = true;
            btnSearch.Click += btnSearch_Click;
            // 
            // sfDataGrid_InvLookup
            // 
            sfDataGrid_InvLookup.AccessibleName = "Table";
            sfDataGrid_InvLookup.AllowResizingColumns = true;
            sfDataGrid_InvLookup.BackColor = Color.White;
            sfDataGrid_InvLookup.Dock = DockStyle.Fill;
            sfDataGrid_InvLookup.Location = new Point(3, 142);
            sfDataGrid_InvLookup.Margin = new Padding(3, 4, 3, 4);
            sfDataGrid_InvLookup.Name = "sfDataGrid_InvLookup";
            sfDataGrid_InvLookup.PreviewRowHeight = 35;
            sfDataGrid_InvLookup.Size = new Size(804, 547);
            sfDataGrid_InvLookup.Style.BorderColor = Color.FromArgb(100, 100, 100);
            sfDataGrid_InvLookup.Style.DragPreviewRowStyle.Font = new Font("Segoe UI", 9F);
            sfDataGrid_InvLookup.Style.DragPreviewRowStyle.RowCountIndicatorTextColor = Color.FromArgb(255, 255, 255);
            sfDataGrid_InvLookup.TabIndex = 1;
            sfDataGrid_InvLookup.Text = "sfDataGrid_InvLookup";
            sfDataGrid_InvLookup.SelectionChanged += sfDataGrid_InvLookup_SelectionChanged;
            sfDataGrid_InvLookup.CellClick += sfDataGrid_InvLookup_CellClick;
            sfDataGrid_InvLookup.Click += sfDataGrid_InvLookup_Click;
            // 
            // btnPrintCart
            // 
            btnPrintCart.Location = new Point(14, 191);
            btnPrintCart.Margin = new Padding(3, 4, 3, 4);
            btnPrintCart.Name = "btnPrintCart";
            btnPrintCart.Size = new Size(133, 31);
            btnPrintCart.TabIndex = 9;
            btnPrintCart.Text = "Print Current Cart";
            btnPrintCart.UseVisualStyleBackColor = true;
            // 
            // BuySell
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1364, 852);
            Controls.Add(tableLayoutPanel1);
            Controls.Add(headerControl1);
            Margin = new Padding(3, 4, 3, 4);
            MinimumSize = new Size(1380, 891);
            Name = "BuySell";
            Style.MdiChild.IconHorizontalAlignment = HorizontalAlignment.Center;
            Style.MdiChild.IconVerticalAlignment = System.Windows.Forms.VisualStyles.VerticalAlignment.Center;
            Text = "BuySell";
            WindowState = FormWindowState.Maximized;
            FormClosed += BuySell_FormClosed;
            Load += BuySell_Load;
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel2.ResumeLayout(false);
            tLP_img.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)imgCardUrl).EndInit();
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)tbAmtTraded).EndInit();
            ((System.ComponentModel.ISupportInitialize)tbPrice).EndInit();
            tableLayoutPanel4.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)sfDataGrid_Cart).EndInit();
            ((System.ComponentModel.ISupportInitialize)sfDataGrid_CartSummary).EndInit();
            tableLayoutPanel3.ResumeLayout(false);
            flowLayoutPanel1.ResumeLayout(false);
            flowLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)sfDataGrid_InvLookup).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private LogIn.Controls.HeaderControl headerControl1;
        private TableLayoutPanel tableLayoutPanel1;
        private TableLayoutPanel tableLayoutPanel2;
        private TableLayoutPanel tLP_img;
        private PictureBox imgCardUrl;
        private Panel panel1;
        private Label lblInStock;
        private Label lblMktPrice;
        private Label lblSaleInfo;
        private Button btnAddCt;
        private Syncfusion.Windows.Forms.Tools.IntegerTextBox tbAmtTraded;
        private Syncfusion.Windows.Forms.Tools.CurrencyTextBox tbPrice;
        private Button btnFinalizeSale;
        private TableLayoutPanel tableLayoutPanel3;
        private FlowLayoutPanel flowLayoutPanel1;
        private Syncfusion.WinForms.DataGrid.SfDataGrid sfDataGrid_InvLookup;
        private TextBox tbSearchBar;
        private Button btnSearch;
        private Label lblAmtInStock;
        private Label lblAmt;
        private Syncfusion.WinForms.DataGrid.SfDataGrid sfDataGrid_Cart;
        private TableLayoutPanel tableLayoutPanel4;
        private Syncfusion.WinForms.DataGrid.SfDataGrid sfDataGrid_CartSummary;
        private Controls.SelectCardGameControl selectCardGameControl1;
        private Button btnPrintCart;
    }
}