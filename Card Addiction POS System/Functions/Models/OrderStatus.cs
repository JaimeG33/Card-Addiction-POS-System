using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Card_Addiction_POS_System.Functions.Models
{
    public enum OrderStatus
    {
        PrePrep = 0, // No order has been placed yet, employee is ready / idle
        TakingOrder = 1, // Items have been added to cart, but order not yet finalized (likely to change amounts / prices / items)
        Processing = 2, // Items have been agreed apon, employee is fetching them from inventory (small chance that an item may be out of stock)
        ReadyForPickup = 3, // All items have been gathered / finalized, waiting for customer to pay and pick up (small chance to further modify agreed price)
        FinishedAndPaid = 4 // Customer has paid and picked up items, order is fully complete (prepare for next customer)
    }
}
