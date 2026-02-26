namespace Card_Addiction_POS_System.Forms.Controls
{
    partial class InventoryItemPopup
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            tableLayoutPanel1 = new TableLayoutPanel();
            btnBack = new Button();
            pnlInfo = new Panel();
            pnlButtons = new Panel();
            label1 = new Label();
            button2 = new Button();
            label2 = new Label();
            lblUp2Date = new Label();
            panel1 = new Panel();
            btnEnter = new Button();
            label3 = new Label();
            currencyTextBox_MktPrice = new Syncfusion.Windows.Forms.Tools.CurrencyTextBox();
            integerTextBox1 = new Syncfusion.Windows.Forms.Tools.IntegerTextBox();
            cbAISenabled = new CheckBox();
            cmbxIssues = new ComboBox();
            tableLayoutPanel1.SuspendLayout();
            pnlInfo.SuspendLayout();
            pnlButtons.SuspendLayout();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)currencyTextBox_MktPrice).BeginInit();
            ((System.ComponentModel.ISupportInitialize)integerTextBox1).BeginInit();
            SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(pnlButtons, 0, 2);
            tableLayoutPanel1.Controls.Add(pnlInfo, 0, 1);
            tableLayoutPanel1.Controls.Add(panel1, 0, 0);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 3;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 45F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 35F));
            tableLayoutPanel1.Size = new Size(290, 191);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // btnBack
            // 
            btnBack.BackColor = Color.Transparent;
            btnBack.Location = new Point(0, 0);
            btnBack.Name = "btnBack";
            btnBack.Size = new Size(89, 28);
            btnBack.TabIndex = 0;
            btnBack.Text = "<----   Back";
            btnBack.UseVisualStyleBackColor = false;
            // 
            // pnlInfo
            // 
            pnlInfo.Controls.Add(cmbxIssues);
            pnlInfo.Controls.Add(label1);
            pnlInfo.Dock = DockStyle.Fill;
            pnlInfo.Location = new Point(4, 42);
            pnlInfo.Name = "pnlInfo";
            pnlInfo.Size = new Size(282, 78);
            pnlInfo.TabIndex = 1;
            // 
            // pnlButtons
            // 
            pnlButtons.Controls.Add(cbAISenabled);
            pnlButtons.Controls.Add(integerTextBox1);
            pnlButtons.Controls.Add(currencyTextBox_MktPrice);
            pnlButtons.Controls.Add(label3);
            pnlButtons.Controls.Add(button2);
            pnlButtons.Controls.Add(lblUp2Date);
            pnlButtons.Controls.Add(label2);
            pnlButtons.Dock = DockStyle.Fill;
            pnlButtons.Location = new Point(4, 127);
            pnlButtons.Name = "pnlButtons";
            pnlButtons.Size = new Size(282, 60);
            pnlButtons.TabIndex = 2;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(5, 4);
            label1.Name = "label1";
            label1.Size = new Size(71, 15);
            label1.TabIndex = 0;
            label1.Text = "Report Issue";
            // 
            // button2
            // 
            button2.Font = new Font("Segoe UI", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            button2.Location = new Point(66, 3);
            button2.Name = "button2";
            button2.Size = new Size(54, 48);
            button2.TabIndex = 0;
            button2.Text = "Refresh Mkt Price";
            button2.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(4, 3);
            label2.Name = "label2";
            label2.Size = new Size(60, 15);
            label2.TabIndex = 1;
            label2.Text = "Mkt Price:";
            // 
            // lblUp2Date
            // 
            lblUp2Date.AutoSize = true;
            lblUp2Date.Font = new Font("Segoe UI", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblUp2Date.ForeColor = SystemColors.ControlDarkDark;
            lblUp2Date.Location = new Point(1, 44);
            lblUp2Date.Name = "lblUp2Date";
            lblUp2Date.Size = new Size(68, 13);
            lblUp2Date.TabIndex = 3;
            lblUp2Date.Text = "Up to Date?";
            // 
            // panel1
            // 
            panel1.Controls.Add(btnEnter);
            panel1.Controls.Add(btnBack);
            panel1.Dock = DockStyle.Fill;
            panel1.Location = new Point(4, 4);
            panel1.Name = "panel1";
            panel1.Size = new Size(282, 31);
            panel1.TabIndex = 3;
            // 
            // btnEnter
            // 
            btnEnter.BackColor = Color.DarkSeaGreen;
            btnEnter.Location = new Point(179, -1);
            btnEnter.Name = "btnEnter";
            btnEnter.Size = new Size(89, 28);
            btnEnter.TabIndex = 1;
            btnEnter.Text = "Proceed";
            btnEnter.UseVisualStyleBackColor = false;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.BackColor = Color.Transparent;
            label3.Location = new Point(178, 1);
            label3.Name = "label3";
            label3.Size = new Size(52, 30);
            label3.TabIndex = 4;
            label3.Text = "Set Amt\r\nIn Stock:";
            // 
            // currencyTextBox_MktPrice
            // 
            currencyTextBox_MktPrice.AccessibilityEnabled = true;
            currencyTextBox_MktPrice.BeforeTouchSize = new Size(51, 23);
            currencyTextBox_MktPrice.DecimalValue = new decimal(new int[] { 100, 0, 0, 131072 });
            currencyTextBox_MktPrice.Location = new Point(6, 20);
            currencyTextBox_MktPrice.Name = "currencyTextBox_MktPrice";
            currencyTextBox_MktPrice.Size = new Size(58, 23);
            currencyTextBox_MktPrice.TabIndex = 1;
            currencyTextBox_MktPrice.Text = "$1.00";
            // 
            // integerTextBox1
            // 
            integerTextBox1.AccessibilityEnabled = true;
            integerTextBox1.BeforeTouchSize = new Size(51, 23);
            integerTextBox1.IntegerValue = 1L;
            integerTextBox1.Location = new Point(228, 6);
            integerTextBox1.Name = "integerTextBox1";
            integerTextBox1.Size = new Size(51, 23);
            integerTextBox1.TabIndex = 5;
            integerTextBox1.Text = "1";
            // 
            // cbAISenabled
            // 
            cbAISenabled.AutoSize = true;
            cbAISenabled.Location = new Point(198, 34);
            cbAISenabled.Name = "cbAISenabled";
            cbAISenabled.Size = new Size(68, 19);
            cbAISenabled.TabIndex = 7;
            cbAISenabled.Text = "Enabled";
            cbAISenabled.UseVisualStyleBackColor = true;
            // 
            // cmbxIssues
            // 
            cmbxIssues.FormattingEnabled = true;
            cmbxIssues.Items.AddRange(new object[] { "(None Selected)", "AmtInStock Not Accurate", "Link Not Working", "Issue With Column", "Innacurate Info", "Other (Please Specify)" });
            cmbxIssues.Location = new Point(6, 22);
            cmbxIssues.Name = "cmbxIssues";
            cmbxIssues.Size = new Size(133, 23);
            cmbxIssues.TabIndex = 1;
            cmbxIssues.Text = "(None Selected)";
            // 
            // InventoryItemPopup
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.Info;
            Controls.Add(tableLayoutPanel1);
            Name = "InventoryItemPopup";
            Size = new Size(290, 191);
            tableLayoutPanel1.ResumeLayout(false);
            pnlInfo.ResumeLayout(false);
            pnlInfo.PerformLayout();
            pnlButtons.ResumeLayout(false);
            pnlButtons.PerformLayout();
            panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)currencyTextBox_MktPrice).EndInit();
            ((System.ComponentModel.ISupportInitialize)integerTextBox1).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel tableLayoutPanel1;
        private Button btnBack;
        private Panel pnlButtons;
        private Panel pnlInfo;
        private Label label1;
        private Label lblUp2Date;
        private Label label2;
        private Button button2;
        private Label label3;
        private Panel panel1;
        private Button btnEnter;
        private Syncfusion.Windows.Forms.Tools.IntegerTextBox integerTextBox1;
        private Syncfusion.Windows.Forms.Tools.CurrencyTextBox currencyTextBox_MktPrice;
        private CheckBox cbAISenabled;
        private ComboBox cmbxIssues;
    }
}
