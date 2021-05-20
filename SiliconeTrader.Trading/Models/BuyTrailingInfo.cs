using SiliconeTrader.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace SiliconeTrader.Trading
{
    internal class BuyTrailingInfo : TrailingInfo
    {
        public BuyOptions BuyOptions { get; set; }
        public BuyTrailingStopAction TrailingStopAction { get; set; }
    }
}
