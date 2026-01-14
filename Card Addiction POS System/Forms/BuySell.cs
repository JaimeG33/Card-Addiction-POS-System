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
    public partial class BuySell : SfForm
    {
        public BuySell()
        {
            InitializeComponent();
        }

        private void BuySell_Load(object sender, EventArgs e)
        {
            // Avoid running layout/resize code while Visual Studio Designer is rendering.
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
            {
                return;
            }

            // Resize image and adjust layout
            ImageResizing();
            PositionLabels();
        }

        private void ImageResizing()
        {
            double imgHeight = tLP_img.Height;
            double imgWantedWidth = imgHeight * 0.84;
            int imgWidth = Convert.ToInt32(imgWantedWidth);

            tLP_img.Width = imgWidth;
        }
        private void PositionLabels()
        {
            if (this.Width >= 1200)
            {
                int spacing = 20;

                // Align lblInStock to be 20px to the right of tLP_img, and same top
                lblInStock.Location = new Point(tLP_img.Right + spacing, tLP_img.Top);

                // Position lblMarketPrice below lblInStock
                lblMktPrice.Location = new Point(lblInStock.Left, lblInStock.Bottom + 10);

                //Position tbPrice text box
                tbPrice.Location = new Point(lblMktPrice.Left, lblMktPrice.Bottom + 10);

                //Position tbAmtTraded text box
                tbAmtTraded.Location = new Point(lblMktPrice.Right + 25, lblMktPrice.Bottom + 10);

                //Position add2cart button
                btnAddCt.Location = new Point(tbPrice.Left, tbPrice.Bottom + 10);
                //Sale Info label stays where its at
            }
        }
    }
}
