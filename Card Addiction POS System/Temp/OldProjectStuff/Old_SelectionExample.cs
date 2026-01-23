using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Card_Addiction_POS_System.Temp.OldProjectStuff
{
    // The folowing code is from an older implementation of price updating logic.
    // It is not currently in use, but is kept here for reference.

    internal class Old_SelectionExample
    {
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0) // Make sure it's not a header click?
            {
                // Get the column that was clicked
                string clickedColumn = dataGridView1.Columns[e.ColumnIndex].Name;
                // Select the row that was clicked
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];
                //gets the selected image url
                string imageURL = row.Cells["imageURL"].Value?.ToString().Trim();


                //gets the selected values (used in pricecheck function) 
                selectedAmtInStock = Convert.ToInt32(row.Cells["amtInStock"].Value?.ToString());
                selectedPriceURL = row.Cells["mktPriceURL"].Value?.ToString();
                selectedPriceUp2Date = Convert.ToBoolean(row.Cells["priceUp2Date"].Value);
                selectedRarity = row.Cells["rarity"].Value?.ToString();
                //Format market price onto the label properly and record value
                string mktPriceRaw = row.Cells["mktPrice"].Value?.ToString();

                if (double.TryParse(mktPriceRaw, System.Globalization.NumberStyles.Currency,
                System.Globalization.CultureInfo.CreateSpecificCulture("en-US"), out double mktPriceParsed))
                {
                    selectedMktPrice = mktPriceParsed;
                    TransactionPriceLogic();
                    lblMktPrice.Text = $"Market Price: {mktPriceParsed.ToString("C2", System.Globalization.CultureInfo.CreateSpecificCulture("en-US"))}";
                    tbPrice.Text = transactionPrice.ToString("C2");
                }
                else
                {
                    lblMktPrice.Text = "Market Price: N/A";
                }


                if (selectedPriceUp2Date == false)
                {
                    lblMktPrice.Text = lblMktPrice.Text + " (Price not up to Date)";
                }


                selectedCardId = Convert.ToInt32(row.Cells["cardId"].Value);
                selectedConditionId = Convert.ToInt32(row.Cells["conditionId"].Value?.ToString());
                selectedSetId = Convert.ToInt32(row.Cells["setId"].Value?.ToString());
                selectedCardName = row.Cells["cardName"].Value?.ToString();


                //gets the selected amount in stock value
                var value = row.Cells["amtInStock"].Value;
                if (value != null && double.TryParse(value.ToString(), out double parsed))
                {
                    lblInStock.Text = $"Amount In Stock: {parsed}";
                }
                else
                {
                    lblInStock.Text = "Amount In Stock: N/A";
                }




                if (!string.IsNullOrEmpty(imageURL))
                {
                    try //the old (simple) method got blocked by tcgplayer
                    {
                        // Create a new HTTP request to the image URL
                        var request = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(imageURL);
                        // Spoof a browser User-Agent to avoid being blocked by the server (403 Forbidden)
                        request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64)";
                        // Set a Referer header to make the request appear as if it's coming from a browser visiting TCGPlayer
                        request.Referer = "https://www.tcgplayer.com/";
                        // Accept header tells the server what types of content we want (images mostly)
                        request.Accept = "image/webp,image/apng,image/*,*/*;q=0.8";
                        // Send the request and get the response (which should contain the image data)
                        using (var response = request.GetResponse())
                        // Get the image stream from the response
                        using (var stream = response.GetResponseStream())
                        {
                            // Convert the stream into an Image object and display it in the PictureBox
                            imgCardUrl.Image = Image.FromStream(stream);
                        }
                    }
                    catch (Exception ex)
                    {
                        imgCardUrl.Image = null;
                        MessageBox.Show($"Unable to load image.\n\nError: {ex.Message}", "Image Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                if (clickedColumn == "priceUp2Date")
                {
                    //check to see if the selected row is up to date
                    bool isUp2Date = Convert.ToBoolean(dataGridView1.Rows[e.RowIndex].Cells["priceUp2Date"].Value);
                    if (isUp2Date == false)
                    {
                        UpdatePrice();
                    }
                }
            }
            else
            {
                imgCardUrl.Image = null;
            }


        }
    }
}
