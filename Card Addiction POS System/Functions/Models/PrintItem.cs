using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Card_Addiction_POS_System.Functions.Models
{
    internal sealed class PrintItem
    {
        public int CardId { get; set; }
        public int SetId { get; set; }

        public string CardName { get; set; } = string.Empty;
        public string Rarity { get; set; } = string.Empty;

        public string SetName { get; set; } = string.Empty;
        public string Abbreviation { get; set; } = string.Empty;

        public string CardType { get; set; } = string.Empty;
        public string CardColor { get; set; } = string.Empty;

        public int Quantity { get; set; }
        public int AmtInCase { get; set; }

        // Nullable so we can preserve SQL NULL ("Not Inv")
        public int? AmtInStock { get; set; }

        public bool IsNotInventory => !AmtInStock.HasValue;
        public bool IsCaseCard => AmtInCase > 0;
    }
}
