using Card_Addiction_POS_System.Data.SQLServer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Card_Addiction_POS_System.LogIn.Controls
{
    public partial class HeaderControl : UserControl
    {
        public HeaderControl()
        {
            InitializeComponent();
            this.Load += HeaderControl_Load;
            this.Disposed += HeaderControl_Disposed;
        }

        private void HeaderControl_Load(object sender, EventArgs e)
        {
            // Prevent design-time execution of runtime logic that can throw while the designer loads types.
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
            {
                return;
            }

            ApplyTitleFromParentForm();
            ApplyUserFromConnectionFactory();

            // Keep the title updated if the form’s Text changes
            var form = this.FindForm();
            if (form != null)
            {
                form.TextChanged += (_, __) => ApplyTitleFromParentForm();
            }

            // Subscribe to username changes from the connection factory
            SqlConnectionFactory.CurrentUsernameChanged += OnCurrentUsernameChanged;
        }

        private void HeaderControl_Disposed(object? sender, EventArgs e)
        {
            // Unsubscribe to prevent leaks
            SqlConnectionFactory.CurrentUsernameChanged -= OnCurrentUsernameChanged;
        }

        private void OnCurrentUsernameChanged(object? sender, UserChangedEventArgs e)
        {
            UpdateUserLabel(e?.Username);
        }

        private void UpdateUserLabel(string? username)
        {
            if (lblUser.IsHandleCreated && lblUser.InvokeRequired)
            {
                lblUser.Invoke(() => UpdateUserLabel(username));
                return;
            }

            lblUser.Text = string.IsNullOrWhiteSpace(username)
                ? "User: Unknown"
                : $"User: {username}";
        }

        private void ApplyTitleFromParentForm()
        {
            var form = this.FindForm();
            if (form == null)
            {
                lblTitle.Text = "Form Title";
                return;
            }

            // 1) Preferred: interface
            if (form is Card_Addiction_POS_System.Forms.IHasFormTitle hasTitle && !string.IsNullOrWhiteSpace(hasTitle.FormTitle))
            {
                lblTitle.Text = hasTitle.FormTitle;
                return;
            }

            // 2) Fallback: reflection property "FormTitle" (property or field)
            var type = form.GetType();

            // Try property first (public or non-public)
            var prop = type.GetProperty("FormTitle", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (prop != null && prop.PropertyType == typeof(string))
            {
                var value = prop.GetValue(form) as string;
                if (!string.IsNullOrWhiteSpace(value))
                {
                    lblTitle.Text = value;
                    return;
                }
            }

            // Also allow a private field named "FormTitle"
            var field = type.GetField("FormTitle", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field != null && field.FieldType == typeof(string))
            {
                var value = field.GetValue(form) as string;
                if (!string.IsNullOrWhiteSpace(value))
                {
                    lblTitle.Text = value;
                    return;
                }
            }

            // 3) Use the actual form title text
            if (!string.IsNullOrWhiteSpace(form.Text))
            {
                lblTitle.Text = form.Text;
                return;
            }

            // 4) Default
            lblTitle.Text = "Form Title";
        }


        private void ApplyUserFromConnectionFactory()
        {
            try
            {
                // Read static CurrentUsername and update label
                UpdateUserLabel(SqlConnectionFactory.CurrentUsername);
            }
            catch
            {
                lblUser.Text = "User: Unknown";
            }
        }

        private void btnHome_Click(object sender, EventArgs e)
        {
            // Pressing this button should take the user to the HomePage form.
            var form = this.FindForm();
            if (form is Forms.HomePage)
            {
                // Already on HomePage, do nothing.
                return;
            }

            var homePage = new Forms.HomePage();

            // If the current form exposes an IsNavigating boolean (property or field),
            // set it to true so the form knows we're navigating and won't exit the app.
            if (form != null)
            {
                var type = form.GetType();

                var prop = type.GetProperty("IsNavigating", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (prop != null && prop.PropertyType == typeof(bool) && prop.CanWrite)
                {
                    prop.SetValue(form, true);
                }
                else
                {
                    var field = type.GetField("IsNavigating", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (field != null && field.FieldType == typeof(bool))
                    {
                        field.SetValue(form, true);
                    }
                }
            }

            homePage.Show();
            form?.Close();
        }
    }
}
