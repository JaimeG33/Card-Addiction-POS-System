using Syncfusion.WinForms.Controls;
using System.Drawing;
using System.Windows.Forms;

namespace Card_Addiction_POS_System.Forms
{
    public partial class HomePage : SfForm
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
            tableLayoutPanel1 = new TableLayoutPanel();
            flowLayoutPanel1 = new FlowLayoutPanel();
            btnOrders = new Button();
            btnSale = new Button();
            headerControl1 = new Card_Addiction_POS_System.LogIn.Controls.HeaderControl();
            tableLayoutPanel1.SuspendLayout();
            flowLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            tableLayoutPanel1.Controls.Add(flowLayoutPanel1, 0, 0);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(2, 102);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 1;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Size = new Size(1190, 537);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.Controls.Add(btnOrders);
            flowLayoutPanel1.Controls.Add(btnSale);
            flowLayoutPanel1.Dock = DockStyle.Fill;
            flowLayoutPanel1.Location = new Point(3, 3);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(827, 531);
            flowLayoutPanel1.TabIndex = 0;
            // 
            // btnOrders
            // 
            btnOrders.Location = new Point(10, 10);
            btnOrders.Margin = new Padding(10);
            btnOrders.Name = "btnOrders";
            btnOrders.Size = new Size(158, 54);
            btnOrders.TabIndex = 0;
            btnOrders.Text = "Check Orders";
            btnOrders.UseVisualStyleBackColor = true;
            // 
            // btnSale
            // 
            btnSale.BackColor = Color.LightGray;
            btnSale.Location = new Point(188, 10);
            btnSale.Margin = new Padding(10);
            btnSale.Name = "btnSale";
            btnSale.Size = new Size(158, 54);
            btnSale.TabIndex = 1;
            btnSale.Text = "Sale";
            btnSale.UseVisualStyleBackColor = false;
            btnSale.Click += btnSale_Click;
            // 
            // headerControl1
            // 
            headerControl1.BackColor = SystemColors.ActiveBorder;
            headerControl1.Dock = DockStyle.Top;
            headerControl1.Location = new Point(2, 2);
            headerControl1.Name = "headerControl1";
            headerControl1.Size = new Size(1190, 100);
            headerControl1.TabIndex = 1;
            // 
            // HomePage
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1194, 641);
            Controls.Add(tableLayoutPanel1);
            Controls.Add(headerControl1);
            MinimumSize = new Size(1210, 680);
            Name = "HomePage";
            Style.MdiChild.IconHorizontalAlignment = HorizontalAlignment.Center;
            Style.MdiChild.IconVerticalAlignment = System.Windows.Forms.VisualStyles.VerticalAlignment.Center;
            Text = "HomePage";
            WindowState = FormWindowState.Maximized;
            FormClosed += HomePage_FormClosed;
            Load += HomePage_Load;
            tableLayoutPanel1.ResumeLayout(false);
            flowLayoutPanel1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel tableLayoutPanel1;
        private LogIn.Controls.HeaderControl headerControl1;
        private FlowLayoutPanel flowLayoutPanel1;
        private Button btnOrders;
        private Button btnSale;
    }
}