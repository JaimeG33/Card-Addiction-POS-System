using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Card_Addiction_POS_System.Functions.Models
{
    public class InventoryItem
    {
        public string CardName { get; set; } = "";
        public string Rarity { get; set; } = "";
        public int SetId { get; set; }
        public decimal MktPrice { get; set; }
        public int ConditionId { get; set; }
        public int AmtInStock { get; set; }
        public bool PriceUp2Date { get; set; }
        public string? ImageUrl { get; set; }
        public string? MktPriceUrl { get; set; }
        public int CardId { get; set; }
    }
}
