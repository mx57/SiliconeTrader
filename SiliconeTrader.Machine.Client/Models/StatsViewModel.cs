using System;
using System.Collections.Generic;
using SiliconeTrader.Core;

namespace SiliconeTrader.Machine.Client.Models
{
    public class StatsViewModel : BotResponse, IInstanceVersion
    {
        public decimal AccountBalance { get; set; }

        public decimal AccountInitialBalance { get; set; }

        public Dictionary<DateTimeOffset, decimal> Balances { get; set; }

        public string Market { get; set; }

        public double TimezoneOffset { get; set; }

        public Dictionary<DateTimeOffset, List<TradeResult>> Trades { get; set; }
    }
}