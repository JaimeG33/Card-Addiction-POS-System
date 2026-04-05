using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Card_Addiction_POS_System.Functions.Models
{
    // TransactionLine table has Composite Primary keys (saleId and transactionId). 
    
    public class TransactionLineItem
    {
        public int SaleId { get; set; } // saleId should be the same one that is currently selected
        public int TransactionId { get; set; } // TransactionId should increment for each item added to cart (starting from 1, then 2, then 3, etc...)
        public int CardGameId { get; set; }
        public int CardId { get; set; }
        public int ConditionId { get; set; }
        public string CardName { get; set; }
        public int SetId { get; set; }

        // Display-only value for cart grid Set column.
        // Uses abbreviation when available; otherwise set name lookup via setId.
        public string SetDisplay { get; set; } = string.Empty;

        public decimal TimeMktPrice { get; set; }
        public decimal AgreedPrice { get; set; }
        public int AmtTraded { get; set; }
        public int AmtInStock { get; set; }

        // Added for cart display/reference in future logic
        public int AmtInCase { get; set; }

        public bool BuyOrSell { get; set; } //True = Sold to customer, False = Store bought card from customer (set to True for now)
        public string Rarity { get; set; }
    }
}
