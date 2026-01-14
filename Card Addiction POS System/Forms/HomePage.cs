using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Syncfusion.WinForms.Controls;

namespace Card_Addiction_POS_System.Forms
{
    public partial class HomePage : SfForm
    {
        // This is the home page used to navigate to other forms.
        public bool IsNavigating { get; set; }
        public HomePage()
        {
            InitializeComponent();
        }

        private void HomePage_Load(object sender, EventArgs e)
        {

        }

        private void HomePage_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (IsNavigating)
            {
                return; // Do nothing if navigating to another form
            }
            else
            {
                Application.Exit(); // Exit the application if not navigating
            }
        }

        private void btnSale_Click(object sender, EventArgs e)
        {
            IsNavigating = true;
            var posForm = new BuySell();
            posForm.Show();
            this.Close();  // BaseForm will see IsNavigating == true and NOT exit app
        }
    }
}
