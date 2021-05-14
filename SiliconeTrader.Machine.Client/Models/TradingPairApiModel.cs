using SiliconeTrader.Core;
using System.Collections.Generic;

namespace SiliconeTrader.Machine.Client.Models
{
    public class TradingPairApiModel
    {
        public string Age { get; set; }

        public string Amount { get; set; }

        public string BoughtPrice { get; set; }

        public string BoughtRating { get; set; }

        public IPairConfig Config { get; set; }

        public string Cost { get; set; }

        public string CurrentCost { get; set; }

        public string CurrentPrice { get; set; }

        public string CurrentRating { get; set; }

        public string CurrentSpread { get; set; }

        public int DCA { get; set; }

        public bool IsTrailingBuy { get; set; }

        public bool IsTrailingSell { get; set; }

        public string LastBuyMargin { get; set; }

        public string Margin { get; set; }

        public string Name { get; set; }

        public IEnumerable<string> OrderDates { get; set; }

        public List<string> OrderIds { get; set; }

        public string SignalRule { get; set; }

        public string SwapPair { get; set; }

        public string Target { get; set; }

        public IEnumerable<string> TradingRules { get; set; }

        public string TradingViewName { get; set; }
    }
}