using Microsoft.Data.SqlClient;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Card_Addiction_POS_System.Temp.OldProjectStuff

// The folowing code is from an older implementation of price updating logic.
// It is not currently in use, but is kept here for reference.
{
    internal class Old_UpdatePriceLogic
    {
        // This code was used to find the market price of a card from tcgplayer using the mktPriceURL, while also bypassing restrictions set by the website to allow access to the information.
        private void UpdatePrice()
        {
            if (selectedPriceUp2Date == false)
            {
                lblMktPrice.BackColor = Color.LightYellow; // change the color so the user knows the program is loading and not frozen
                                                           //remember the selected position
                int currentRowIndex = dataGridView1.CurrentRow?.Index ?? -1;
                //updates the actual database, but doesnt imediately show on the DataGridView
                PriceCheck();
                // Refresh table
                LoadInventoryData(tbSearchBar.Text.Trim());
                // Restore position
                if (currentRowIndex >= 0 && currentRowIndex < dataGridView1.Rows.Count)
                {
                    dataGridView1.ClearSelection();
                    dataGridView1.Rows[currentRowIndex].Selected = true;
                    dataGridView1.FirstDisplayedScrollingRowIndex = currentRowIndex;
                }
                lblMktPrice.BackColor = SystemColors.AppWorkspace; // return the color back to normal
            }
        }




        private void PriceCheck()
        {
            try
            {
                driver.Navigate().GoToUrl(selectedPriceURL);


                // Wait until price is visible AND has non-empty text
                var priceElement = wait.Until(driver =>
                {
                    try
                    {
                        var el = driver.FindElement(By.CssSelector("span.price-points__upper__price"));
                        if (el != null && el.Displayed && !string.IsNullOrWhiteSpace(el.Text))
                            return el;
                    }
                    catch { }
                    return null;
                });


                // (Optional but recommended)
                Thread.Sleep(1200); // let Vue finish binding text


                string rawPrice = priceElement.Text.Trim()
                                                    .Replace("$", "")
                                                    .Replace(",", "");


                selectedMktPrice = double.Parse(rawPrice, System.Globalization.CultureInfo.InvariantCulture);


                lblMktPrice.Text = "Market Price: " + selectedMktPrice.ToString("C2", System.Globalization.CultureInfo.CreateSpecificCulture("en-US"));
                TransactionPriceLogic();
                tbPrice.Text = transactionPrice.ToString("C2");


                selectedPriceUp2Date = true;
                lblMktPrice.Font = new Font(lblMktPrice.Font, FontStyle.Regular);


                UpdateDBwPriceCheck();
            }
            catch (WebDriverTimeoutException)
            {
                MessageBox.Show("Price not found in time. The page may have loaded too slowly.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }







        private void LoadInventoryData(string searchText)
        {
            string choice = cbCardGame.Text;
            searchText = tbSearchBar.Text.Trim();
            string cardGame = string.Format("{0}Inventory", choice); //Chose card game from dropdown menu


            string query = string.Format("SELECT cardName, rarity, setId, mktPrice, conditionId, amtInStock, priceUp2Date, imageURL, mktPriceURL, cardId " +
                                         "FROM {0} " +
                                         "WHERE cardName LIKE @cardName " +
                                         "ORDER BY cardName;", cardGame);


            //Before connecting to database, check to see if valid card game is selected
            List<string> allowedTables = new List<string>
    { "YugiohInventory", "MagicInventory", "PokemonInventory" };
            if (!allowedTables.Contains(cardGame))
            {
                MessageBox.Show("Invalid card game selection.");
                return;
            }


            //connect to database using connection string
            using (SqlConnection connection = new SqlConnection(connString))
            {
                try
                {
                    connection.Open();
                    //enter the query
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        //Filter by the text from the search bar
                        command.Parameters.AddWithValue("@cardName", "%" + searchText + "%");




                        //Now display the results on the data table
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            DataTable dataTable = new DataTable();
                            dataTable.Load(reader);


                            if (dataTable.Rows.Count == 0)
                            {
                                MessageBox.Show($"No results found for \"{searchText}\".");
                                dataGridView1.DataSource = null;
                            }
                            else
                            {
                                dataGridView1.DataSource = dataTable;


                                // Visual Stuff for the collumns
                                dataGridView1.Columns["cardName"].HeaderText = "Card Name";
                                dataGridView1.Columns["rarity"].HeaderText = "Rarity";
                                dataGridView1.Columns["mktPrice"].HeaderText = "Market Price";
                                dataGridView1.Columns["conditionId"].HeaderText = "Condition";
                                dataGridView1.Columns["amtInStock"].HeaderText = "In Stock";
                                dataGridView1.Columns["priceUp2Date"].HeaderText = "Up 2 Date";
                                //Hide urls and unnessisary columns
                                if (dataGridView1.Columns.Contains("imageURL"))
                                    dataGridView1.Columns["imageURL"].Visible = false;
                                if (dataGridView1.Columns.Contains("mktPriceURL"))
                                    dataGridView1.Columns["mktPriceURL"].Visible = false;
                                if (dataGridView1.Columns.Contains("cardId"))
                                    dataGridView1.Columns["cardId"].Visible = false;
                                //Width of collumns
                                dataGridView1.Columns["conditionId"].Width = 55;
                                dataGridView1.Columns["setId"].Width = 60;
                                dataGridView1.Columns["mktPrice"].Width = 80;
                                dataGridView1.Columns["amtInStock"].Width = 40;
                                dataGridView1.Columns["priceUp2Date"].Width = 40;
                                dataGridView1.Columns["cardName"].Width = 150;
                                dataGridView1.Columns["mktPrice"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                                dataGridView1.Columns["mktPrice"].DefaultCellStyle.Format = "C2";
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading inventory: " + ex.Message);
                }
            }
        }
    }
}
