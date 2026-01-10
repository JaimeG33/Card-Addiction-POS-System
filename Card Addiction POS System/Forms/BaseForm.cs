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
            this.FormClosed += BaseForm_FormClosed;
            InitializeComponent();
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
