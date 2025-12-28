namespace Card_Addiction_POS_System
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            btnLogin = new Button();
            tbUser = new TextBox();
            tbPswd = new TextBox();
            lblUser = new Label();
            label1 = new Label();
            cbShowPswd = new CheckBox();
            btnIP = new Button();
            label2 = new Label();
            SuspendLayout();
            // 
            // btnLogin
            // 
            btnLogin.Location = new Point(529, 117);
            btnLogin.Name = "btnLogin";
            btnLogin.Size = new Size(106, 71);
            btnLogin.TabIndex = 0;
            btnLogin.Text = "Login";
            btnLogin.UseVisualStyleBackColor = true;
            btnLogin.Click += btnLogin_Click;
            // 
            // tbUser
            // 
            tbUser.Location = new Point(226, 126);
            tbUser.Name = "tbUser";
            tbUser.Size = new Size(289, 23);
            tbUser.TabIndex = 1;
            // 
            // tbPswd
            // 
            tbPswd.Location = new Point(226, 155);
            tbPswd.Name = "tbPswd";
            tbPswd.Size = new Size(289, 23);
            tbPswd.TabIndex = 2;
            tbPswd.KeyDown += tbPswd_KeyDown;
            // 
            // lblUser
            // 
            lblUser.AutoSize = true;
            lblUser.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblUser.Location = new Point(139, 124);
            lblUser.Name = "lblUser";
            lblUser.Size = new Size(81, 21);
            lblUser.TabIndex = 3;
            lblUser.Text = "Username";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label1.Location = new Point(139, 157);
            label1.Name = "label1";
            label1.Size = new Size(76, 21);
            label1.TabIndex = 4;
            label1.Text = "Password";
            // 
            // cbShowPswd
            // 
            cbShowPswd.AutoSize = true;
            cbShowPswd.Location = new Point(226, 184);
            cbShowPswd.Name = "cbShowPswd";
            cbShowPswd.Size = new Size(108, 19);
            cbShowPswd.TabIndex = 5;
            cbShowPswd.Text = "Show Password";
            cbShowPswd.UseVisualStyleBackColor = true;
            cbShowPswd.CheckedChanged += cbShowPswd_CheckedChanged;
            // 
            // btnIP
            // 
            btnIP.Location = new Point(670, 12);
            btnIP.Name = "btnIP";
            btnIP.Size = new Size(75, 23);
            btnIP.TabIndex = 6;
            btnIP.Text = "Reset IP";
            btnIP.UseVisualStyleBackColor = true;
            btnIP.Click += btnIP_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label2.Location = new Point(304, 46);
            label2.Name = "label2";
            label2.Size = new Size(81, 21);
            label2.TabIndex = 7;
            label2.Text = "Username";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(775, 400);
            Controls.Add(label2);
            Controls.Add(btnIP);
            Controls.Add(cbShowPswd);
            Controls.Add(label1);
            Controls.Add(lblUser);
            Controls.Add(tbPswd);
            Controls.Add(tbUser);
            Controls.Add(btnLogin);
            Name = "Form1";
            Text = "Form1";
            Load += Form1_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnLogin;
        private TextBox tbUser;
        private TextBox tbPswd;
        private Label lblUser;
        private Label label1;
        private CheckBox cbShowPswd;
        private Button btnIP;
        private Label label2;
    }
}
