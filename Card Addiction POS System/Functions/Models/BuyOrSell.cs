using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Card_Addiction_POS_System.Functions.Models
{
    public enum BuyOrSell 
    //True = Sold to customer, False = Store bought card from customer (set to True for now)
    {
        Buy = 0,
        Sell = 1
    }
}
