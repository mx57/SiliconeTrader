using System;
using System.Collections.Generic;
using System.Text;
using SiliconeTrader.Core;

namespace SiliconeTrader.Exchange.Base
{
    public class BuyOrder : Order
    {
        public override OrderSide Side => OrderSide.Buy;
    }
}
