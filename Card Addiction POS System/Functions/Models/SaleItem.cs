using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Card_Addiction_POS_System.Functions.Models
{
    internal class SaleItem
    {

        // This class represents a sale transaction in the POS system.



        public int SaleId { get; set; } // Primary Key (new for each sale) (generally will have a new key whenever OrderStatus is set to "Pre-Prep")
        public DateTimeOffset SaleDate { get; set; }
        public byte RegisterId { get; set; } // Register where the sale was made (will create logic to determine later)
        public byte? EmployeeId { get; set; } // Employee who made the sale (will create logic to determine later)
        public short? CustomerId { get; set; } // If the customer who made the sale has an account with the store (if applicable)

        public string OrderStatus  { get; set; } // see Models/OrderStatus.cs for reference


    }
}
