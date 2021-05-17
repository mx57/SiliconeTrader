using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using SiliconeTrader.Machine.Client.Core;
using SiliconeTrader.Machine.Client.Models;

namespace SiliconeTrader.Machine.Client
{
    public interface ITradingBotClient
    {
        public IInstanceManager Instance { get; }

        IBacktestingManager Backtesting { get; }

        IHealthManager Health { get; }

        INotificationManager Notification { get; }

        ITradingManager Trading { get; }

        IMarketManager Markets { get; }
        IAccountManager Account { get; }
    }

    public interface IAccountManager
    {
        Task Refresh(CancellationToken cancellationToken);
    }

    internal class AccountManager : BaseManager, IAccountManager
    {

        private AccountManager(IRestClient restClient, IModelConverter modelConverter)
            : base(restClient, modelConverter)
        {
        }

        public static IAccountManager Create(IRestClient restClient, IModelConverter modelConverter)
        {
            return new AccountManager(restClient, modelConverter);
        }

        public Task Refresh(CancellationToken cancellationToken)
            => this.SendAsync<EmptyResponse>(HttpMethod.Post, "/api/ORCA/v1/account/refresh", cancellationToken);
    }

    public interface IMarketManager
    {
        Task<MarketPairsResponse> GetMarketPairs(MarketPairsRequest request, CancellationToken cancellationToken);
        Task<MarketSignalsResponse> GetMarketSignals(CancellationToken cancellationToken);
    }

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

        public Task<MarketPairsResponse> GetMarketPairs(MarketPairsRequest request, CancellationToken cancellationToken)
            => this.SendAsync<MarketPairsRequest, MarketPairsResponse>(request, HttpMethod.Post, "/api/ORCA/v1/market-pairs", cancellationToken);

        public Task<MarketSignalsResponse> GetMarketSignals(CancellationToken cancellationToken)
            => this.SendAsync<MarketSignalsResponse>(HttpMethod.Get, "/api/ORCA/v1/market-signals", cancellationToken);
    }

    public class MarketPairsRequest : BotRequest
    {
        public List<string> SignalsFilter { get; set; }
    }

    public class MarketSignalsResponse : BotResponse
    {
        public IEnumerable<string> Signals { get; set; }
    }

    public class MarketPairsResponse : BotResponse
    {
        public IEnumerable<MarketPairApiModel> MarketPairs { get; set; }
    }

    public class TradingBotClient : ITradingBotClient
    {
        public TradingBotClient(IRestClient restClient, IModelConverter modelConverter)
        {
            this.Health = HealthManager.Create(restClient, modelConverter);
            this.Instance = InstanceManager.Create(restClient, modelConverter);
            this.Trading = TradingManager.Create(restClient, modelConverter);
            this.Markets = MarketsManager.Create(restClient, modelConverter);
            this.Account = AccountManager.Create(restClient, modelConverter);

        }

        public IBacktestingManager Backtesting { get; }

        public IHealthManager Health { get; }

        public IInstanceManager Instance { get; }

        public INotificationManager Notification { get; }

        public ITradingManager Trading { get; }

        public IMarketManager Markets { get; }
        public IAccountManager Account { get; }
    }
}