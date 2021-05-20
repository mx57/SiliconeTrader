using System.Collections.Generic;

namespace SiliconeTrader.Machine.Client.Models
{
    public class MarketSignalsResponse : BotResponse
    {
        public IEnumerable<string> Signals { get; set; }
    }
}