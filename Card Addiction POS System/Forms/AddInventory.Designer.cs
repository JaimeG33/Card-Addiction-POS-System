namespace Card_Addiction_POS_System.Forms
{
    partial class AddInventory
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
            headerControl1 = new Card_Addiction_POS_System.LogIn.Controls.HeaderControl();
            tableLayoutPanel1 = new TableLayoutPanel();
            panel2 = new Panel();
            sfDataGrid_NewInv = new Syncfusion.WinForms.DataGrid.SfDataGrid();
            tableLayoutPanel2 = new TableLayoutPanel();
            btnPrev = new Button();
            btnNext = new Button();
            panel1 = new Panel();
            lblBatchNumber = new Label();
            lblSetName = new Label();
            tableLayoutPanel3 = new TableLayoutPanel();
            sfDataGrid_NewSets = new Syncfusion.WinForms.DataGrid.SfDataGrid();
            panel3 = new Panel();
            lblMaxBatchSize = new Label();
            label2 = new Label();
            button2 = new Button();
            btnScanItems = new Button();
            btnFetchSets = new Button();
            integerTextBox_MBS = new Syncfusion.Windows.Forms.Tools.IntegerTextBox();
            label1 = new Label();
            selectCardGameControl1 = new Card_Addiction_POS_System.Forms.Controls.SelectCardGameControl();
            tableLayoutPanel1.SuspendLayout();
            panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)sfDataGrid_NewInv).BeginInit();
            tableLayoutPanel2.SuspendLayout();
            panel1.SuspendLayout();
            tableLayoutPanel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)sfDataGrid_NewSets).BeginInit();
            panel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)integerTextBox_MBS).BeginInit();
            SuspendLayout();
            // 
            // headerControl1
            // 
            headerControl1.BackColor = SystemColors.ActiveBorder;
            headerControl1.Dock = DockStyle.Top;
            headerControl1.Location = new Point(2, 2);
            headerControl1.Name = "headerControl1";
            headerControl1.Size = new Size(1190, 113);
            headerControl1.TabIndex = 0;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(panel2, 0, 1);
            tableLayoutPanel1.Controls.Add(tableLayoutPanel3, 0, 0);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(2, 115);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 1;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 30F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 70F));
            tableLayoutPanel1.Size = new Size(1190, 524);
            tableLayoutPanel1.TabIndex = 1;
            // 
            // panel2
            // 
            panel2.BorderStyle = BorderStyle.FixedSingle;
            panel2.Controls.Add(sfDataGrid_NewInv);
            panel2.Controls.Add(tableLayoutPanel2);
            panel2.Dock = DockStyle.Fill;
            panel2.Location = new Point(3, 160);
            panel2.Name = "panel2";
            panel2.Size = new Size(1184, 361);
            panel2.TabIndex = 1;
            // 
            // sfDataGrid_NewInv
            // 
            sfDataGrid_NewInv.AccessibleName = "Table";
            sfDataGrid_NewInv.BackColor = Color.PeachPuff;
            sfDataGrid_NewInv.Dock = DockStyle.Fill;
            sfDataGrid_NewInv.Location = new Point(0, 60);
            sfDataGrid_NewInv.Name = "sfDataGrid_NewInv";
            sfDataGrid_NewInv.Size = new Size(1182, 299);
            sfDataGrid_NewInv.Style.BorderColor = Color.FromArgb(100, 100, 100);
            sfDataGrid_NewInv.Style.CheckBoxStyle.CheckedBackColor = Color.FromArgb(0, 120, 215);
            sfDataGrid_NewInv.Style.CheckBoxStyle.CheckedBorderColor = Color.FromArgb(0, 120, 215);
            sfDataGrid_NewInv.Style.CheckBoxStyle.IndeterminateBorderColor = Color.FromArgb(0, 120, 215);
            sfDataGrid_NewInv.Style.DragPreviewRowStyle.Font = new Font("Segoe UI", 9F);
            sfDataGrid_NewInv.Style.DragPreviewRowStyle.RowCountIndicatorBackColor = Color.FromArgb(0, 120, 215);
            sfDataGrid_NewInv.Style.DragPreviewRowStyle.RowCountIndicatorTextColor = Color.FromArgb(255, 255, 255);
            sfDataGrid_NewInv.Style.HyperlinkStyle.DefaultLinkColor = Color.FromArgb(0, 120, 215);
            sfDataGrid_NewInv.TabIndex = 1;
            sfDataGrid_NewInv.Text = "sfDataGrid_NewInv";
            sfDataGrid_NewInv.SelectionChanged += sfDataGrid_NewInv_SelectionChanged;
            sfDataGrid_NewInv.CurrentCellEndEdit += sfDataGrid_NewInv_CurrentCellEndEdit;
            // 
            // tableLayoutPanel2
            // 
            tableLayoutPanel2.BackColor = Color.PeachPuff;
            tableLayoutPanel2.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
            tableLayoutPanel2.ColumnCount = 3;
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10F));
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 80F));
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10F));
            tableLayoutPanel2.Controls.Add(btnPrev, 0, 0);
            tableLayoutPanel2.Controls.Add(btnNext, 2, 0);
            tableLayoutPanel2.Controls.Add(panel1, 1, 0);
            tableLayoutPanel2.Dock = DockStyle.Top;
            tableLayoutPanel2.Location = new Point(0, 0);
            tableLayoutPanel2.MaximumSize = new Size(0, 64);
            tableLayoutPanel2.MinimumSize = new Size(0, 60);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 1;
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel2.Size = new Size(1182, 60);
            tableLayoutPanel2.TabIndex = 0;
            // 
            // btnPrev
            // 
            btnPrev.Dock = DockStyle.Fill;
            btnPrev.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnPrev.Location = new Point(4, 4);
            btnPrev.Name = "btnPrev";
            btnPrev.Size = new Size(111, 52);
            btnPrev.TabIndex = 1;
            btnPrev.Text = "Prev\r\n<<< ";
            btnPrev.UseVisualStyleBackColor = true;
            btnPrev.Click += btnPrev_Click;
            // 
            // btnNext
            // 
            btnNext.Dock = DockStyle.Fill;
            btnNext.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnNext.Location = new Point(1065, 4);
            btnNext.Name = "btnNext";
            btnNext.Size = new Size(113, 52);
            btnNext.TabIndex = 0;
            btnNext.Text = "Next \r\n>>>";
            btnNext.UseVisualStyleBackColor = true;
            btnNext.Click += btnNext_Click;
            // 
            // panel1
            // 
            panel1.Controls.Add(lblBatchNumber);
            panel1.Controls.Add(lblSetName);
            panel1.Dock = DockStyle.Fill;
            panel1.Location = new Point(122, 4);
            panel1.Name = "panel1";
            panel1.Size = new Size(936, 52);
            panel1.TabIndex = 2;
            // 
            // lblBatchNumber
            // 
            lblBatchNumber.Anchor = AnchorStyles.Top;
            lblBatchNumber.AutoSize = true;
            lblBatchNumber.Font = new Font("Segoe UI", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblBatchNumber.Location = new Point(441, 32);
            lblBatchNumber.Name = "lblBatchNumber";
            lblBatchNumber.Size = new Size(74, 13);
            lblBatchNumber.TabIndex = 1;
            lblBatchNumber.Text = "Entry X of ___";
            // 
            // lblSetName
            // 
            lblSetName.Anchor = AnchorStyles.Top;
            lblSetName.AutoSize = true;
            lblSetName.Font = new Font("Segoe UI", 15.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblSetName.Location = new Point(424, 1);
            lblSetName.Name = "lblSetName";
            lblSetName.Size = new Size(108, 30);
            lblSetName.TabIndex = 0;
            lblSetName.Text = "Set Name";
            lblSetName.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // tableLayoutPanel3
            // 
            tableLayoutPanel3.ColumnCount = 2;
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F));
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65F));
            tableLayoutPanel3.Controls.Add(sfDataGrid_NewSets, 0, 0);
            tableLayoutPanel3.Controls.Add(panel3, 1, 0);
            tableLayoutPanel3.Dock = DockStyle.Fill;
            tableLayoutPanel3.Location = new Point(3, 3);
            tableLayoutPanel3.Name = "tableLayoutPanel3";
            tableLayoutPanel3.RowCount = 1;
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel3.Size = new Size(1184, 151);
            tableLayoutPanel3.TabIndex = 2;
            // 
            // sfDataGrid_NewSets
            // 
            sfDataGrid_NewSets.AccessibleName = "Table";
            sfDataGrid_NewSets.BackColor = Color.LightCyan;
            sfDataGrid_NewSets.Dock = DockStyle.Fill;
            sfDataGrid_NewSets.Location = new Point(3, 3);
            sfDataGrid_NewSets.Name = "sfDataGrid_NewSets";
            sfDataGrid_NewSets.Size = new Size(408, 145);
            sfDataGrid_NewSets.Style.BorderColor = Color.FromArgb(100, 100, 100);
            sfDataGrid_NewSets.Style.CheckBoxStyle.CheckedBackColor = Color.FromArgb(0, 120, 215);
            sfDataGrid_NewSets.Style.CheckBoxStyle.CheckedBorderColor = Color.FromArgb(0, 120, 215);
            sfDataGrid_NewSets.Style.CheckBoxStyle.IndeterminateBorderColor = Color.FromArgb(0, 120, 215);
            sfDataGrid_NewSets.Style.DragPreviewRowStyle.Font = new Font("Segoe UI", 9F);
            sfDataGrid_NewSets.Style.DragPreviewRowStyle.RowCountIndicatorBackColor = Color.FromArgb(0, 120, 215);
            sfDataGrid_NewSets.Style.DragPreviewRowStyle.RowCountIndicatorTextColor = Color.FromArgb(255, 255, 255);
            sfDataGrid_NewSets.Style.HyperlinkStyle.DefaultLinkColor = Color.FromArgb(0, 120, 215);
            sfDataGrid_NewSets.TabIndex = 2;
            sfDataGrid_NewSets.Text = "sfDataGrid1";
            sfDataGrid_NewSets.CellClick += sfDataGrid_NewSets_CellClick_1;
            sfDataGrid_NewSets.CellCheckBoxClick += sfDataGrid_NewSets_CellCheckBoxClick;
            // 
            // panel3
            // 
            panel3.Controls.Add(lblMaxBatchSize);
            panel3.Controls.Add(label2);
            panel3.Controls.Add(button2);
            panel3.Controls.Add(btnScanItems);
            panel3.Controls.Add(btnFetchSets);
            panel3.Controls.Add(integerTextBox_MBS);
            panel3.Controls.Add(label1);
            panel3.Controls.Add(selectCardGameControl1);
            panel3.Dock = DockStyle.Fill;
            panel3.Location = new Point(417, 3);
            panel3.Name = "panel3";
            panel3.Size = new Size(764, 145);
            panel3.TabIndex = 3;
            // 
            // lblMaxBatchSize
            // 
            lblMaxBatchSize.AutoSize = true;
            lblMaxBatchSize.Location = new Point(261, 6);
            lblMaxBatchSize.Name = "lblMaxBatchSize";
            lblMaxBatchSize.Size = new Size(22, 15);
            lblMaxBatchSize.TabIndex = 9;
            lblMaxBatchSize.Text = "___";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(167, 6);
            label2.Name = "label2";
            label2.Size = new Size(88, 15);
            label2.TabIndex = 8;
            label2.Text = "Max Batch Size:";
            // 
            // button2
            // 
            button2.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            button2.Location = new Point(276, 48);
            button2.Name = "button2";
            button2.Size = new Size(123, 35);
            button2.TabIndex = 7;
            button2.Text = "Fetch Sets";
            button2.UseVisualStyleBackColor = true;
            // 
            // btnScanItems
            // 
            btnScanItems.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnScanItems.Location = new Point(147, 48);
            btnScanItems.Name = "btnScanItems";
            btnScanItems.Size = new Size(123, 35);
            btnScanItems.TabIndex = 6;
            btnScanItems.Text = "Scan Items";
            btnScanItems.UseVisualStyleBackColor = true;
            btnScanItems.Click += btnScanItems_Click;
            // 
            // btnFetchSets
            // 
            btnFetchSets.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnFetchSets.Location = new Point(12, 48);
            btnFetchSets.Name = "btnFetchSets";
            btnFetchSets.Size = new Size(123, 35);
            btnFetchSets.TabIndex = 5;
            btnFetchSets.Text = "Fetch Sets";
            btnFetchSets.UseVisualStyleBackColor = true;
            btnFetchSets.Click += btnFetchSets_Click;
            // 
            // integerTextBox_MBS
            // 
            integerTextBox_MBS.AccessibilityEnabled = true;
            integerTextBox_MBS.BeforeTouchSize = new Size(29, 23);
            integerTextBox_MBS.ForeColor = SystemColors.WindowText;
            integerTextBox_MBS.IntegerValue = 0L;
            integerTextBox_MBS.Location = new Point(306, 21);
            integerTextBox_MBS.Name = "integerTextBox_MBS";
            integerTextBox_MBS.Size = new Size(32, 23);
            integerTextBox_MBS.TabIndex = 4;
            integerTextBox_MBS.TextChanged += integerTextBox_MBS_TextChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label1.Location = new Point(166, 21);
            label1.Name = "label1";
            label1.Size = new Size(140, 21);
            label1.TabIndex = 3;
            label1.Text = "Current Batch Size:";
            // 
            // selectCardGameControl1
            // 
            selectCardGameControl1.Location = new Point(3, 3);
            selectCardGameControl1.Name = "selectCardGameControl1";
            selectCardGameControl1.SelectedCardGameId = -1;
            selectCardGameControl1.Size = new Size(158, 24);
            selectCardGameControl1.TabIndex = 2;
            // 
            // AddInventory
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1194, 641);
            Controls.Add(tableLayoutPanel1);
            Controls.Add(headerControl1);
            MinimumSize = new Size(1210, 680);
            Name = "AddInventory";
            Style.MdiChild.IconHorizontalAlignment = HorizontalAlignment.Center;
            Style.MdiChild.IconVerticalAlignment = System.Windows.Forms.VisualStyles.VerticalAlignment.Center;
            Text = "AddInventory";
            WindowState = FormWindowState.Maximized;
            FormClosed += AddInventory_FormClosed;
            Load += AddInventory_Load;
            tableLayoutPanel1.ResumeLayout(false);
            panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)sfDataGrid_NewInv).EndInit();
            tableLayoutPanel2.ResumeLayout(false);
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            tableLayoutPanel3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)sfDataGrid_NewSets).EndInit();
            panel3.ResumeLayout(false);
            panel3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)integerTextBox_MBS).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private LogIn.Controls.HeaderControl headerControl1;
        private TableLayoutPanel tableLayoutPanel1;
        private TableLayoutPanel tableLayoutPanel2;
        private Button btnPrev;
        private Button btnNext;
        private Panel panel1;
        private Label lblSetName;
        private Label lblBatchNumber;
        private Panel panel2;
        private Syncfusion.WinForms.DataGrid.SfDataGrid sfDataGrid_NewInv;
        private Controls.SelectCardGameControl selectCardGameControl1;
        private TableLayoutPanel tableLayoutPanel3;
        private Syncfusion.WinForms.DataGrid.SfDataGrid sfDataGrid_NewSets;
        private Panel panel3;
        private Label label1;
        private Syncfusion.Windows.Forms.Tools.IntegerTextBox integerTextBox_MBS;
        private Button btnFetchSets;
        private Button btnScanItems;
        private Button button2;
        private Label lblMaxBatchSize;
        private Label label2;
    }
}