using System.Collections.Generic; // For List and IEnumerable
using System; // For string, object, decimal, double, long, etc.

namespace SiliconeTrader.Core.Models // Updated namespace
{
    // From MarketPairsRequest.cs
    public class MarketPairsRequest : BotRequest // Added inheritance
    {
        public List<string> SignalsFilter { get; set; }
    }

    // From MarketPairsResponse.cs
    public class MarketPairsResponse : BotResponse // Added inheritance
    {
        public IEnumerable<MarketPairApiModel> MarketPairs { get; set; }
    }

    // From MarketPairApiModel.cs
    public class ArbitrageInfo
    {
        public string Arbitrage { get; set; }
        public string Name { get; set; }
    }

    public class MarketPairApiModel
    {
        public IEnumerable<ArbitrageInfo> ArbitrageList { get; set; }
        public object Config { get; set; } // Consider making this a specific type if possible later
        public bool HasTradingPair { get; set; }
        public string Name { get; set; }
        public string Price { get; set; }
        public IEnumerable<NameValue<decimal?>> PriceChangeList { get; set; }
        public IEnumerable<NameValue<double?>> RatingChangeList { get; set; }
        public IEnumerable<NameValue<double?>> RatingList { get; set; }
        public IEnumerable<string> SignalRules { get; set; }
        public string Spread { get; set; }
        public string TradingViewName { get; set; }
        public IEnumerable<NameValue<double?>> VolatilityList { get; set; }
        public IEnumerable<NameValue<double?>> VolumeChangeList { get; set; }
        public IEnumerable<NameValue<long?>> VolumeList { get; set; }
    }

    public class NameValue<T>
    {
        public NameValue(string name, T value)
        {
            this.Name = name;
            this.Value = value;
        }
        public string Name { get; set; }
        public T Value { get; set; }
    }
}
