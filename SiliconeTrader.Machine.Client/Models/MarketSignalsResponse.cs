using System.Collections.Generic;
using SiliconeTrader.Core.Models; // Added for BotResponse

namespace SiliconeTrader.Machine.Client.Models
{
    public class MarketSignalsResponse : BotResponse
    {
        public IEnumerable<string> Signals { get; set; }
    }
}