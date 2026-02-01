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
            tableLayoutPanel3 = new TableLayoutPanel();
            flowLayoutPanel1 = new FlowLayoutPanel();
            cbCardGame = new ComboBox();
            tbSearchBar = new TextBox();
            btnSearch = new Button();
            sfDataGrid_InvLookup = new Syncfusion.WinForms.DataGrid.SfDataGrid();
            sfDataGrid_Cart = new Syncfusion.WinForms.DataGrid.SfDataGrid();
            tableLayoutPanel1.SuspendLayout();
            tableLayoutPanel2.SuspendLayout();
            tLP_img.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)imgCardUrl).BeginInit();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)tbAmtTraded).BeginInit();
            ((System.ComponentModel.ISupportInitialize)tbPrice).BeginInit();
            tableLayoutPanel3.SuspendLayout();
            flowLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)sfDataGrid_InvLookup).BeginInit();
            ((System.ComponentModel.ISupportInitialize)sfDataGrid_Cart).BeginInit();
            SuspendLayout();
            // 
            // headerControl1
            // 
            headerControl1.BackColor = SystemColors.ActiveBorder;
            headerControl1.Dock = DockStyle.Top;
            headerControl1.Location = new Point(2, 2);
            headerControl1.MaximumSize = new Size(0, 110);
            headerControl1.Name = "headerControl1";
            headerControl1.Size = new Size(1190, 110);
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
            tableLayoutPanel1.Location = new Point(2, 112);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 1;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Size = new Size(1190, 527);
            tableLayoutPanel1.TabIndex = 1;
            // 
            // tableLayoutPanel2
            // 
            tableLayoutPanel2.ColumnCount = 1;
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel2.Controls.Add(tLP_img, 0, 0);
            tableLayoutPanel2.Controls.Add(sfDataGrid_Cart, 0, 1);
            tableLayoutPanel2.Dock = DockStyle.Fill;
            tableLayoutPanel2.Location = new Point(3, 3);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 2;
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel2.Size = new Size(470, 521);
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
            tLP_img.Location = new Point(3, 3);
            tLP_img.Name = "tLP_img";
            tLP_img.Padding = new Padding(3);
            tLP_img.RowCount = 1;
            tLP_img.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tLP_img.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tLP_img.Size = new Size(464, 254);
            tLP_img.TabIndex = 0;
            // 
            // imgCardUrl
            // 
            imgCardUrl.BorderStyle = BorderStyle.FixedSingle;
            imgCardUrl.Dock = DockStyle.Fill;
            imgCardUrl.Location = new Point(6, 6);
            imgCardUrl.Name = "imgCardUrl";
            imgCardUrl.Size = new Size(223, 242);
            imgCardUrl.SizeMode = PictureBoxSizeMode.StretchImage;
            imgCardUrl.TabIndex = 0;
            imgCardUrl.TabStop = false;
            imgCardUrl.Click += imgCardUrl_Click;
            // 
            // panel1
            // 
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
            panel1.Location = new Point(235, 6);
            panel1.Name = "panel1";
            panel1.Size = new Size(223, 242);
            panel1.TabIndex = 1;
            // 
            // lblAmt
            // 
            lblAmt.AutoSize = true;
            lblAmt.Location = new Point(118, 55);
            lblAmt.Name = "lblAmt";
            lblAmt.Size = new Size(54, 15);
            lblAmt.TabIndex = 8;
            lblAmt.Text = "Amount:";
            // 
            // lblAmtInStock
            // 
            lblAmtInStock.AutoSize = true;
            lblAmtInStock.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblAmtInStock.Location = new Point(116, 7);
            lblAmtInStock.Name = "lblAmtInStock";
            lblAmtInStock.Size = new Size(0, 21);
            lblAmtInStock.TabIndex = 7;
            // 
            // btnFinalizeSale
            // 
            btnFinalizeSale.Location = new Point(117, 195);
            btnFinalizeSale.Name = "btnFinalizeSale";
            btnFinalizeSale.Size = new Size(86, 23);
            btnFinalizeSale.TabIndex = 6;
            btnFinalizeSale.Text = "Finalize Sale";
            btnFinalizeSale.UseVisualStyleBackColor = true;
            btnFinalizeSale.Click += btnFinalizeSale_Click;
            // 
            // lblSaleInfo
            // 
            lblSaleInfo.AutoSize = true;
            lblSaleInfo.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblSaleInfo.Location = new Point(12, 197);
            lblSaleInfo.Name = "lblSaleInfo";
            lblSaleInfo.Size = new Size(61, 17);
            lblSaleInfo.TabIndex = 5;
            lblSaleInfo.Text = "Sale Info:";
            // 
            // btnAddCt
            // 
            btnAddCt.Location = new Point(12, 114);
            btnAddCt.Name = "btnAddCt";
            btnAddCt.Size = new Size(86, 23);
            btnAddCt.TabIndex = 4;
            btnAddCt.Text = "Add to Cart";
            btnAddCt.UseVisualStyleBackColor = true;
            btnAddCt.Click += btnAddCt_Click;
            // 
            // tbAmtTraded
            // 
            tbAmtTraded.AccessibilityEnabled = true;
            tbAmtTraded.BeforeTouchSize = new Size(29, 23);
            tbAmtTraded.IntegerValue = 1L;
            tbAmtTraded.Location = new Point(132, 73);
            tbAmtTraded.Name = "tbAmtTraded";
            tbAmtTraded.Size = new Size(29, 23);
            tbAmtTraded.TabIndex = 3;
            tbAmtTraded.Text = "1";
            // 
            // tbPrice
            // 
            tbPrice.AccessibilityEnabled = true;
            tbPrice.BeforeTouchSize = new Size(29, 23);
            tbPrice.DecimalValue = new decimal(new int[] { 100, 0, 0, 131072 });
            tbPrice.Location = new Point(12, 73);
            tbPrice.Name = "tbPrice";
            tbPrice.Size = new Size(100, 23);
            tbPrice.TabIndex = 2;
            tbPrice.Text = "$1.00";
            // 
            // lblMktPrice
            // 
            lblMktPrice.AutoSize = true;
            lblMktPrice.Location = new Point(12, 55);
            lblMktPrice.Name = "lblMktPrice";
            lblMktPrice.Size = new Size(76, 15);
            lblMktPrice.TabIndex = 1;
            lblMktPrice.Text = "Market Price:";
            // 
            // lblInStock
            // 
            lblInStock.AutoSize = true;
            lblInStock.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblInStock.Location = new Point(5, 9);
            lblInStock.Name = "lblInStock";
            lblInStock.Size = new Size(105, 17);
            lblInStock.TabIndex = 0;
            lblInStock.Text = "Amount In Stock:";
            // 
            // tableLayoutPanel3
            // 
            tableLayoutPanel3.ColumnCount = 1;
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel3.Controls.Add(flowLayoutPanel1, 0, 0);
            tableLayoutPanel3.Controls.Add(sfDataGrid_InvLookup, 0, 1);
            tableLayoutPanel3.Dock = DockStyle.Fill;
            tableLayoutPanel3.Location = new Point(479, 3);
            tableLayoutPanel3.Name = "tableLayoutPanel3";
            tableLayoutPanel3.RowCount = 2;
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Percent, 80F));
            tableLayoutPanel3.Size = new Size(708, 521);
            tableLayoutPanel3.TabIndex = 1;
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.Controls.Add(cbCardGame);
            flowLayoutPanel1.Controls.Add(tbSearchBar);
            flowLayoutPanel1.Controls.Add(btnSearch);
            flowLayoutPanel1.Dock = DockStyle.Fill;
            flowLayoutPanel1.Location = new Point(3, 3);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(702, 98);
            flowLayoutPanel1.TabIndex = 0;
            // 
            // cbCardGame
            // 
            cbCardGame.FormattingEnabled = true;
            cbCardGame.Items.AddRange(new object[] { "Yugioh", "Magic", "Pokemon" });
            cbCardGame.Location = new Point(3, 3);
            cbCardGame.Name = "cbCardGame";
            cbCardGame.Size = new Size(156, 23);
            cbCardGame.TabIndex = 0;
            // 
            // tbSearchBar
            // 
            tbSearchBar.Dock = DockStyle.Bottom;
            tbSearchBar.Location = new Point(3, 32);
            tbSearchBar.Name = "tbSearchBar";
            tbSearchBar.Size = new Size(609, 23);
            tbSearchBar.TabIndex = 1;
            tbSearchBar.TextChanged += tbSearchBar_TextChanged;
            tbSearchBar.KeyDown += tbSearchBar_KeyDown;
            // 
            // btnSearch
            // 
            btnSearch.Location = new Point(618, 32);
            btnSearch.Name = "btnSearch";
            btnSearch.Size = new Size(75, 23);
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
            sfDataGrid_InvLookup.Location = new Point(3, 107);
            sfDataGrid_InvLookup.Name = "sfDataGrid_InvLookup";
            sfDataGrid_InvLookup.Size = new Size(702, 411);
            sfDataGrid_InvLookup.Style.BorderColor = Color.FromArgb(100, 100, 100);
            sfDataGrid_InvLookup.Style.CheckBoxStyle.CheckedBackColor = Color.FromArgb(0, 120, 215);
            sfDataGrid_InvLookup.Style.CheckBoxStyle.CheckedBorderColor = Color.FromArgb(0, 120, 215);
            sfDataGrid_InvLookup.Style.CheckBoxStyle.IndeterminateBorderColor = Color.FromArgb(0, 120, 215);
            sfDataGrid_InvLookup.Style.DragPreviewRowStyle.Font = new Font("Segoe UI", 9F);
            sfDataGrid_InvLookup.Style.DragPreviewRowStyle.RowCountIndicatorBackColor = Color.FromArgb(0, 120, 215);
            sfDataGrid_InvLookup.Style.DragPreviewRowStyle.RowCountIndicatorTextColor = Color.FromArgb(255, 255, 255);
            sfDataGrid_InvLookup.Style.HyperlinkStyle.DefaultLinkColor = Color.FromArgb(0, 120, 215);
            sfDataGrid_InvLookup.TabIndex = 1;
            sfDataGrid_InvLookup.Text = "sfDataGrid_InvLookup";
            sfDataGrid_InvLookup.SelectionChanged += sfDataGrid_InvLookup_SelectionChanged;
            sfDataGrid_InvLookup.CellClick += sfDataGrid_InvLookup_CellClick;
            sfDataGrid_InvLookup.Click += sfDataGrid_InvLookup_Click;
            // 
            // sfDataGrid_Cart
            // 
            sfDataGrid_Cart.AccessibleName = "Table";
            sfDataGrid_Cart.Dock = DockStyle.Fill;
            sfDataGrid_Cart.Location = new Point(3, 263);
            sfDataGrid_Cart.Name = "sfDataGrid_Cart";
            sfDataGrid_Cart.Size = new Size(464, 255);
            sfDataGrid_Cart.Style.BorderColor = Color.FromArgb(100, 100, 100);
            sfDataGrid_Cart.Style.CheckBoxStyle.CheckedBackColor = Color.FromArgb(0, 120, 215);
            sfDataGrid_Cart.Style.CheckBoxStyle.CheckedBorderColor = Color.FromArgb(0, 120, 215);
            sfDataGrid_Cart.Style.CheckBoxStyle.IndeterminateBorderColor = Color.FromArgb(0, 120, 215);
            sfDataGrid_Cart.Style.DragPreviewRowStyle.Font = new Font("Segoe UI", 9F);
            sfDataGrid_Cart.Style.DragPreviewRowStyle.RowCountIndicatorBackColor = Color.FromArgb(0, 120, 215);
            sfDataGrid_Cart.Style.DragPreviewRowStyle.RowCountIndicatorTextColor = Color.FromArgb(255, 255, 255);
            sfDataGrid_Cart.Style.HyperlinkStyle.DefaultLinkColor = Color.FromArgb(0, 120, 215);
            sfDataGrid_Cart.TabIndex = 1;
            sfDataGrid_Cart.Text = "sfDataGrid1";
            // 
            // BuySell
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1194, 641);
            Controls.Add(tableLayoutPanel1);
            Controls.Add(headerControl1);
            MinimumSize = new Size(1210, 680);
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
            tableLayoutPanel3.ResumeLayout(false);
            flowLayoutPanel1.ResumeLayout(false);
            flowLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)sfDataGrid_InvLookup).EndInit();
            ((System.ComponentModel.ISupportInitialize)sfDataGrid_Cart).EndInit();
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
        private ComboBox cbCardGame;
        private TextBox tbSearchBar;
        private Button btnSearch;
        private Label lblAmtInStock;
        private Label lblAmt;
        private Syncfusion.WinForms.DataGrid.SfDataGrid sfDataGrid_Cart;
    }
}