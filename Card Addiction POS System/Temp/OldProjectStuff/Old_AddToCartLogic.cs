using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Card_Addiction_POS_System.Temp.OldProjectStuff
{
    internal class Old_AddToCartLogic
    {
        // The following code shows how my old application handled adding a selected item to the cart. 
        //  The UpdatePrice() function is the same as in Old_UpdatePriceLogic.cs
        // The following code is not perfect, but it is functional and works for the purpose of adding items to the cart.
        // The main idea behind this is to store the selected items in a list of TransactionLineItem objects, then display that list in a DataGridView.
        // The database stores sales through two tables: Sale and TransactionLine. Sale is the main table that stores the overall sale information, while TransactionLine stores each individual item in the sale.
        // Throughout the process of the sale, the saleId is used to link the two tables together and keep track of which items belong to which sale and how many sales have been made per day. 
        // The order status is there to indicate the state of an order, in case something goes wrong and the sale needs to be resumed later.
        // Pre-Prep: initial state when sale is created (no items added yet) (Brand new sale being made)
        //Taking Order: items have been added to cart, but sale is not finalized yet (By this point, the SaleId is determined and a new one should not be created)
        // Processing: Items have been selected and the employee is getting the items from the back or preparing them for the customer
        // want to add a new one called "Ready for Pickup" which implies the items are ready but not paid for yet
        // Finished and Paid: Sale is complete and the customer has paid (this is the final state) (next sale will use a new SaleId)

        // First some stuff that is done when the form is loaded:
        private void BuySell_Load(object sender, EventArgs e)
        {
             CheckSeller();
            if (currentSaleStatus !=  null || currentSaleStatus == "finished and paid")
            {
                CheckTransactionId();
            }
            //these if statements are not connected
            if (string.IsNullOrEmpty(currentSaleStatus) && !(workingOnOrder == true))
            {
                GenerateSale();// updates the database and creates a row for the Sale table
            }
            btnFinalizeSale.Visible = false;
        }

        private void CheckSeller()
        {
            int saleId = 1; //Default if no sales today
            int employeeId = 10; //Fix later, just keep 10 for now
            //Get current date / time
            DateTime currentDateTime = DateTime.Now;
            DateTime currentDate = DateTime.Now.Date;

            //Queries to be used later
            string queryFindLatestSaleId = "SELECT ISNULL(MAX(saleId), 0) FROM dbo.Sale;";

            string queryCheckLastSale = @" 
        SELECT TOP 1 saleId, orderStatus, saleDate
        FROM dbo.Sale
        WHERE employeeId = @employeeId
        ORDER BY saleDate DESC;
                                        ";

            using (SqlConnection connection = new SqlConnection(connString))
            {
                connection.Open();

                // find the latest SaleId
                using (SqlCommand cmd1 = new SqlCommand(queryFindLatestSaleId, connection))
                {
                    object result = cmd1.ExecuteScalar();
                    if (result != DBNull.Value)
                    {
                        saleId = Convert.ToInt32(result);
                    }
                }

                // then check if the user has any unfinished orders
                using (SqlCommand cmd2 = new SqlCommand(queryCheckLastSale, connection))
                {
                    cmd2.Parameters.AddWithValue("@employeeId", employeeId);

                    using (SqlDataReader reader = cmd2.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string lastStatus = reader["orderStatus"] != DBNull.Value
                                ? reader["orderStatus"].ToString() : null;
                            //check back here
                            // If last sale is null or marked "finished and paid", treat it as done
                            if (string.IsNullOrEmpty(lastStatus) || lastStatus == "finished and paid")
                            {
                                currentSaleId = saleId + 1;
                                currentSaleStatus = null;
                                workingOnOrder = false;
                                selectedSellerDateTime = currentDateTime;
                            }
                            else
                            {
                                // Still working on previous sale
                                currentSaleId = Convert.ToInt32(reader["saleId"]);
                                currentSaleStatus = lastStatus;
                                workingOnOrder = true;
                                selectedSellerDateTime = Convert.ToDateTime(reader["saleDate"]);

                            }

                            return;
                        }
                    }
                }
                // 3. If employee has never made a sale, start new one
                //doesnt get to happen if the return function runs
                currentSaleId = saleId + 1;
                currentSaleStatus = null;
                workingOnOrder = false;
                selectedSellerDateTime = currentDateTime;
            }

        }
        private void GenerateSale()
        {
            DateTime currentDateTime = DateTime.Now;
            int employeeIdColumn = 10; //Make a funciton to get this later
            int registerColumn = 1; //Make a funciton to get this later
            int saleIdColumn = currentSaleId;

            if (!workingOnOrder == true)
            {
                currentSaleStatus = "pre-prep";

                string query = @"
    INSERT INTO dbo.Sale (
        saleDate,
        saleId,
        employeeId,
        orderStatus,
        register
    )
    VALUES (
        @saleDate,
        @saleId,
        @employeeId,
        @orderStatus,
        @register
    );
";
                using (SqlConnection connection = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@saleDate", currentDateTime);
                    cmd.Parameters.AddWithValue("@saleId", saleIdColumn);
                    cmd.Parameters.AddWithValue("@employeeId", employeeIdColumn);
                    cmd.Parameters.AddWithValue("@orderStatus", currentSaleStatus);
                    cmd.Parameters.AddWithValue("@register", registerColumn);

                    connection.Open();
                    cmd.ExecuteNonQuery();
                }
                workingOnOrder = true;
            }
            else if (workingOnOrder == true)
            {

            }

        }

        private List<TransactionLineItem> cartItems = new List<TransactionLineItem>(); //The list representing the selected items added to cart 
        // Each item selected from the available inventory will be added to this list as a TransactionLineItem object
        public class TransactionLineItem //(TransactioLine)
        {
            public int CardGameId { get; set; }
            public int CardId { get; set; }
            public int ConditionId { get; set; }
            public string CardName { get; set; }
            public int SetId { get; set; }
            public decimal TimeMktPrice { get; set; }
            public decimal AgreedPrice { get; set; }
            public int AmtTraded { get; set; }
            public int AmtInStock { get; set; }
            public bool BuyOrSell { get; set; } //True = Sold to customer, False = Store bought card from customer (set to True for now)
            public string Rarity { get; set; }
        }






        // The following snippet is the code for the Add to Cart button click event
        private void btnAddCt_Click(object sender, EventArgs e)
        {
            UpdatePrice(); //First update the price if needed to make sure its accurate (if the price is out of date)

            // Check to see if the item is being bought or sold
            decimal finalValue = Convert.ToDecimal(transactionPrice);
            if (modeBuySell == false)
            {
                finalValue = finalValue * -1;
            }
            // 1. Collect input from form (dropdowns, textboxes, etc.)
            // NOTE: in the old project, these variables were local in the BuySell form, but they are similar to what is currently in the InventoryItem class
            TransactionLineItem newItem = new TransactionLineItem
            {
                CardGameId = selectedCardGame,
                CardId = selectedCardId,
                ConditionId = selectedConditionId,
                CardName = selectedCardName,
                Rarity = selectedRarity,
                SetId = selectedSetId,
                TimeMktPrice = (decimal)selectedMktPrice,
                AgreedPrice = finalValue,
                AmtTraded = (int)transactionAmt,
                AmtInStock = selectedAmtInStock, // This value is only used by the ColorIndicator in the UI
                BuyOrSell = modeBuySell // true = store is selling,         false = store is buying item from customer
            };

            // 2. Add to cart
            cartItems.Add(newItem);

            // 3. Refresh the DataGridView
            if (currentSaleStatus == "pre-prep")
            {
                currentSaleStatus = "taking order";

            }
            btnFinalizeSale.Visible = true;
            SaleTransactionLineSystem();
            dataGridTransactionSystem.ReadOnly = true;
        }


        private void SaleTransactionLineSystem()
        {
            
            int employeeIdColumn = 10; //Make a funciton to get this later
            int registerColumn = 1; //Make a funciton to get this later

            DataTable dt = new DataTable();
            dt.Columns.Add("Type");              // "Sale" or "Item"
            dt.Columns.Add("Buy/Sell");
            dt.Columns.Add("Card Name");
            dt.Columns.Add("Rarity");
            dt.Columns.Add("Set ID");
            dt.Columns.Add("Qty");
            dt.Columns.Add("Agreed Price");
            dt.Columns.Add("Market Price");
            dt.Columns.Add("Total");

            //This is a Sale Info type of row (at the top of the grid)
            dt.Rows.Add("Sale Info", $"Sale ID: {currentSaleId}", " ", $"DateTime: {selectedSellerDateTime}", $"Employee ID: {employeeIdColumn}", "", "",
                $"Status: {currentSaleStatus ?? "N/A"}", $"Register: {registerColumn}");

            foreach (var item in cartItems) // cartItems is List<TransactionLineItem>
            {
                decimal total = item.AgreedPrice * item.AmtTraded;
                string buySell;
                if (item.BuyOrSell == true)
                {
                    buySell = "(Sell)";
                }
                else
                {
                    buySell = "(Buy)";
                }
                //This is an Item type of row (TransactionLine)
                // This adds a new row to the DataTable for each item in the cart (gets looped for each item in the cart)
                dt.Rows.Add("Item", buySell, item.CardName, item.Rarity, item.SetId, item.AmtTraded, item.AgreedPrice.ToString("C2"),
                        item.TimeMktPrice.ToString("C2"), total.ToString("C2"));
            }
            //Stuff at the bottom of the row
            decimal grandTotal = cartItems.Sum(i => i.AgreedPrice * i.AmtTraded);
            dt.Rows.Add("TOTAL", "", "", "", "", "", "", "", grandTotal.ToString("C2"));
            saleTotal = grandTotal;

            dataGridTransactionSystem.DataSource = dt;

            //Only allow the user to edit the Agreed Price column
            for (int i = 0; i < dataGridTransactionSystem.Rows.Count; i++)
            {
                var row = dataGridTransactionSystem.Rows[i];
                var cellValue = row.Cells[0].Value;
                if (cellValue != null && cellValue.ToString() == "Item")
                {
                    row.Cells["Agreed Price"].ReadOnly = false;

                    int transactionLineItemIndex = i - 1; //The corosponding value in the list
                    if (transactionLineItemIndex >= 0 && transactionLineItemIndex < cartItems.Count)
                    {
                        var item = cartItems[transactionLineItemIndex];
                        if (item.BuyOrSell == true && item.AmtTraded > item.AmtInStock)
                        {
                            // Change background color to light red if quantity exceeds stock
                            row.DefaultCellStyle.BackColor = Color.Red;
                        }
                    }
                }
                else
                {
                    row.ReadOnly = true;
                }
            }


            //change colors of certain rows to make it easier to read
            dataGridTransactionSystem.Rows[0].DefaultCellStyle.BackColor = Color.LightBlue; // Sale Info
            dataGridTransactionSystem.Rows[dataGridTransactionSystem.Rows.Count - 1].DefaultCellStyle.
                BackColor = Color.LightGray; // Total
                                             //Quantity check to see if the amount of cards requested is more than the amount in stock


            //also prevent user from adding new rows manually (might allow later, idk yet)
            dataGridTransactionSystem.AllowUserToAddRows = false;

        }

    }





    // The following sections are regarding processing the sale
    private void btnFinalizeSale_Click(object sender, EventArgs e)
        {
            TransactionLineLogic();
            UpdateSaleStatus();
            CalcSale();

            workingOnOrder = false;
            changingTabs = true;
            NavigationHelper.ReturnToHome(this, _homePage, ref changingTabs); // In the old application, this returned to the home page after finalizing the sale, but I don't want that to happen here

        }


        private void TransactionLineLogic()
        {
            int transactionId = currentTransactionId; //default

            // Use the cartItems list to write sql queries to insert into the database
            using (SqlConnection connection = new SqlConnection(connString))
            {
                connection.Open();
                // Step 1: TractionLine Table
                foreach (var item in cartItems) // Loop through all entries in the cartItems list to write them down as lines of the sql query
                {
                    string query = @"
                INSERT INTO TransactionLine 
                (transactionId, saleId, cardGameId, cardId, conditionId, cardName, rarity, setId, timeMktPrice, agreedPrice, amtTraded, buyOrSell)
                VALUES 
                (@transactionId, @saleId, @cardGameId, @cardId, @conditionId, @cardName, @rarity, @setId, @timeMktPrice, @agreedPrice, @amtTraded, @buyOrSell)";

                    using (SqlCommand cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@transactionId", currentTransactionId);
                        ++currentTransactionId;
                        cmd.Parameters.AddWithValue("@saleId", currentSaleId);
                        cmd.Parameters.AddWithValue("@cardGameId", item.CardGameId); //remember, this is the number value, not the text
                        cmd.Parameters.AddWithValue("@cardId", item.CardId);
                        cmd.Parameters.AddWithValue("@conditionId", item.ConditionId);
                        cmd.Parameters.AddWithValue("@cardName", item.CardName);
                        cmd.Parameters.AddWithValue("@rarity", item.Rarity);
                        cmd.Parameters.AddWithValue("@setId", item.SetId);
                        cmd.Parameters.AddWithValue("@timeMktPrice", item.TimeMktPrice);
                        cmd.Parameters.AddWithValue("@agreedPrice", item.AgreedPrice);
                        cmd.Parameters.AddWithValue("@amtTraded", item.AmtTraded);
                        cmd.Parameters.AddWithValue("@buyOrSell", item.BuyOrSell);

                        cmd.ExecuteNonQuery();
                    }
                }

                //Step 2: Update amtInStock columns of affected _Inventory tables
                // Assemble values of cartItems into batches, then create another sql query to update the amtInStock collumns of the affected tables
                var grouped = cartItems.GroupBy(x => x.CardGameId);

                foreach (var group in grouped)
                {
                    string tableName;
                    switch (group.Key)
                    {
                        case 1: // Yugioh
                            tableName = "YugiohInventory";
                            break;
                        case 2: // Magic
                            tableName = "MagicInventory";
                            break;
                        case 3: // Pokemon
                            tableName = "PokemonInventory";
                            break;
                        default:
                            continue; // Skip if not a valid card game
                    }

                    // Assemble sql querys to update the amtInStock collumns of the affected tables
                    var sb = new StringBuilder();
                    var idList = new List<int>(); // Keep track of all cardIds for WHERE clause

                    // Start building UPDATE query
                    sb.Append($"UPDATE {tableName} SET amtInStock = amtInStock + CASE cardId ");

                    // CASE WHEN for each item in the group
                    foreach (var item in group)
                    {
                        // If buyOrSell = true → shop sold → decrease stock
                        // If buyOrSell = false → shop bought → increase stock
                        int stockChange = item.BuyOrSell ? -item.AmtTraded : item.AmtTraded;

                        // Add a CASE entry: WHEN {cardId} THEN {amount to add/subtract}
                        sb.Append($"WHEN {item.CardId} THEN {stockChange} ");

                        // Track cardId for the WHERE clause
                        idList.Add(item.CardId);
                    }

                    // Close out the CASE expression and add WHERE with all cardIds
                    sb.Append("END WHERE cardId IN (");
                    sb.Append(string.Join(",", idList)); // e.g., (111,222,333)
                    sb.Append(");");

                    // Execute the single batched UPDATE statement
                    using (SqlCommand cmdUpdate = new SqlCommand(sb.ToString(), connection))
                    {
                        cmdUpdate.ExecuteNonQuery();
                    }
                }
                connection.Close();
            }
            // Step 3: Refresh DataGridView
            // Update the DataGridView to show the new transaction lines in the UI
            currentSaleStatus = "processing";
            // The Status column is the 6th column (index 6)
            dataGridTransactionSystem.Rows[0].Cells[6].Value = $"Status: {currentSaleStatus ?? "N/A"}";
            MessageBox.Show("Transaction lines saved successfully.");
        }

        private void UpdateSaleStatus()
        {
            string query = @"
UPDATE dbo.Sale
SET orderStatus = @orderStatus
WHERE saleId = @saleId;"; // took out: AND CAST(saleDate AS DATE) = CAST(@saleDate AS DATE)
            using (SqlConnection connection = new SqlConnection(connString))
            {
                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@orderStatus", currentSaleStatus);
                    cmd.Parameters.AddWithValue("@saleId", currentSaleId);
                    //cmd.Parameters.AddWithValue("@saleDate", selectedSellerDateTime);

                    connection.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    connection.Close();


                    MessageBox.Show($"{rowsAffected} sale(s) updated to status '{currentSaleStatus}'.\n" +
                        $"This is where you should go collect the cards in the back");
                }
            }
        }

        private void CalcSale()
        {
            currentSaleStatus = "finished and paid";
            string query = @"
UPDATE dbo.Sale
SET 
    orderStatus = @orderStatus,
    profit = @saleTotal
WHERE 
    saleId = @saleId;"; // took out: AND CAST(saleDate AS DATE) = CAST(@saleDate AS DATE)
            using (SqlConnection connection = new SqlConnection(connString))
            {
                connection.Open();
                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@orderStatus", currentSaleStatus);
                    cmd.Parameters.AddWithValue("@saleTotal", saleTotal);
                    cmd.Parameters.AddWithValue("@saleId", currentSaleId);

                    cmd.ExecuteNonQuery();//finaly, hopefully this will actually work
                }
            }
            MessageBox.Show($"Order {currentSaleId} has been filled. Returning to home now \n" +
                $"{currentSaleStatus}");
        }



    }
