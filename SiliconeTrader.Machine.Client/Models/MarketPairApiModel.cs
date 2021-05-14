using SiliconeTrader.Core;
using System.Collections.Generic;

namespace SiliconeTrader.Machine.Client.Models
{
    public class ArbitrageInfo
    {
        public string Arbitrage { get; set; }

        public string Name { get; set; }
    }

    public class MarketPairApiModel
    {
        public IEnumerable<ArbitrageInfo> ArbitrageList { get; set; }

        public IPairConfig Config { get; set; }

        public bool HasTradingPair { get; set; }

        public string Name { get; set; }

        public string Price { get; set; }

        public IEnumerable<(string Name, double? RatingChange)> RatingChangeList { get; set; }

        public IEnumerable<(string Name, double? Rating)> RatingList { get; set; }

        public IEnumerable<string> SignalRules { get; set; }

        public string Spread { get; set; }

        public string TradingViewName { get; set; }

        public IEnumerable<(string Name, double? Volatility)> VolatilityList { get; set; }

        public IEnumerable<(string Name, double? VolumeChange)> VolumeChangeList { get; set; }

        public IEnumerable<(string Name, long? Volume)> VolumeList { get; set; }

        public IEnumerable<(string Name, decimal? PriceChange)> PriceChangeList { get; set; }
    }
}