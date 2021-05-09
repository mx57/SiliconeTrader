using System;
using System.Collections.Generic;
using System.Text;

namespace SiliconeTrader.Core
{
    public interface IOrderingService
    {
        IOrderDetails PlaceBuyOrder(BuyOptions options);
        IOrderDetails PlaceSellOrder(SellOptions options);
    }
}
