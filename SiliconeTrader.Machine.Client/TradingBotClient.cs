using SiliconeTrader.Machine.Client.Core;
using SiliconeTrader.Machine.Client.Core.Abstractions;

namespace SiliconeTrader.Machine.Client
{
    public interface ITradingBotClient
    {
        IAccountManager Account { get; }

        IBacktestingManager Backtesting { get; }

        IHealthManager Health { get; }

        IInstanceManager Instance { get; }

        IMarketManager Markets { get; }

        INotificationManager Notification { get; }

        ITradingManager Trading { get; }

        IOrdersManager Orders { get; }
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
            this.Orders = OrdersManager.Create(restClient, modelConverter);
        }

        public IAccountManager Account { get; }

        public IBacktestingManager Backtesting { get; }

        public IHealthManager Health { get; }

        public IInstanceManager Instance { get; }

        public IMarketManager Markets { get; }

        public INotificationManager Notification { get; }

        public ITradingManager Trading { get; }

        public IOrdersManager Orders { get; } 
    }
}