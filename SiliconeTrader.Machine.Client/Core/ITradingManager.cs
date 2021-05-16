using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using SiliconeTrader.Machine.Client.Models;

namespace SiliconeTrader.Machine.Client.Core
{
    public interface ITradingManager
    {
        Task<TradingPairResponse> GetTradingPairs(CancellationToken cancellationToken);
    }


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

        public Task<TradingPairResponse> GetTradingPairs(CancellationToken cancellationToken)
            => this.SendAsync<TradingPairResponse>(HttpMethod.Get, "/api/ORCA/v1/trading-pairs", cancellationToken);
    }

    public class TradingPairResponse : BotResponse
    {
        public IEnumerable<TradingPairApiModel> TradingPairs { get; set; }
    }
}