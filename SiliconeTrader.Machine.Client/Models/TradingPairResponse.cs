using System.Collections.Generic;
using SiliconeTrader.Core.Models; // Added for BotResponse

namespace SiliconeTrader.Machine.Client.Models
{
    public class TradingPairResponse : BotResponse
    {
        public IEnumerable<TradingPairApiModel> TradingPairs { get; set; }
    }
}