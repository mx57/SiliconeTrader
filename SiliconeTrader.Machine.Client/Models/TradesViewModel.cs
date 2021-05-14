using SiliconeTrader.Core;
using System;
using System.Collections.Generic;

namespace SiliconeTrader.Machine.Client.Models
{
    public class TradesViewModel : DefaultViewModel
    {
        public DateTimeOffset Date { get; set; }

        public double TimezoneOffset { get; set; }

        public List<TradeResult> Trades { get; set; }
    }
}