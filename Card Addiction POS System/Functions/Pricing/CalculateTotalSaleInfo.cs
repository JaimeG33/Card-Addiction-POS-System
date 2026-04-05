using Card_Addiction_POS_System.Functions.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Card_Addiction_POS_System.Functions.Pricing
{
    internal sealed class CalculateTotalSaleInfo
    {
        internal sealed class Result
        {
            public int TotalItems { get; init; }
            public decimal SubTotalPrice { get; init; }
            public decimal Rounding { get; init; }
            public decimal FinalPrice { get; init; }
        }

        public Result Calculate(
            IEnumerable<TransactionLineItem>? cartItems,
            decimal? requestedFinalPrice = null,
            SaleItem? saleItem = null)
        {
            var safeItems = cartItems ?? Enumerable.Empty<TransactionLineItem>();

            int totalItems = safeItems.Sum(x => Math.Max(0, x.AmtTraded));
            decimal subTotalPrice = safeItems.Sum(x => Math.Max(0, x.AmtTraded) * x.AgreedPrice);

            decimal finalPrice = requestedFinalPrice ?? subTotalPrice;
            finalPrice = decimal.Round(finalPrice, 2, MidpointRounding.AwayFromZero);

            decimal roundedSubTotal = decimal.Round(subTotalPrice, 2, MidpointRounding.AwayFromZero);
            decimal rounding = decimal.Round(finalPrice - roundedSubTotal, 2, MidpointRounding.AwayFromZero);

            var result = new Result
            {
                TotalItems = totalItems,
                SubTotalPrice = roundedSubTotal,
                Rounding = rounding,
                FinalPrice = finalPrice
            };

            // Keep SaleItem in sync every time totals are calculated
            if (saleItem != null)
            {
                saleItem.UpdateTotals(result.SubTotalPrice, result.FinalPrice);
            }

            return result;
        }
    }
}
