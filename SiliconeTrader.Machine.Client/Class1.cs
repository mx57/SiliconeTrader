namespace SiliconeTrader.Machine.Client
{
    public interface ITradingBotClient
    {
        IBacktestingManager Backtesting { get; }

        IHealthManager Health { get; }

        INotificationManager Notification { get; }

        ITradingManager Trading { get; }
    }

    public class TradingBotClient : ITradingBotClient
    {
        public TradingBotClient(IRestClient restClient, IModelConverter modelConverter)
        {
            this.Health = HealthManager.Create(restClient, modelConverter);
        }

        public IBacktestingManager Backtesting { get; }

        public IHealthManager Health { get; }

        public INotificationManager Notification { get; }

        public ITradingManager Trading { get; }
    }
}