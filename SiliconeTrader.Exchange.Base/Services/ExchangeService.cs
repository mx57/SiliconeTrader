using ExchangeSharp;
using SiliconeTrader.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SiliconeTrader.Exchange.Base
{
    public abstract class ExchangeService : ConfigrableServiceBase<ExchangeConfig>, IExchangeService
    {
        public const int SOCKET_DISPOSE_TIMEOUT_MILLISECONDS = 10000;
        public const int MAX_TICKERS_AGE_TO_RECONNECT_MILLISECONDS = 60000;
        public const int INITIAL_TICKERS_TIMEOUT_MILLISECONDS = 5000;
        public const int INITIAL_TICKERS_RETRY_LIMIT = 4;

        public override string ServiceName => Constants.ServiceNames.ExchangeService;

        protected readonly ILoggingService loggingService;
        protected readonly IHealthCheckService healthCheckService;
        protected readonly ITasksService tasksService;

        public ExchangeAPI Api { get; set; }
        public ConcurrentDictionary<string, Ticker> Tickers { get; private set; }

        private IDisposable socket;
        private ConcurrentBag<string> markets;
        private TickersMonitorTimedTask tickersMonitorTimedTask;
        private DateTimeOffset lastTickersUpdate;
        private bool tickersStarted;

        public ExchangeService(ILoggingService loggingService, IHealthCheckService healthCheckService, ITasksService tasksService)
        {
            this.loggingService = loggingService;
            this.healthCheckService = healthCheckService;
            this.tasksService = tasksService;
        }

        public virtual void Start(bool virtualTrading)
        {
            loggingService.Info("Start Exchange service...");
            this.Api = this.InitializeApi();

            if (!virtualTrading && !String.IsNullOrWhiteSpace(this.Config.KeysPath))
            {
                if (File.Exists(this.Config.KeysPath))
                {
                    loggingService.Info("Load keys from encrypted file...");
                    this.Api.LoadAPIKeys(this.Config.KeysPath);
                }
                else
                {
                    throw new FileNotFoundException("Keys file not found");
                }
            }

            loggingService.Info("Get initial ticker values...");
            IEnumerable<KeyValuePair<string, ExchangeTicker>> exchangeTickers = null;
            for (int retry = 0; retry < INITIAL_TICKERS_RETRY_LIMIT; retry++)
            {
                Task.Run(() => exchangeTickers = this.Api.GetTickersAsync().Result).Wait(TimeSpan.FromMilliseconds(INITIAL_TICKERS_TIMEOUT_MILLISECONDS));
                if (exchangeTickers != null) break;
            }
            if (exchangeTickers != null)
            {
                this.Tickers = new ConcurrentDictionary<string, Ticker>(exchangeTickers.Select(t => new KeyValuePair<string, Ticker>(t.Key, new Ticker
                {
                    Pair = t.Key,
                    AskPrice = t.Value.Ask,
                    BidPrice = t.Value.Bid,
                    LastPrice = t.Value.Last
                })));
                markets = new ConcurrentBag<string>(this.Tickers.Keys.Select(pair => this.GetPairMarket(pair)).Distinct().ToList());

                lastTickersUpdate = DateTimeOffset.Now;
                healthCheckService.UpdateHealthCheck(Constants.HealthChecks.TickersUpdated, $"Updates: {this.Tickers.Count}");
            }
            else if (this.Tickers != null)
            {
                loggingService.Error("Unable to get initial ticker values");
            }
            else
            {
                throw new Exception("Unable to get initial ticker values");
            }

            //this.ConnectTickersWebsocket(); // Temporarily commented out due to async changes needed

            loggingService.Info("Exchange service started");
        }

        public virtual void Stop()
        {
            loggingService.Info("Stop Exchange service...");

            //this.DisconnectTickersWebsocket(); // Temporarily commented out
            lastTickersUpdate = DateTimeOffset.MinValue;
            healthCheckService.RemoveHealthCheck(Constants.HealthChecks.TickersUpdated);

            loggingService.Info("Exchange service stopped");
        }

        protected abstract ExchangeAPI InitializeApi();

        public abstract IOrderDetails PlaceOrder(IOrder order);

        public virtual decimal ClampOrderAmount(string pair, decimal amount)
        {
            // DIAGNOSTIC: Using dummy market data
            // IEnumerable<ExchangeMarket> allMarkets = this.Api.GetMarketSymbolsAsync().Result;
            // ExchangeMarket market = allMarkets?.FirstOrDefault(m => m.MarketSymbol == pair);
            ExchangeMarket market = new ExchangeMarket { MarketSymbol = pair, MinTradeSize = 0.00000001m, MaxTradeSize = 10000000m, QuantityStepSize = 0.00000001m, PriceStepSize = 0.00000001m }; // Base/Quote currency not strictly needed for ClampDecimal
            return market == null ? amount : CryptoUtility.ClampDecimal(market.MinTradeSize ?? 0m, market.MaxTradeSize ?? decimal.MaxValue, market.QuantityStepSize ?? 0.00000001m, amount);
        }

        public virtual decimal ClampOrderPrice(string pair, decimal price)
        {
            // DIAGNOSTIC: Using dummy market data
            // IEnumerable<ExchangeMarket> allMarkets = this.Api.GetMarketSymbolsAsync().Result;
            // ExchangeMarket market = allMarkets?.FirstOrDefault(m => m.MarketSymbol == pair);
            ExchangeMarket market = new ExchangeMarket { MarketSymbol = pair, MinPrice = 0.00000001m, MaxPrice = 100000000m, PriceStepSize = 0.00000001m, QuantityStepSize = 0.00000001m }; // Base/Quote currency not strictly needed for ClampDecimal
            return market == null ? price : CryptoUtility.ClampDecimal(market.MinPrice ?? 0m, market.MaxPrice ?? decimal.MaxValue, market.PriceStepSize ?? 0.00000001m, price);
        }

        public void ConnectTickersWebsocket()
        {
            // Temporarily commented out due to async changes needed for GetTickersWebSocketAsync
            /*
            try
            {
                loggingService.Info("Connect to Exchange tickers...");
                // socket = await this.Api.GetTickersWebSocketAsync(this.OnTickersUpdated); // Needs async signature
                loggingService.Info("Connected to Exchange tickers");

                tickersMonitorTimedTask = tasksService.AddTask(
                    name: nameof(TickersMonitorTimedTask),
                    task: new TickersMonitorTimedTask(loggingService, this),
                    interval: MAX_TICKERS_AGE_TO_RECONNECT_MILLISECONDS / 2,
                    startDelay: Constants.TaskDelays.ZeroDelay,
                    startTask: tickersStarted,
                    runNow: false,
                    skipIteration: 0);
            }
            catch (Exception ex)
            {
                loggingService.Error("Unable to connect to Exchange tickers", ex);
            }
            */
        }

        public void DisconnectTickersWebsocket()
        {
            // Temporarily commented out
            /*
            try
            {
                tasksService.RemoveTask(nameof(TickersMonitorTimedTask), stopTask: true);

                loggingService.Info("Disconnect from Exchange tickers...");
                // Give Dispose 10 seconds to complete and then time out if not
                // Task.Run(() => socket?.Dispose()).Wait(TimeSpan.FromMilliseconds(SOCKET_DISPOSE_TIMEOUT_MILLISECONDS)); // socket would be IWebSocket
                socket = null;
                loggingService.Info("Disconnected from Exchange tickers");
            }
            catch (Exception ex)
            {
                loggingService.Error("Unable to disconnect from Exchange tickers", ex);
            }
            */
        }

        public virtual IEnumerable<ITicker> GetTickers()
        {
            return this.Tickers.Values;
        }

        public virtual IEnumerable<string> GetMarkets()
        {
            return markets.AsEnumerable();
        }

        public virtual IEnumerable<string> GetMarketPairs(string market)
        {
            return this.Tickers.Keys.Where(t => t.EndsWith(market));
        }

        public virtual Dictionary<string, decimal> GetAvailableAmounts()
        {
            return this.Api.GetAmountsAvailableToTradeAsync().Result;
        }

        public abstract IEnumerable<IOrderDetails> GetTrades(string pair);

        public virtual decimal GetPrice(string pair, TradePriceType priceType)
        {
            if (this.Tickers.TryGetValue(pair, out Ticker ticker))
            {
                if (priceType == TradePriceType.Ask)
                {
                    return ticker.AskPrice;
                }
                else if (priceType == TradePriceType.Bid)
                {
                    return ticker.BidPrice;
                }
                else
                {
                    return ticker.LastPrice;
                }
            }
            else
            {
                return 0;
            }
        }

        public virtual decimal GetPriceSpread(string pair)
        {
            if (this.Tickers.TryGetValue(pair, out Ticker ticker))
            {
                return Utils.CalculatePercentage(ticker.BidPrice, ticker.AskPrice);
            }
            else
            {
                return 0;
            }
        }

        public abstract Arbitrage GetArbitrage(string pair, string tradingMarket, List<ArbitrageMarket> arbitrageMarkets = null, ArbitrageType? arbitrageType = null);

        public abstract string GetArbitrageMarketPair(ArbitrageMarket arbitrageMarket);

        public virtual string GetPairMarket(string pair)
        {
            // Using NormalizeMarketSymbol as per new research
            return this.Api.NormalizeMarketSymbol(pair).Split('-')[0];
        }

        public virtual string ChangeMarket(string pair, string market)
        {
            if (!pair.StartsWith(market) && !pair.EndsWith(market))
            {
                string currentMarket = this.GetPairMarket(pair);
                return pair.Substring(0, pair.Length - currentMarket.Length) + market;
            }
            return pair;
        }

        public virtual decimal ConvertPrice(string pair, decimal price, string market, TradePriceType priceType)
        {
            string pairMarket = this.GetPairMarket(pair);
            if (pairMarket == Constants.Markets.USDT)
            {
                string marketPair = market + pairMarket;
                return price / this.GetPrice(marketPair, priceType);
            }
            else
            {
                string marketPair = pairMarket + market;
                return this.GetPrice(marketPair, priceType) * price;
            }
        }

        public TimeSpan GetTimeElapsedSinceLastTickersUpdate()
        {
            return DateTimeOffset.Now - lastTickersUpdate;
        }

        private void OnTickersUpdated(IReadOnlyCollection<KeyValuePair<string, ExchangeTicker>> updatedTickers)
        {
            if (!tickersStarted)
            {
                loggingService.Info("Ticker updates are working, good!");
                tickersStarted = true;
            }

            healthCheckService.UpdateHealthCheck(Constants.HealthChecks.TickersUpdated, $"Updates: {updatedTickers.Count}");

            lastTickersUpdate = DateTimeOffset.Now;

            foreach (KeyValuePair<string, ExchangeTicker> update in updatedTickers)
            {
                if (this.Tickers.TryGetValue(update.Key, out Ticker ticker))
                {
                    ticker.AskPrice = update.Value.Ask;
                    ticker.BidPrice = update.Value.Bid;
                    ticker.LastPrice = update.Value.Last;
                }
                else
                {
                    this.Tickers.TryAdd(update.Key, new Ticker
                    {
                        Pair = update.Key,
                        AskPrice = update.Value.Ask,
                        BidPrice = update.Value.Bid,
                        LastPrice = update.Value.Last
                    });

                    string market = this.GetPairMarket(update.Key);
                    if (!markets.Contains(market))
                    {
                        markets.Add(market);
                    }
                }
            }
        }
    }
}
