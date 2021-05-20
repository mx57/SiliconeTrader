using System.Threading;
using System.Threading.Tasks;
using SiliconeTrader.Machine.Client.Models;

namespace SiliconeTrader.Machine.Client.Core.Abstractions
{
    public interface IOrdersManager
    {
        Task Buy(BuyRequest buyRequest, CancellationToken cancellationToken);

        Task BuyDefault(BuyRequest buyRequest, CancellationToken cancellationToken);

        Task Sell(SellRequest sellRequest, CancellationToken cancellationToken);

        Task Swap(SwapRequest swapRequest, CancellationToken cancellationToken);
    }
}