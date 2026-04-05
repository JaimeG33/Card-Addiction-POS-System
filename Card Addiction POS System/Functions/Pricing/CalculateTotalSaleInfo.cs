using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Card_Addiction_POS_System.Functions.Pricing
{
    internal class CalculateTotalSaleInfo
    {
        // Will eventually result in the following outputs
        int totalItems; // sum of quantity of all items in the sale, represents the amount of cards being sold
        decimal subTotalPrice; // sum of (quantity * unit price) for all items in the sale, represents the total price of the sale
        decimal rounding; // the ammount added to the subTotalPrice to get to the finalPrice
        decimal finalPrice; // the final total price of all items in the sale. 
        // users can set a custom final price for the sale, which will adjust the rounding accordingly.
        // ex: $24.99 subTotalPrice, user sets finalPrice to $20, then rounding will be -$4.99
        // ex: $19.50 subTotalPrice, user sets finalPrice to $20, then rounding will be +$0.50

    }
}
