using System.Collections.Generic;

namespace SiliconeTrader.Machine.Client.Models
{
    public class TradingPairResponse : BotResponse
    {
        public IEnumerable<TradingPairApiModel> TradingPairs { get; set; }
    }
}