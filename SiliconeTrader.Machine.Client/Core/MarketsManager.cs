using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using SiliconeTrader.Machine.Client.Core.Abstractions;
using SiliconeTrader.Machine.Client.Models;

namespace SiliconeTrader.Machine.Client.Core
{
    internal class MarketsManager : BaseManager, IMarketManager
    {
        private MarketsManager(IRestClient restClient, IModelConverter modelConverter)
            : base(restClient, modelConverter)
        {
        }

        public static IMarketManager Create(IRestClient restClient, IModelConverter modelConverter)
        {
            return new MarketsManager(restClient, modelConverter);
        }

        public Task<SiliconeTrader.Core.Models.MarketPairsResponse> GetMarketPairs(SiliconeTrader.Core.Models.MarketPairsRequest request, CancellationToken cancellationToken)
            => this.SendAsync<SiliconeTrader.Core.Models.MarketPairsRequest, SiliconeTrader.Core.Models.MarketPairsResponse>(request, HttpMethod.Post, "/api/ORCA/v1/market-pairs", cancellationToken);

        public Task<MarketSignalsResponse> GetMarketSignals(CancellationToken cancellationToken)
            => this.SendAsync<MarketSignalsResponse>(HttpMethod.Get, "/api/ORCA/v1/market-signals", cancellationToken);
    }
}