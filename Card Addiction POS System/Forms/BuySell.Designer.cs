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
            btnFinalizeSale = new Button();
            lblSaleInfo = new Label();
            btnAddCt = new Button();
            tbAmtTraded = new Syncfusion.Windows.Forms.Tools.IntegerTextBox();
            tbPrice = new Syncfusion.Windows.Forms.Tools.CurrencyTextBox();
            lblMktPrice = new Label();
            lblInStock = new Label();
            tableLayoutPanel1.SuspendLayout();
            tableLayoutPanel2.SuspendLayout();
            tLP_img.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)imgCardUrl).BeginInit();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)tbAmtTraded).BeginInit();
            ((System.ComponentModel.ISupportInitialize)tbPrice).BeginInit();
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
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
            tableLayoutPanel1.Controls.Add(tableLayoutPanel2, 0, 0);
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
            imgCardUrl.Dock = DockStyle.Fill;
            imgCardUrl.Location = new Point(6, 6);
            imgCardUrl.Name = "imgCardUrl";
            imgCardUrl.Size = new Size(223, 242);
            imgCardUrl.TabIndex = 0;
            imgCardUrl.TabStop = false;
            // 
            // panel1
            // 
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
            // btnFinalizeSale
            // 
            btnFinalizeSale.Location = new Point(117, 195);
            btnFinalizeSale.Name = "btnFinalizeSale";
            btnFinalizeSale.Size = new Size(86, 23);
            btnFinalizeSale.TabIndex = 6;
            btnFinalizeSale.Text = "Finalize Sale";
            btnFinalizeSale.UseVisualStyleBackColor = true;
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
            lblInStock.Location = new Point(12, 9);
            lblInStock.Name = "lblInStock";
            lblInStock.Size = new Size(105, 17);
            lblInStock.TabIndex = 0;
            lblInStock.Text = "Amount In Stock:";
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
            Load += BuySell_Load;
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel2.ResumeLayout(false);
            tLP_img.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)imgCardUrl).EndInit();
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)tbAmtTraded).EndInit();
            ((System.ComponentModel.ISupportInitialize)tbPrice).EndInit();
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
    }
}