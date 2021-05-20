using SiliconeTrader.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace SiliconeTrader.Trading
{
    internal class SellTrailingInfo : TrailingInfo
    {
        public SellOptions SellOptions { get; set; }
        public SellTrailingStopAction TrailingStopAction { get; set; }
        public decimal SellMargin { get; set; }
    }
}
