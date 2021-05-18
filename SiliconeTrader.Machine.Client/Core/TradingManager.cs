using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using SiliconeTrader.Machine.Client.Core.Abstractions;
using SiliconeTrader.Machine.Client.Models;

namespace SiliconeTrader.Machine.Client.Core
{
    internal class TradingManager : BaseManager, ITradingManager
    {
        private TradingManager(IRestClient restClient, IModelConverter modelConverter)
            : base(restClient, modelConverter)
        {
        }

        public static ITradingManager Create(IRestClient restClient, IModelConverter modelConverter)
        {
            return new TradingManager(restClient, modelConverter);
        }

        public Task<TradesViewModel> GetTrades(DateTimeOffset id, CancellationToken cancellationToken)
            => this.SendAsync<TradesViewModel>(HttpMethod.Get, $"/api/ORCA/v1/trades/{id:o}", cancellationToken);

        public Task<TradingPairResponse> GetTradingPairs(CancellationToken cancellationToken)
            => this.SendAsync<TradingPairResponse>(HttpMethod.Get, "/api/ORCA/v1/trading-pairs", cancellationToken);
    }
}