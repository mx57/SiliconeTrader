using SiliconeTrader.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SiliconeTrader.Machine.Models
{
    public class ArbitrageInfo
    {
        public string Name { get; set; }
        public string Arbitrage { get; set; }
    }
    public class MarketPairApiModel
    {
        public string Name { get; internal set; }
        public string TradingViewName { get; internal set; }
        public string Price { get; internal set; }
        public string Spread { get; internal set; }
        public IEnumerable<(string Name, long? Volume)> VolumeList { get; internal set; }
        public IEnumerable<(string Name, double? VolumeChange)> VolumeChangeList { get; internal set; }
        public IEnumerable<(string Name, double? Rating)> RatingList { get; internal set; }
        public IEnumerable<(string Name, double? RatingChange)> RatingChangeList { get; internal set; }
        public IEnumerable<(string Name, double? Volatility)> VolatilityList { get; internal set; }
        public IEnumerable<ArbitrageInfo> ArbitrageList { get; internal set; }
        public IEnumerable<string> SignalRules { get; internal set; }
        public bool HasTradingPair { get; internal set; }
        public IPairConfig Config { get; internal set; }
        internal IEnumerable<(string Name, decimal? PriceChange)> PriceChangeList { get; set; }
    }
}
