using System.Collections.Generic;

namespace SiliconeTrader.Machine.Client.Models
{
    public class MarketPairsRequest : BotRequest
    {
        public List<string> SignalsFilter { get; set; }
    }
}