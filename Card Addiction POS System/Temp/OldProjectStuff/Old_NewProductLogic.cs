using CsvHelper;
using CsvHelper.Configuration;
using Demo_Application_1.Helpers;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Demo_Application_1.Forms
{
    public partial class AddInventory : Form
    {
        // State variables for handling navigation and database connection
        private HomePage _homePage; // Reference to the HomePage form
        private bool changingTabs = false; // Tracks if the tab is being changed
        private string connString; // Database connection string

        // Constructor: Initializes form components and sets up references
        public AddInventory(string connectionString, HomePage homePage)
        {
            InitializeComponent();

            // Save the database connection string and reference to the HomePage form
            connString = connectionString;
            _homePage = homePage;

        }

        // SECTION: Initialization and Loading

        // Important Variables:
        public int selectedCardGameId; // Stores the selected card game ID
        public int selectedSetId; // Stores the selected set ID
        public string selectedSetName; // Stores the selected set name
        public string selectedTableName; // Stores the selected table name



        // Event triggered when the form loads
        private void AddInventory_Load(object sender, EventArgs e)
        {
            // Initialize card game selection options
            var cardGames = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("1", "Yugioh"),
                new KeyValuePair<string, string>("2", "Magic"),
                new KeyValuePair<string, string>("3", "Pokemon")
            };
            cbCardGame.DataSource = new BindingSource(cardGames, null);
            cbCardGame.DisplayMember = "Value"; // Display card game names
            cbCardGame.ValueMember = "Key"; // Store card game IDs internally

            // Maximize the form window upon loading
            this.WindowState = FormWindowState.Maximized;
        }

        // SECTION: Drag-and-Drop File Upload

        // Event triggered when a file is dragged over the drag-and-drop panel
        private void pDragDrop_DragEnter(object sender, DragEventArgs e)
        {
            // Indicate that the file can be dropped if it is a valid file
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        // Event triggered when a file is dropped into the drag-and-drop panel
        private void pDragDrop_DragDrop(object sender, DragEventArgs e)
        {
            // Retrieve file paths from drag-and-drop action
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length > 0 && files[0].EndsWith(".csv")) // Ensure the file is a.csv file
            {
                string filePath = files[0]; // Get the first (and only) file path
                try
                {
                    // Parse the CSV file into a list of YourDataClass objects
                    var records = ReadCSV<YourDataClass>(filePath);

                    // Bind parsed records to DataGridView
                    dataGridView1.DataSource = records;

                    // Customize DataGridView appearance
                    CustomizeDataGridView();

                    // Notify success
                    MessageBox.Show("CSV processed successfully!");
                    pasteORupload.Visible = false;

                    // Begin processing to SQL queries
                    SetSetup();

                }
                catch (Exception ex)
                {
                    // Show error message if parsing fails
                    MessageBox.Show($"Error processing the CSV file:\n{ex.Message}\n\nDetails:\n{ex.StackTrace}");
                }
            }
            else
            {
                // Show error if an invalid file is uploaded
                MessageBox.Show("Please upload a valid.csv file.");
            }
        }

        // SECTION: DataGridView Customization

        // Method to customize the DataGridView headers and appearance
        private void CustomizeDataGridView()
        {
            // Rename the headers to be more user-friendly
            dataGridView1.Columns["cardId"].HeaderText = "Card ID";
            dataGridView1.Columns["cardName"].HeaderText = "Card Name";
            dataGridView1.Columns["setId"].HeaderText = "Set ID";
            dataGridView1.Columns["rarity"].HeaderText = "rarity";
            dataGridView1.Columns["imageURL"].HeaderText = "Image URL";
            dataGridView1.Columns["mktPriceUrl"].HeaderText = "Market Price URL";
            dataGridView1.Columns["mktPrice"].HeaderText = "Market Price";

            // Optionally hide the extNumber column if it is not needed
            if (dataGridView1.Columns.Contains("extNumber"))
            {
                dataGridView1.Columns["extNumber"].Visible = false;
            }
        }

        // SECTION: CSV Parsing and Mapping

        // Method to read and parse the CSV file using CsvHelper
        // This method maps the file headers to YourDataClass properties
        public List<T> ReadCSV<T>(string filePath) where T : class
        {
            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                // Register the header mapping for YourDataClass
                csv.Context.RegisterClassMap<YourDataClassMap>();

                // Read and parse records from the CSV file
                var records = csv.GetRecords<T>().ToList();
                return records;
            }
        }

        // SECTION: Converting DataGridView Data to SQL Query

        // Method to setup key variables
        private void SetSetup()
        {
            gbSetInfo.Visible = true;


            if (dataGridView1 != null)
            {
                try
                {
                    selectedCardGameId = int.Parse(cbCardGame.SelectedValue.ToString());
                    selectedTableName = cbCardGame.Text.Replace(" ", "");
                    selectedSetId = int.Parse(dataGridView1.Rows[0].Cells["setId"].Value.ToString());
                    selectedSetName = dataGridView1.Rows[0].Cells["cardName"].Value.ToString();


                    tbGroupId.Text = selectedSetId.ToString();
                    int setId = tbGroupId.Text != "" ? int.Parse(tbGroupId.Text) : 0;
                    tbItemName.Text = selectedSetName;
                    lblCardGameId.Text = selectedCardGameId.ToString();
                    lblTable.Text = string.Format(selectedTableName + "Set and " + selectedTableName + "Inventory");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error setting up variables:\n{ex.Message}\n\nDetails:\n{ex.StackTrace}");
                }
            }
        }



        // Method to convert the DataGridView data into SQL queries for database insertion
        private void ConvertDataGridViewToSqlQuery()
        {
            string setTableName = selectedTableName + "Set";
            string inventoryTableName = selectedTableName + "Inventory";

            if (dataGridView1.DataSource is List<YourDataClass> records && records.Count > 0)
            {
                bool setExists = false;
                // Check if setId already exists in the Set table
                string checkSetQuery = $@"SELECT COUNT(*) FROM [{setTableName}] WHERE setId = @setId";
                using (SqlConnection connection = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand(checkSetQuery, connection))
                {
                    cmd.Parameters.AddWithValue("@setId", selectedSetId);
                    connection.Open();
                    int count = (int)cmd.ExecuteScalar();
                    setExists = count > 0;
                }

                // Insert the set info only if it doesn't exist
                if (!setExists)
                {
                    string setQuery = $@"
                        INSERT INTO [{setTableName}] (setId, cardGameId, setName)
                        VALUES (@setId, @cardGameId, @setName);";

                    using (SqlConnection connection = new SqlConnection(connString))
                    using (SqlCommand cmd = new SqlCommand(setQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@setId", selectedSetId);
                        cmd.Parameters.AddWithValue("@cardGameId", selectedCardGameId);
                        cmd.Parameters.AddWithValue("@setName", selectedSetName);

                        connection.Open();
                        cmd.ExecuteNonQuery();
                    }
                }

                // Insert each inventory record
                string inventoryQuery = $@"
                    INSERT INTO [{inventoryTableName}] (cardId, conditionId, cardGameId, setId, cardName, rarity, imageURL, mktPriceURL, mktPrice, amtInStock)
                    VALUES (@cardId, @conditionId, @cardGameId, @setId, @cardName, @rarity, @imageURL, @mktPriceURL, @mktPrice, @amtInStock);";

                using (SqlConnection connection = new SqlConnection(connString))
                {
                    connection.Open();
                    foreach (var card in records)
                    {
                        // Sanitize and parse market price
                        decimal safeMktPrice = 0m;
                        if (card.mktPrice.HasValue)
                        {
                            safeMktPrice = card.mktPrice.Value;
                        }

                        // Determine conditionId based on selectedTableName and subTypeName
                        int conditionId = 0;
                        string subType = card.subTypeName?.Trim() ?? "";

                        if (selectedTableName.Equals("Yugioh", StringComparison.OrdinalIgnoreCase))
                        {
                            if (subType.Equals("Normal", StringComparison.OrdinalIgnoreCase))
                                conditionId = 10;
                            else if (subType.StartsWith("1", StringComparison.OrdinalIgnoreCase) ||
                                     subType.StartsWith("F", StringComparison.OrdinalIgnoreCase) ||
                                     subType.StartsWith("1st", StringComparison.OrdinalIgnoreCase))
                                conditionId = 1;
                            else
                                conditionId = 0;
                        }
                        else if (selectedTableName.Equals("Magic", StringComparison.OrdinalIgnoreCase))
                        {
                            conditionId = 10; // default
                            if (subType.Equals("Normal", StringComparison.OrdinalIgnoreCase))
                                conditionId = 0;
                            else if (subType.Equals("Foil", StringComparison.OrdinalIgnoreCase))
                                conditionId = 1;
                        }
                        else if (selectedTableName.Equals("Pokemon", StringComparison.OrdinalIgnoreCase))
                        {
                            conditionId = 10; // default
                            if (subType.Equals("Normal", StringComparison.OrdinalIgnoreCase))
                                conditionId = 0;
                            else if (subType.Equals("Foil", StringComparison.OrdinalIgnoreCase))
                                conditionId = 1;
                            else if (subType.Equals("Holofoil", StringComparison.OrdinalIgnoreCase))
                                conditionId = 2;
                            else if (subType.Equals("Reverse Holofoil", StringComparison.OrdinalIgnoreCase))
                                conditionId = 3;
                        }

                        using (SqlCommand cmd = new SqlCommand(inventoryQuery, connection))
                        {
                            cmd.Parameters.AddWithValue("@cardId", card.cardId);
                            cmd.Parameters.AddWithValue("@conditionId", conditionId); // <-- Use calculated value
                            cmd.Parameters.AddWithValue("@setId", card.setId);
                            cmd.Parameters.AddWithValue("@cardGameId", selectedCardGameId); // <-- Use user selection
                            cmd.Parameters.AddWithValue("@cardName", card.cardName);
                            cmd.Parameters.AddWithValue("@rarity", card.rarity ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@imageURL", card.imageUrl ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@mktPriceURL", card.mktPriceUrl ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@mktPrice", safeMktPrice);
                            cmd.Parameters.AddWithValue("@amtInStock", card.amtInStock);

                            cmd.ExecuteNonQuery();
                        }
                    }
                }

                // Notify the user that queries were generated and executed
                MessageBox.Show("The new set and inventory were successfully added to the database!");
            }
            else
            {
                // No data in DataGridView
                MessageBox.Show("No data available to generate SQL queries.");
            }
        }

        // SECTION: Navigation and Profile Handling

        // Event triggered when the user selects "Return Home" from the profile dropdown
        private void cbxProfile_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbxProfile.Text == "Return Home")
            {
                changingTabs = true; // Set tab-changing flag
                NavigationHelper.ReturnToHome(this, _homePage, ref changingTabs); // Navigate back to HomePage
            }
        }

        // SECTION: Data Class and Mapping

        // Class representing the data structure for the inventory
        public class YourDataClass
        {
            public int cardId { get; set; } // Maps to "cardId"
            public string cardName { get; set; } // Maps to "cardName"
            public int setId { get; set; } // Maps to "setId"
            public string rarity { get; set; } // Maps to "rarity"
            public string imageUrl { get; set; } // Maps to "imageUrl"
            public string mktPriceUrl { get; set; } // Maps to "mktPriceURL"
            public decimal? mktPrice { get; set; } // <-- Make this nullable
            public int amtInStock { get; set; } // New column "amtInStock"
            public string priceUp2Date { get; set; } // New column "priceUp2Date"
            public string subTypeName { get; set; } // <-- Add this property
        }

        // CsvHelper ClassMap for mapping the CSV headers to YourDataClass properties
        public class YourDataClassMap : ClassMap<YourDataClass>
        {
            public YourDataClassMap()
            {
                // Map the CSV header names to class properties
                Map(m => m.cardId).Name("productId"); // Matches CSV header
                Map(m => m.cardName).Name("name"); // Matches CSV header
                Map(m => m.setId).Name("groupId"); // Matches CSV header
                Map(m => m.rarity).Name("extRarity"); // Matches CSV header
                Map(m => m.imageUrl).Name("imageUrl"); // Matches CSV header
                Map(m => m.mktPriceUrl).Name("url"); // Matches CSV header
                Map(m => m.mktPrice).Name("marketPrice").TypeConverterOption.NullValues(string.Empty); // <-- Allow empty string as null
                Map(m => m.subTypeName).Name("subTypeName"); // <-- Map this column

                // Remove these mappings if the CSV doesn't contain corresponding headers
                // or add the headers `amtInStock` and `priceUp2Date` to the CSV
                // Map(m => m.amtInStock).Name("amtInStock");
                // Map(m => m.priceUp2Date).Name("priceUp2Date");
            }
        }
        // Event handler for when the selected index of cbCardGame changes
        private void cbCardGame_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Add your logic here, if any
            // For example, store the selected card game ID
            if (cbCardGame.SelectedItem != null)
            {
                string cardGameId = cbCardGame.SelectedValue.ToString();
                // MessageBox.Show($"Selected Card Game ID: {cardGameId}");
            }
        }

        // Event handler for when the form is closed
        private void AddInventory_FormClosed(object sender, FormClosedEventArgs e)
        {
            // Logic for when the form is closed
            if (changingTabs == false)
            {
                Application.Exit(); // Ensure the application is terminated
            }
        }

        private void btnTDD_Click(object sender, EventArgs e)
        {
            bool seeTDD;

            if (pasteORupload.Visible == true)
            {
                pasteORupload.Visible = false;
                seeTDD = false;
            }
            else
            {
                pasteORupload.Visible = true;
                seeTDD = true;
            }
        }

        // Push all the changes onto the database
        private void btnConfirmAllInfo_Click(object sender, EventArgs e)
        {
            ConvertDataGridViewToSqlQuery();
        }

        private void splitContainer1_Panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void pasteORupload_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}