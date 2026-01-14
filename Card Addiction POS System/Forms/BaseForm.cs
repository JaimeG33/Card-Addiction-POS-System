using Syncfusion.WinForms.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace Card_Addiction_POS_System.Forms
{
    public partial class BaseForm : SfForm, IHasFormTitle
    {
        /// <summary>
        /// Set this to true when you’re switching to another form (navigation),
        /// so closing this one doesn’t exit the app.
        /// </summary>
        public bool IsNavigating { get; set; }

        // Expose FormTitle as a property (preferred contract for HeaderControl)
        public virtual string FormTitle { get; set; } = "Home";

        public BaseForm()
        {
            // Ensure designer-generated controls are created first so the designer can render.
            InitializeComponent();

            // Avoid running runtime-only wiring when Visual Studio designer is loading this type.
            try
            {
                if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                {
                    return;
                }

                this.FormClosed += BaseForm_FormClosed;
            }
            catch
            {
                // Swallow exceptions at design time to avoid preventing the designer from loading.
            }
        }

        private void BaseForm_Load(object sender, EventArgs e)
        {
             
        }
        private void BaseForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (!IsNavigating)
            {
                Application.Exit();
            }
        }
    }
}
