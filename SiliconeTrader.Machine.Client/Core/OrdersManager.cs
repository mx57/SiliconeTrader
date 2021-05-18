using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using SiliconeTrader.Machine.Client.Core.Abstractions;
using SiliconeTrader.Machine.Client.Models;

namespace SiliconeTrader.Machine.Client.Core
{
    internal class OrdersManager : BaseManager, IOrdersManager
    {
        private OrdersManager(IRestClient restClient, IModelConverter modelConverter)
                : base(restClient, modelConverter)
        {
        }

        public static IOrdersManager Create(IRestClient restClient, IModelConverter modelConverter)
        {
            return new OrdersManager(restClient, modelConverter);
        }

        public Task Buy(BuyRequest buyRequest, CancellationToken cancellationToken)
            => this.SendAsync<BuyRequest, EmptyResponse>(buyRequest, HttpMethod.Post, "/api/ORCA/v1/buy", cancellationToken);

        public Task BuyDefault(BuyRequest buyRequest, CancellationToken cancellationToken)
            => this.SendAsync<BuyRequest, EmptyResponse>(buyRequest, HttpMethod.Post, "/api/ORCA/v1/buy-default", cancellationToken);

        public Task Sell(SellRequest sellRequest, CancellationToken cancellationToken)
            => this.SendAsync<SellRequest, EmptyResponse>(sellRequest, HttpMethod.Post, "/api/ORCA/v1/sell", cancellationToken);

        public Task Swap(SwapRequest swapRequest, CancellationToken cancellationToken)
            => this.SendAsync<SwapRequest, EmptyResponse>(swapRequest, HttpMethod.Post, "/api/ORCA/v1/swap", cancellationToken);
    }
}