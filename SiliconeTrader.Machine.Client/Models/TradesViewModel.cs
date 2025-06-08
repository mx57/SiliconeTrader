using System;
using System.Collections.Generic;
using SiliconeTrader.Core;
using SiliconeTrader.Core.Models; // Added for BotResponse

namespace SiliconeTrader.Machine.Client.Models
{
    public class TradesViewModel : BotResponse
    {
        public DateTimeOffset Date { get; set; }

        public double TimezoneOffset { get; set; }

        public List<TradeResult> Trades { get; set; }
    }
}