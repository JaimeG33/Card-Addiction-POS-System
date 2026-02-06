namespace Card_Addiction_POS_System.Forms
{
    partial class Admin
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
            label1 = new Label();
            label2 = new Label();
            btnNewEpin = new Button();
            integerTextBox1 = new TextBox();
            integerTextBox2 = new TextBox();
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
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(85, 137);
            label1.Name = "label1";
            label1.Size = new Size(69, 15);
            label1.TabIndex = 1;
            label1.Text = "employeeId";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(96, 175);
            label2.Name = "label2";
            label2.Size = new Size(49, 15);
            label2.TabIndex = 4;
            label2.Text = "new pin";
            // 
            // btnNewEpin
            // 
            btnNewEpin.Location = new Point(289, 137);
            btnNewEpin.Name = "btnNewEpin";
            btnNewEpin.Size = new Size(94, 61);
            btnNewEpin.TabIndex = 5;
            btnNewEpin.Text = "Set New Employee Pin";
            btnNewEpin.UseVisualStyleBackColor = true;
            btnNewEpin.Click += btnNewEpin_Click;
            // 
            // integerTextBox1
            // 
            integerTextBox1.Location = new Point(161, 134);
            integerTextBox1.Name = "integerTextBox1";
            integerTextBox1.Size = new Size(100, 23);
            integerTextBox1.TabIndex = 6;
            // 
            // integerTextBox2
            // 
            integerTextBox2.Location = new Point(161, 172);
            integerTextBox2.Name = "integerTextBox2";
            integerTextBox2.Size = new Size(100, 23);
            integerTextBox2.TabIndex = 7;
            // 
            // Admin
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1194, 641);
            Controls.Add(integerTextBox2);
            Controls.Add(integerTextBox1);
            Controls.Add(btnNewEpin);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(headerControl1);
            MinimumSize = new Size(1210, 680);
            Name = "Admin";
            Style.MdiChild.IconHorizontalAlignment = HorizontalAlignment.Center;
            Style.MdiChild.IconVerticalAlignment = System.Windows.Forms.VisualStyles.VerticalAlignment.Center;
            Text = "Admin";
            Load += Admin_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private LogIn.Controls.HeaderControl headerControl1;
        private Label label1;
        private Label label2;
        private Button btnNewEpin;
        private TextBox integerTextBox1;
        private TextBox integerTextBox2;
    }
}