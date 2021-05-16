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

        public object Config { get; set; }

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