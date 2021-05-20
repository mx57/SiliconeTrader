using System.Collections.Generic;

namespace SiliconeTrader.Machine.Client.Models
{
    public class MarketPairsResponse : BotResponse
    {
        public IEnumerable<MarketPairApiModel> MarketPairs { get; set; }
    }
}