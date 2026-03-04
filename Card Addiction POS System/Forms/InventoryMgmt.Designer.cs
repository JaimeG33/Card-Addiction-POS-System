namespace Card_Addiction_POS_System.Forms
{
    partial class InventoryMgmt
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
            flowLayoutPanel1 = new FlowLayoutPanel();
            selectCardGameControl1 = new Card_Addiction_POS_System.Forms.Controls.SelectCardGameControl();
            tbSearchBar = new TextBox();
            btnSearch = new Button();
            tableLayoutPanel3 = new TableLayoutPanel();
            imgCardUrl = new PictureBox();
            panel1 = new Panel();
            button1 = new Button();
            btnSetPrice = new Button();
            btnUpdateAll = new Button();
            btnUpdateAmt = new Button();
            btnFetch = new Button();
            lblAmt = new Label();
            tbAmtTraded = new Syncfusion.Windows.Forms.Tools.IntegerTextBox();
            tbPrice = new Syncfusion.Windows.Forms.Tools.CurrencyTextBox();
            lblMktPrice = new Label();
            label1 = new Label();
            sfDataGrid_InvLookup = new Syncfusion.WinForms.DataGrid.SfDataGrid();
            btnAddInventory = new Button();
            tableLayoutPanel1.SuspendLayout();
            tableLayoutPanel2.SuspendLayout();
            flowLayoutPanel1.SuspendLayout();
            tableLayoutPanel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)imgCardUrl).BeginInit();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)tbAmtTraded).BeginInit();
            ((System.ComponentModel.ISupportInitialize)tbPrice).BeginInit();
            ((System.ComponentModel.ISupportInitialize)sfDataGrid_InvLookup).BeginInit();
            SuspendLayout();
            // 
            // headerControl1
            // 
            headerControl1.BackColor = SystemColors.ActiveBorder;
            headerControl1.Dock = DockStyle.Top;
            headerControl1.Location = new Point(2, 2);
            headerControl1.Margin = new Padding(3, 5, 3, 5);
            headerControl1.Name = "headerControl1";
            headerControl1.Size = new Size(1360, 151);
            headerControl1.TabIndex = 0;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 771F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.Controls.Add(tableLayoutPanel2, 0, 0);
            tableLayoutPanel1.Controls.Add(sfDataGrid_InvLookup, 1, 0);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(2, 153);
            tableLayoutPanel1.Margin = new Padding(3, 4, 3, 4);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 1;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Size = new Size(1360, 697);
            tableLayoutPanel1.TabIndex = 1;
            // 
            // tableLayoutPanel2
            // 
            tableLayoutPanel2.ColumnCount = 1;
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel2.Controls.Add(flowLayoutPanel1, 0, 0);
            tableLayoutPanel2.Controls.Add(tableLayoutPanel3, 0, 1);
            tableLayoutPanel2.Dock = DockStyle.Fill;
            tableLayoutPanel2.Location = new Point(3, 4);
            tableLayoutPanel2.Margin = new Padding(3, 4, 3, 4);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 2;
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 35F));
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 65F));
            tableLayoutPanel2.Size = new Size(765, 689);
            tableLayoutPanel2.TabIndex = 0;
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
            flowLayoutPanel1.Size = new Size(759, 233);
            flowLayoutPanel1.TabIndex = 0;
            // 
            // selectCardGameControl1
            // 
            selectCardGameControl1.Location = new Point(3, 5);
            selectCardGameControl1.Margin = new Padding(3, 5, 3, 5);
            selectCardGameControl1.Name = "selectCardGameControl1";
            selectCardGameControl1.SelectedCardGameId = -1;
            selectCardGameControl1.Size = new Size(198, 31);
            selectCardGameControl1.TabIndex = 6;
            // 
            // tbSearchBar
            // 
            tbSearchBar.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            tbSearchBar.Location = new Point(3, 45);
            tbSearchBar.Margin = new Padding(3, 4, 3, 4);
            tbSearchBar.Name = "tbSearchBar";
            tbSearchBar.Size = new Size(708, 27);
            tbSearchBar.TabIndex = 4;
            // 
            // btnSearch
            // 
            btnSearch.Location = new Point(3, 80);
            btnSearch.Margin = new Padding(3, 4, 3, 4);
            btnSearch.Name = "btnSearch";
            btnSearch.Size = new Size(86, 31);
            btnSearch.TabIndex = 5;
            btnSearch.Text = "Search";
            btnSearch.UseVisualStyleBackColor = true;
            btnSearch.Click += btnSearch_Click_1;
            // 
            // tableLayoutPanel3
            // 
            tableLayoutPanel3.ColumnCount = 2;
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55F));
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45F));
            tableLayoutPanel3.Controls.Add(imgCardUrl, 0, 0);
            tableLayoutPanel3.Controls.Add(panel1, 1, 0);
            tableLayoutPanel3.Dock = DockStyle.Fill;
            tableLayoutPanel3.Location = new Point(3, 245);
            tableLayoutPanel3.Margin = new Padding(3, 4, 3, 4);
            tableLayoutPanel3.Name = "tableLayoutPanel3";
            tableLayoutPanel3.RowCount = 1;
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel3.Size = new Size(759, 440);
            tableLayoutPanel3.TabIndex = 1;
            // 
            // imgCardUrl
            // 
            imgCardUrl.BorderStyle = BorderStyle.FixedSingle;
            imgCardUrl.Dock = DockStyle.Fill;
            imgCardUrl.Location = new Point(3, 4);
            imgCardUrl.Margin = new Padding(3, 4, 3, 4);
            imgCardUrl.Name = "imgCardUrl";
            imgCardUrl.Size = new Size(411, 432);
            imgCardUrl.SizeMode = PictureBoxSizeMode.StretchImage;
            imgCardUrl.TabIndex = 1;
            imgCardUrl.TabStop = false;
            // 
            // panel1
            // 
            panel1.Controls.Add(button1);
            panel1.Controls.Add(btnSetPrice);
            panel1.Controls.Add(btnUpdateAll);
            panel1.Controls.Add(btnUpdateAmt);
            panel1.Controls.Add(btnFetch);
            panel1.Controls.Add(lblAmt);
            panel1.Controls.Add(tbAmtTraded);
            panel1.Controls.Add(tbPrice);
            panel1.Controls.Add(lblMktPrice);
            panel1.Controls.Add(label1);
            panel1.Dock = DockStyle.Fill;
            panel1.Location = new Point(420, 4);
            panel1.Margin = new Padding(3, 4, 3, 4);
            panel1.Name = "panel1";
            panel1.Size = new Size(336, 432);
            panel1.TabIndex = 2;
            // 
            // button1
            // 
            button1.Location = new Point(194, 174);
            button1.Margin = new Padding(3, 4, 3, 4);
            button1.Name = "button1";
            button1.Size = new Size(128, 73);
            button1.TabIndex = 15;
            button1.Text = "Item Not Counted in Inventory";
            button1.UseVisualStyleBackColor = true;
            // 
            // btnSetPrice
            // 
            btnSetPrice.Location = new Point(11, 174);
            btnSetPrice.Margin = new Padding(3, 4, 3, 4);
            btnSetPrice.Name = "btnSetPrice";
            btnSetPrice.Size = new Size(114, 31);
            btnSetPrice.TabIndex = 14;
            btnSetPrice.Text = "Set New Price";
            btnSetPrice.UseVisualStyleBackColor = true;
            btnSetPrice.Click += btnSetPrice_Click;
            // 
            // btnUpdateAll
            // 
            btnUpdateAll.Location = new Point(134, 20);
            btnUpdateAll.Margin = new Padding(3, 4, 3, 4);
            btnUpdateAll.Name = "btnUpdateAll";
            btnUpdateAll.Size = new Size(114, 31);
            btnUpdateAll.TabIndex = 13;
            btnUpdateAll.Text = "Update All";
            btnUpdateAll.UseVisualStyleBackColor = true;
            btnUpdateAll.Click += btnUpdateAll_Click;
            // 
            // btnUpdateAmt
            // 
            btnUpdateAmt.Location = new Point(194, 135);
            btnUpdateAmt.Margin = new Padding(3, 4, 3, 4);
            btnUpdateAmt.Name = "btnUpdateAmt";
            btnUpdateAmt.Size = new Size(128, 31);
            btnUpdateAmt.TabIndex = 12;
            btnUpdateAmt.Text = "Update Amount";
            btnUpdateAmt.UseVisualStyleBackColor = true;
            btnUpdateAmt.Click += btnUpdateAmt_Click;
            // 
            // btnFetch
            // 
            btnFetch.Location = new Point(11, 135);
            btnFetch.Margin = new Padding(3, 4, 3, 4);
            btnFetch.Name = "btnFetch";
            btnFetch.Size = new Size(114, 31);
            btnFetch.TabIndex = 11;
            btnFetch.Text = "Fetch Mkt $";
            btnFetch.UseVisualStyleBackColor = true;
            btnFetch.Click += btnFetch_Click;
            // 
            // lblAmt
            // 
            lblAmt.AutoSize = true;
            lblAmt.Location = new Point(194, 72);
            lblAmt.Name = "lblAmt";
            lblAmt.Size = new Size(121, 20);
            lblAmt.TabIndex = 10;
            lblAmt.Text = "Amount in Stock:";
            // 
            // tbAmtTraded
            // 
            tbAmtTraded.AccessibilityEnabled = true;
            tbAmtTraded.BeforeTouchSize = new Size(114, 27);
            tbAmtTraded.IntegerValue = 1L;
            tbAmtTraded.Location = new Point(194, 96);
            tbAmtTraded.Margin = new Padding(3, 4, 3, 4);
            tbAmtTraded.Name = "tbAmtTraded";
            tbAmtTraded.Size = new Size(127, 27);
            tbAmtTraded.TabIndex = 9;
            tbAmtTraded.Text = "1";
            // 
            // tbPrice
            // 
            tbPrice.AccessibilityEnabled = true;
            tbPrice.BeforeTouchSize = new Size(114, 27);
            tbPrice.DecimalValue = new decimal(new int[] { 100, 0, 0, 131072 });
            tbPrice.Location = new Point(11, 96);
            tbPrice.Margin = new Padding(3, 4, 3, 4);
            tbPrice.Name = "tbPrice";
            tbPrice.Size = new Size(114, 27);
            tbPrice.TabIndex = 4;
            tbPrice.Text = "$1.00";
            // 
            // lblMktPrice
            // 
            lblMktPrice.AutoSize = true;
            lblMktPrice.Location = new Point(11, 72);
            lblMktPrice.Name = "lblMktPrice";
            lblMktPrice.Size = new Size(94, 20);
            lblMktPrice.TabIndex = 3;
            lblMktPrice.Text = "Market Price:";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.Location = new Point(3, 15);
            label1.Name = "label1";
            label1.Size = new Size(107, 32);
            label1.TabIndex = 0;
            label1.Text = "Actions:";
            // 
            // sfDataGrid_InvLookup
            // 
            sfDataGrid_InvLookup.AccessibleName = "Table";
            sfDataGrid_InvLookup.Dock = DockStyle.Fill;
            sfDataGrid_InvLookup.Location = new Point(774, 4);
            sfDataGrid_InvLookup.Margin = new Padding(3, 4, 3, 4);
            sfDataGrid_InvLookup.Name = "sfDataGrid_InvLookup";
            sfDataGrid_InvLookup.PreviewRowHeight = 35;
            sfDataGrid_InvLookup.Size = new Size(667, 689);
            sfDataGrid_InvLookup.Style.BorderColor = Color.FromArgb(100, 100, 100);
            sfDataGrid_InvLookup.Style.DragPreviewRowStyle.Font = new Font("Segoe UI", 9F);
            sfDataGrid_InvLookup.Style.DragPreviewRowStyle.RowCountIndicatorTextColor = Color.FromArgb(255, 255, 255);
            sfDataGrid_InvLookup.TabIndex = 1;
            sfDataGrid_InvLookup.Text = "sfDataGrid1";
            sfDataGrid_InvLookup.SelectionChanged += sfDataGrid_InvLookup_SelectionChanged;
            sfDataGrid_InvLookup.Click += sfDataGrid_InvLookup_Click;
            // 
            // btnAddInventory
            // 
            btnAddInventory.BackColor = Color.Red;
            btnAddInventory.Location = new Point(1208, 115);
            btnAddInventory.Margin = new Padding(3, 4, 3, 4);
            btnAddInventory.Name = "btnAddInventory";
            btnAddInventory.Size = new Size(151, 31);
            btnAddInventory.TabIndex = 6;
            btnAddInventory.Text = "Admin Controls";
            btnAddInventory.UseVisualStyleBackColor = false;
            btnAddInventory.Click += btnAddInventory_Click;
            // 
            // InventoryMgmt
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1364, 852);
            Controls.Add(tableLayoutPanel1);
            Controls.Add(btnAddInventory);
            Controls.Add(headerControl1);
            Margin = new Padding(3, 4, 3, 4);
            MinimumSize = new Size(1380, 891);
            Name = "InventoryMgmt";
            Style.MdiChild.IconHorizontalAlignment = HorizontalAlignment.Center;
            Style.MdiChild.IconVerticalAlignment = System.Windows.Forms.VisualStyles.VerticalAlignment.Center;
            Text = "InventoryMgmt";
            WindowState = FormWindowState.Maximized;
            FormClosed += InventoryMgmt_FormClosed;
            Load += InventoryMgmt_Load;
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel2.ResumeLayout(false);
            flowLayoutPanel1.ResumeLayout(false);
            flowLayoutPanel1.PerformLayout();
            tableLayoutPanel3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)imgCardUrl).EndInit();
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)tbAmtTraded).EndInit();
            ((System.ComponentModel.ISupportInitialize)tbPrice).EndInit();
            ((System.ComponentModel.ISupportInitialize)sfDataGrid_InvLookup).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private LogIn.Controls.HeaderControl headerControl1;
        private TableLayoutPanel tableLayoutPanel1;
        private TableLayoutPanel tableLayoutPanel2;
        private FlowLayoutPanel flowLayoutPanel1;
        private TextBox tbSearchBar;
        private Button btnSearch;
        private TableLayoutPanel tableLayoutPanel3;
        private PictureBox imgCardUrl;
        private Syncfusion.WinForms.DataGrid.SfDataGrid sfDataGrid_InvLookup;
        private Panel panel1;
        private Label label1;
        private Syncfusion.Windows.Forms.Tools.CurrencyTextBox tbPrice;
        private Label lblMktPrice;
        private Label lblAmt;
        private Syncfusion.Windows.Forms.Tools.IntegerTextBox tbAmtTraded;
        private Button btnUpdateAmt;
        private Button btnFetch;
        private Button btnUpdateAll;
        private Button btnAddInventory;
        private Controls.SelectCardGameControl selectCardGameControl1;
        private Button button1;
        private Button btnSetPrice;
    }
}