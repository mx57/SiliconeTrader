using SiliconeTrader.Core;
using System;
using System.Collections.Generic;

namespace SiliconeTrader.Machine.Client.Models
{
    public class StatsViewModel : DefaultViewModel
    {
        public decimal AccountBalance { get; set; }

        public decimal AccountInitialBalance { get; set; }

        public Dictionary<DateTimeOffset, decimal> Balances { get; set; }

        public string Market { get; set; }

        public double TimezoneOffset { get; set; }

        public Dictionary<DateTimeOffset, List<TradeResult>> Trades { get; set; }
    }
}