namespace Card_Addiction_POS_System.LogIn.Controls
{
    partial class HeaderControl
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
            btnHome = new Button();
            lblTitle = new Label();
            lblUser = new Label();
            SuspendLayout();
            // 
            // btnHome
            // 
            btnHome.Location = new Point(38, 30);
            btnHome.Name = "btnHome";
            btnHome.Size = new Size(114, 58);
            btnHome.TabIndex = 0;
            btnHome.Text = "Home";
            btnHome.UseVisualStyleBackColor = true;
            btnHome.Click += btnHome_Click;
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Segoe UI", 15.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblTitle.Location = new Point(547, 40);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(56, 30);
            lblTitle.TabIndex = 1;
            lblTitle.Text = "Title";
            // 
            // lblUser
            // 
            lblUser.AutoSize = true;
            lblUser.Location = new Point(1021, 9);
            lblUser.Name = "lblUser";
            lblUser.Size = new Size(36, 15);
            lblUser.TabIndex = 2;
            lblUser.Text = "User: ";
            // 
            // HeaderControl
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.ActiveBorder;
            Controls.Add(lblUser);
            Controls.Add(lblTitle);
            Controls.Add(btnHome);
            Name = "HeaderControl";
            Size = new Size(1145, 113);
            Load += HeaderControl_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnHome;
        private Label lblTitle;
        private Label lblUser;
    }
}
