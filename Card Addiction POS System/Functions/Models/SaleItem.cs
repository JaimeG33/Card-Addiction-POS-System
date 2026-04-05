using System;

namespace Card_Addiction_POS_System.Functions.Models
{
    internal class SaleItem
    {
        // This class represents a sale transaction in the POS system.

        public int SaleId { get; set; } // Primary Key (new for each sale)
        public DateTimeOffset SaleDate { get; set; }
        public byte RegisterId { get; set; } // Register where the sale was made
        public byte? EmployeeId { get; set; } // Employee who made the sale
        public short? CustomerId { get; set; } // Optional customer account id

        public string OrderStatus { get; set; } = string.Empty; // see Models/OrderStatus.cs

        public decimal Subtotal { get; set; } // Total amount before taxes/discounts
        public decimal Rounding { get; set; } // FinalTotal - Subtotal
        public decimal FinalTotal { get; set; } // Final total amount

        public decimal Expenses { get; set; } // Optional expenses associated with sale

        public void UpdateTotals(decimal subtotal, decimal finalTotal)
        {
            Subtotal = RoundMoney(subtotal);
            FinalTotal = RoundMoney(finalTotal);
            Rounding = RoundMoney(FinalTotal - Subtotal);
        }

        private static decimal RoundMoney(decimal value)
        {
            return decimal.Round(value, 2, MidpointRounding.AwayFromZero);
        }
    }
}
