using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SiliconeTrader.Core
{
    public interface IOrderingService
    {
        Task<IOrderDetails> PlaceBuyOrder(BuyOptions options);
        Task<IOrderDetails> PlaceSellOrder(SellOptions options);
    }
}
