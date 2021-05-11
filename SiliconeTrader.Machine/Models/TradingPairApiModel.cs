using SiliconeTrader.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SiliconeTrader.Machine.Models
{
    public class TradingPairApiModel
    {
        public string Name { get; internal set; }
        public int DCA { get; internal set; }
        public string TradingViewName { get; internal set; }
        public string Margin { get; internal set; }
        public string Target { get; internal set; }
        public string CurrentPrice { get; internal set; }
        public string CurrentSpread { get; internal set; }
        public string BoughtPrice { get; internal set; }
        public string Cost { get; internal set; }
        public string CurrentCost { get; internal set; }
        public string Amount { get; internal set; }
        public IEnumerable<string> OrderDates { get; internal set; }
        public List<string> OrderIds { get; internal set; }
        public string Age { get; internal set; }
        public string CurrentRating { get; internal set; }
        public string BoughtRating { get; internal set; }
        public string SignalRule { get; internal set; }
        public string SwapPair { get; internal set; }
        public IEnumerable<string> TradingRules { get; internal set; }
        public bool IsTrailingSell { get; internal set; }
        public bool IsTrailingBuy { get; internal set; }
        public string LastBuyMargin { get; internal set; }
        public IPairConfig Config { get; internal set; }
    }
}
