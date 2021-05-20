using SiliconeTrader.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace SiliconeTrader.Core
{
    public class TradeResult : ITradeResult
    {
        public bool IsSuccessful { get; set; }
        public bool IsSwap { get; set; }
        public bool IsArbitrage { get; set; }
        public string Pair { get; set; }
        public decimal Amount { get; set; }
        public List<DateTimeOffset> OrderDates { get; set; }
        public decimal AveragePrice { get; set; }
        public decimal Fees { get; set; }
        public decimal FeesTotal => this.Fees + (this.Metadata?.FeesNonDeductible ?? 0);
        public decimal Cost => this.AveragePrice * this.Amount;
        public DateTimeOffset SellDate { get; set; }
        public decimal SellPrice { get; set; }
        public decimal SellCost => this.SellPrice * this.Amount;
        public decimal BalanceOffset { get; set; }
        public decimal Profit { get; set; }
        public OrderMetadata Metadata { get; set; }
    }
}
