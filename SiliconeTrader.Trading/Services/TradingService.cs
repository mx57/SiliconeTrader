using SiliconeTrader.Core;
using SiliconeTrader.Exchange.Base;
using SiliconeTrader.Signals.Base;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SiliconeTrader.Trading
{
    internal class TradingService : ConfigrableServiceBase<TradingConfig>, ITradingService
    {
        private const int MIN_INTERVAL_BETWEEN_BUY_AND_SELL = 7000;
        private const decimal DEFAULT_ARBITRAGE_BUY_MULTIPLIER = 0.99M;
        private const decimal DEFAULT_ARBITRAGE_SELL_MULTIPLIER = 0.99M;

        public override string ServiceName => Constants.ServiceNames.TradingService;

        ITradingConfig ITradingService.Config => this.Config;
        public IModuleRules Rules { get; private set; }
        public TradingRulesConfig RulesConfig { get; private set; }

        public IExchangeService Exchange { get; private set; }
        public ITradingAccount Account { get; private set; }
        public ConcurrentStack<IOrderDetails> OrderHistory { get; private set; } = new ConcurrentStack<IOrderDetails>();
        public bool IsTradingSuspended { get; private set; }

        private readonly ILoggingService loggingService;
        private readonly INotificationService notificationService;
        private readonly IHealthCheckService healthCheckService;
        private readonly ITasksService tasksService;
        private IOrderingService orderingService;
        private IRulesService rulesService;
        private ISignalsService signalsService;

        private TradingTimedTask tradingTimedTask;
        private TradingRulesTimedTask tradingRulesTimedTask;
        private AccountRefreshTimedTask accountRefreshTimedTask;

        private bool tradingForcefullySuspended;
        private object syncRoot = new object();

        public TradingService(ILoggingService loggingService, INotificationService notificationService, IHealthCheckService healthCheckService, ITasksService tasksService)
        {
            this.loggingService = loggingService;
            this.notificationService = notificationService;
            this.healthCheckService = healthCheckService;
            this.tasksService = tasksService;

            bool isBacktesting = Application.Resolve<IBacktestingService>().Config.Enabled && Application.Resolve<IBacktestingService>().Config.Replay;
            if (isBacktesting)
            {
                this.Exchange = Application.ResolveOptionalNamed<IExchangeService>(Constants.ServiceNames.BacktestingExchangeService);
            }
            else
            {
                this.Exchange = Application.ResolveOptionalNamed<IExchangeService>(this.Config.Exchange);
            }

            if (this.Exchange == null)
            {
                throw new Exception($"Unsupported exchange: {this.Config.Exchange}");
            }
        }

        public void Start()
        {
            loggingService.Info($"Start Trading service (Virtual: {this.Config.VirtualTrading})...");

            this.IsTradingSuspended = true;

            orderingService = Application.Resolve<IOrderingService>();
            rulesService = Application.Resolve<IRulesService>();
            this.OnTradingRulesChanged();
            rulesService.RegisterRulesChangeCallback(this.OnTradingRulesChanged);
            this.Exchange.Start(this.Config.VirtualTrading);
            signalsService = Application.Resolve<ISignalsService>();

            if (!this.Config.VirtualTrading)
            {
                this.Account = new ExchangeAccount(loggingService, notificationService, healthCheckService, signalsService, this);
            }
            else
            {
                this.Account = new VirtualAccount(loggingService, notificationService, healthCheckService, signalsService, this);
            }

            accountRefreshTimedTask = tasksService.AddTask(
                name: nameof(AccountRefreshTimedTask),
                task: new AccountRefreshTimedTask(loggingService, healthCheckService, this),
                interval: this.Config.AccountRefreshInterval * 1000 / Application.Speed,
                startDelay: Constants.TaskDelays.ZeroDelay,
                startTask: false,
                runNow: true,
                skipIteration: 0);

            if (signalsService.Config.Enabled)
            {
                signalsService.Start();
            }

            tradingTimedTask = tasksService.AddTask(
                name: nameof(TradingTimedTask),
                task: new TradingTimedTask(loggingService, notificationService, healthCheckService, signalsService, orderingService, this),
                interval: this.Config.TradingCheckInterval * 1000 / Application.Speed,
                startDelay: Constants.TaskDelays.NormalDelay,
                startTask: false,
                runNow: false,
                skipIteration: 0);

            tradingRulesTimedTask = tasksService.AddTask(
                name: nameof(TradingRulesTimedTask),
                task: new TradingRulesTimedTask(loggingService, notificationService, healthCheckService, rulesService, signalsService, this),
                interval: this.RulesConfig.CheckInterval * 1000 / Application.Speed,
                startDelay: Constants.TaskDelays.MidDelay,
                startTask: false,
                runNow: false,
                skipIteration: 0);

            this.IsTradingSuspended = false;

            loggingService.Info("Trading service started");
        }

        public void Stop()
        {
            loggingService.Info("Stop Trading service...");

            this.Exchange.Stop();

            if (signalsService.Config.Enabled)
            {
                signalsService.Stop();
            }

            tasksService.RemoveTask(nameof(TradingTimedTask), stopTask: true);
            tasksService.RemoveTask(nameof(TradingRulesTimedTask), stopTask: true);
            tasksService.RemoveTask(nameof(AccountRefreshTimedTask), stopTask: true);

            this.Account.Dispose();

            rulesService.UnregisterRulesChangeCallback(this.OnTradingRulesChanged);

            healthCheckService.RemoveHealthCheck(Constants.HealthChecks.TradingRulesProcessed);
            healthCheckService.RemoveHealthCheck(Constants.HealthChecks.TradingPairsProcessed);

            loggingService.Info("Trading service stopped");
        }

        public void ResumeTrading(bool forced)
        {
            if (this.IsTradingSuspended && (!tradingForcefullySuspended || forced))
            {
                loggingService.Info("Trading started");
                this.IsTradingSuspended = false;

                tradingTimedTask.Start();
                tradingRulesTimedTask.Start();
                tradingRulesTimedTask.RunNow();
            }
        }

        public void SuspendTrading(bool forced)
        {
            if (!this.IsTradingSuspended)
            {
                loggingService.Info("Trading suspended");
                this.IsTradingSuspended = true;
                tradingForcefullySuspended = forced;

                tradingRulesTimedTask.Stop();
                tradingTimedTask.Stop();
                tradingTimedTask.StopTrailing();
            }
        }

        public IPairConfig GetPairConfig(string pair)
        {
            return tradingRulesTimedTask.GetPairConfig(pair);
        }

        public void ReapplyTradingRules()
        {
            tradingRulesTimedTask.RunNow();
        }

        public void Buy(BuyOptions options)
        {
            lock (syncRoot)
            {
                this.PauseTasks();
                try
                {
                    IRule rule = signalsService.Rules.Entries.FirstOrDefault(r => r.Name == options.Metadata.SignalRule);
                    RuleAction ruleAction = rule?.Action ?? RuleAction.Default;
                    IPairConfig pairConfig = this.GetPairConfig(options.Pair);

                    bool arbitragePair = pairConfig.ArbitrageEnabled && pairConfig.ArbitrageSignalRules.Contains(options.Metadata.SignalRule);
                    if (arbitragePair)
                    {
                        Arbitrage arbitrage = this.Exchange.GetArbitrage(options.Pair, this.Config.Market, pairConfig.ArbitrageMarkets, pairConfig.ArbitrageType);
                        if (arbitrage.IsAssigned)
                        {
                            this.Arbitrage(new ArbitrageOptions(options.Pair, arbitrage, options.Metadata));
                        }
                    }
                    else
                    {
                        ITradingPair swappedPair = this.Account.GetTradingPairs().OrderBy(p => p.CurrentMargin).FirstOrDefault(tradingPair =>
                        {
                            IPairConfig tradingPairConfig = this.GetPairConfig(tradingPair.Pair);
                            return tradingPairConfig.SellEnabled && tradingPairConfig.SwapEnabled && tradingPairConfig.SwapSignalRules != null &&
                                   tradingPairConfig.SwapSignalRules.Contains(options.Metadata.SignalRule) &&
                                   tradingPairConfig.SwapTimeout < (DateTimeOffset.Now - tradingPair.OrderDates.DefaultIfEmpty().Max()).TotalSeconds;
                        });

                        if (swappedPair != null)
                        {
                            this.Swap(new SwapOptions(swappedPair.Pair, options.Pair, options.Metadata));
                        }
                        else if (ruleAction == RuleAction.Default)
                        {
                            if (this.CanBuy(options, out string message))
                            {
                                tradingTimedTask.InitiateBuy(options);
                            }
                            else
                            {
                                loggingService.Debug(message);
                            }
                        }
                    }
                }
                finally
                {
                    this.ContinueTasks();
                }
            }
        }

        public void Sell(SellOptions options)
        {
            lock (syncRoot)
            {
                this.PauseTasks();
                try
                {
                    if (this.CanSell(options, out string message))
                    {
                        tradingTimedTask.InitiateSell(options);
                    }
                    else
                    {
                        loggingService.Debug(message);
                    }
                }
                finally
                {
                    this.ContinueTasks();
                }
            }
        }

        public void Swap(SwapOptions options)
        {
            lock (syncRoot)
            {
                this.PauseTasks();
                try
                {
                    if (this.CanSwap(options, out string message))
                    {
                        ITradingPair oldTradingPair = this.Account.GetTradingPair(options.OldPair);
                        var sellOptions = new SellOptions(options.OldPair)
                        {
                            Swap = true,
                            ManualOrder = options.ManualOrder,
                            Metadata = new OrderMetadata { SwapPair = options.NewPair }
                        };

                        if (this.CanSell(sellOptions, out message))
                        {
                            decimal currentMargin = oldTradingPair.CurrentMargin;
                            decimal additionalCosts = oldTradingPair.Cost - oldTradingPair.CurrentCost + (oldTradingPair.Metadata.AdditionalCosts ?? 0);
                            int additionalDCALevels = oldTradingPair.DCALevel;

                            IOrderDetails sellOrderDetails = orderingService.PlaceSellOrder(sellOptions);
                            if (!this.Account.HasTradingPair(options.OldPair))
                            {
                                var buyOptions = new BuyOptions(options.NewPair)
                                {
                                    Swap = true,
                                    ManualOrder = options.ManualOrder,
                                    MaxCost = sellOrderDetails.Cost,
                                    Metadata = options.Metadata
                                };
                                buyOptions.Metadata.LastBuyMargin = currentMargin;
                                buyOptions.Metadata.SwapPair = options.OldPair;
                                buyOptions.Metadata.AdditionalDCALevels = additionalDCALevels;
                                buyOptions.Metadata.AdditionalCosts = additionalCosts;
                                IOrderDetails buyOrderDetails = orderingService.PlaceBuyOrder(buyOptions);

                                var newTradingPair = this.Account.GetTradingPair(options.NewPair) as TradingPair;
                                if (newTradingPair != null)
                                {
                                    newTradingPair.Metadata.AdditionalCosts += this.CalculateOrderFees(sellOrderDetails);
                                    loggingService.Info($"Swap {oldTradingPair.FormattedName} for {newTradingPair.FormattedName}. " +
                                        $"Old margin: {oldTradingPair.CurrentMargin:0.00}, new margin: {newTradingPair.CurrentMargin:0.00}");
                                }
                                else
                                {
                                    loggingService.Info($"Unable to swap {options.OldPair} for {options.NewPair}. Reason: failed to buy {options.NewPair}");
                                    notificationService.Notify($"Unable to swap {options.OldPair} for {options.NewPair}: Failed to buy {options.NewPair}");
                                }
                            }
                            else
                            {
                                loggingService.Info($"Unable to swap {options.OldPair} for {options.NewPair}. Reason: failed to sell {options.OldPair}");
                            }
                        }
                        else
                        {
                            loggingService.Info($"Unable to swap {options.OldPair} for {options.NewPair}: {message}");
                        }
                    }
                    else
                    {
                        loggingService.Info(message);
                    }
                }
                finally
                {
                    this.ContinueTasks();
                }
            }
        }

        public void Arbitrage(ArbitrageOptions options)
        {
            lock (syncRoot)
            {
                this.PauseTasks();
                try
                {
                    if (this.CanArbitrage(options, out string message))
                    {
                        if (this.CanBuy(new BuyOptions(options.Pair) { Amount = 1 }, out message))
                        {
                            options.Metadata.Arbitrage = $"{options.Arbitrage.Market}-" + options.Arbitrage.Type ?? "All";
                            options.Metadata.ArbitragePercentage = options.Arbitrage.Percentage;
                            loggingService.Info($"{options.Arbitrage.Type} arbitrage {options.Pair} on {options.Arbitrage.Market}. Percentage: {options.Arbitrage.Percentage:0.00}");

                            if (options.Arbitrage.Type == ArbitrageType.Direct)
                            {
                                this.ArbitrageDirect(options);
                            }
                            else if (options.Arbitrage.Type == ArbitrageType.Reverse)
                            {
                                this.ArbitrageReverse(options);
                            }
                        }
                        else
                        {
                            loggingService.Info($"Unable to arbitrage {options.Pair}: {message}");
                        }
                    }
                    else
                    {
                        loggingService.Info(message);
                    }
                }
                finally
                {
                    this.ContinueTasks();
                }
            }
        }

        private void ArbitrageDirect(ArbitrageOptions options)
        {
            string arbitragePair = options.Pair;
            ITradingPair existingArbitragePair = this.Account.GetTradingPair(arbitragePair);
            IPairConfig pairConfig = this.GetPairConfig(options.Pair);
            bool useExistingArbitragePair = (existingArbitragePair != null && existingArbitragePair.CurrentCost > pairConfig.BuyMaxCost &&
                                            existingArbitragePair.AveragePrice <= existingArbitragePair.CurrentPrice);

            var buyArbitragePairOptions = new BuyOptions(arbitragePair)
            {
                Arbitrage = true,
                MaxCost = pairConfig.BuyMaxCost,
                ManualOrder = options.ManualOrder,
                IgnoreBalance = useExistingArbitragePair,
                Metadata = options.Metadata
            };

            if (this.CanBuy(buyArbitragePairOptions, out string message))
            {
                IOrderDetails buyArbitragePairOrderDetails = null;
                if (useExistingArbitragePair)
                {
                    buyArbitragePairOrderDetails = this.Account.AddBlankOrder(buyArbitragePairOptions.Pair,
                        buyArbitragePairOptions.MaxCost.Value / this.GetPrice(buyArbitragePairOptions.Pair, TradePriceType.Ask),
                        includeFees: false);
                    loggingService.Info($"Use existing arbitrage pair for arbitrage: {arbitragePair}. " +
                        $"Average price: {existingArbitragePair.AveragePrice}, Current price: {existingArbitragePair.CurrentPrice}");
                }
                else
                {
                    buyArbitragePairOrderDetails = orderingService.PlaceBuyOrder(buyArbitragePairOptions);
                }

                if (buyArbitragePairOrderDetails.Result == OrderResult.Filled)
                {
                    decimal buyArbitragePairFees = this.CalculateOrderFees(buyArbitragePairOrderDetails);
                    string flippedArbitragePair = this.Exchange.ChangeMarket(arbitragePair, options.Arbitrage.Market.ToString());
                    var sellArbitragePairOptions = new SellOptions(flippedArbitragePair)
                    {
                        Arbitrage = true,
                        Amount = buyArbitragePairOrderDetails.AmountFilled,
                        ManualOrder = options.ManualOrder,
                        Metadata = options.Metadata.MergeWith(new OrderMetadata
                        {
                            IsTransitional = true
                        })
                    };

                    IOrderDetails sellArbitragePairOrderDetails = orderingService.PlaceSellOrder(sellArbitragePairOptions);
                    if (sellArbitragePairOrderDetails.Result == OrderResult.Filled)
                    {
                        decimal sellArbitragePairMultiplier = pairConfig.ArbitrageSellMultiplier ?? DEFAULT_ARBITRAGE_SELL_MULTIPLIER;
                        decimal sellArbitragePairFees = this.CalculateOrderFees(sellArbitragePairOrderDetails);
                        options.Metadata.FeesNonDeductible = buyArbitragePairFees  * sellArbitragePairMultiplier;
                        decimal sellMarketPairAmount = sellArbitragePairOrderDetails.AmountFilled * this.GetPrice(flippedArbitragePair, TradePriceType.Bid, normalize: false) * sellArbitragePairMultiplier;
                        string marketPair = this.Exchange.GetArbitrageMarketPair(options.Arbitrage.Market);

                        var sellMarketPairOptions = new SellOptions(marketPair)
                        {
                            Arbitrage = true,
                            Amount = sellMarketPairAmount,
                            ManualOrder = options.ManualOrder,
                            Metadata = options.Metadata.MergeWith(new OrderMetadata
                            {
                                IsTransitional = false,
                                OriginalPair = arbitragePair
                            })
                        };

                        existingArbitragePair = this.Account.GetTradingPair(marketPair);
                        existingArbitragePair.OverrideCost((buyArbitragePairOrderDetails.Cost + sellArbitragePairFees * 2) * sellArbitragePairMultiplier);
                        IOrderDetails sellMarketPairOrderDetails = orderingService.PlaceSellOrder(sellMarketPairOptions);
                        existingArbitragePair.OverrideCost(null);

                        if (sellMarketPairOrderDetails.Result == OrderResult.Filled)
                        {
                            loggingService.Info($"{pairConfig.ArbitrageType} arbitrage successful: {arbitragePair} -> {flippedArbitragePair} -> {marketPair}");
                        }
                        else
                        {
                            loggingService.Info($"Unable to arbitrage {options.Pair}. Reason: failed to sell market pair {arbitragePair}");
                            notificationService.Notify($"Unable to arbitrage {options.Pair}: Failed to sell market pair {arbitragePair}");
                        }
                    }
                    else
                    {
                        loggingService.Info($"Unable to arbitrage {options.Pair}. Reason: failed to sell arbitrage pair {flippedArbitragePair}");
                        notificationService.Notify($"Unable to arbitrage {options.Pair}: Failed to sell arbitrage pair {flippedArbitragePair}");
                    }
                }
                else
                {
                    loggingService.Info($"Unable to arbitrage {options.Pair}. Reason: failed to buy arbitrage pair {arbitragePair}");
                }
            }
            else
            {
                loggingService.Info($"Unable to arbitrage {options.Pair}: {message}");
            }
        }

        private void ArbitrageReverse(ArbitrageOptions options)
        {
            string marketPair = this.Exchange.GetArbitrageMarketPair(options.Arbitrage.Market);
            ITradingPair existingMarketPair = this.Account.GetTradingPair(marketPair);
            IPairConfig pairConfig = this.GetPairConfig(options.Pair);
            bool useExistingMarketPair = (existingMarketPair != null && existingMarketPair.CurrentCost > pairConfig.BuyMaxCost &&
                                         existingMarketPair.AveragePrice <= existingMarketPair.CurrentPrice);

            var buyMarketPairOptions = new BuyOptions(marketPair)
            {
                Arbitrage = true,
                MaxCost = pairConfig.BuyMaxCost,
                ManualOrder = options.ManualOrder,
                IgnoreBalance = useExistingMarketPair,
                Metadata = options.Metadata
            };

            if (this.CanBuy(buyMarketPairOptions, out string message))
            {
                IOrderDetails buyMarketPairOrderDetails = null;
                if (useExistingMarketPair)
                {
                    buyMarketPairOrderDetails = this.Account.AddBlankOrder(buyMarketPairOptions.Pair,
                        buyMarketPairOptions.MaxCost.Value / this.GetPrice(buyMarketPairOptions.Pair, TradePriceType.Ask),
                        includeFees: false);
                    loggingService.Info($"Use existing market pair for arbitrage: {marketPair}. " +
                        $"Average price: {existingMarketPair.AveragePrice}, Current price: {existingMarketPair.CurrentPrice}");
                }
                else
                {
                    buyMarketPairOrderDetails = orderingService.PlaceBuyOrder(buyMarketPairOptions);
                }

                if (buyMarketPairOrderDetails.Result == OrderResult.Filled)
                {
                    decimal buyArbitragePairMultiplier = pairConfig.ArbitrageBuyMultiplier ?? DEFAULT_ARBITRAGE_BUY_MULTIPLIER;
                    decimal buyMarketPairFees = this.CalculateOrderFees(buyMarketPairOrderDetails);
                    string arbitragePair = this.Exchange.ChangeMarket(options.Pair, options.Arbitrage.Market.ToString());
                    decimal buyArbitragePairAmount = options.Arbitrage.Market == ArbitrageMarket.USDT ?
                        buyMarketPairOrderDetails.AmountFilled * this.GetPrice(buyMarketPairOrderDetails.Pair, TradePriceType.Ask, normalize: false) / this.GetPrice(arbitragePair, TradePriceType.Ask) :
                        buyMarketPairOrderDetails.AmountFilled / this.GetPrice(arbitragePair, TradePriceType.Ask);

                    var buyArbitragePairOptions = new BuyOptions(arbitragePair)
                    {
                        Arbitrage = true,
                        ManualOrder = options.ManualOrder,
                        Amount = buyArbitragePairAmount * buyArbitragePairMultiplier,
                        Metadata = options.Metadata
                    };

                    IOrderDetails buyArbitragePairOrderDetails = orderingService.PlaceBuyOrder(buyArbitragePairOptions);
                    if (buyArbitragePairOrderDetails.Result == OrderResult.Filled)
                    {
                        decimal buyArbitragePairFees = this.CalculateOrderFees(buyArbitragePairOrderDetails);
                        options.Metadata.FeesNonDeductible = buyMarketPairFees * buyArbitragePairMultiplier;
                        var sellArbitragePairOptions = new SellOptions(buyArbitragePairOrderDetails.Pair)
                        {
                            Arbitrage = true,
                            Amount = buyArbitragePairOrderDetails.AmountFilled,
                            ManualOrder = options.ManualOrder,
                            Metadata = options.Metadata
                        };

                        TradingPair existingArbitragePair = this.Account.GetTradingPair(buyArbitragePairOrderDetails.Pair) as TradingPair;
                        existingArbitragePair.OverrideCost(buyArbitragePairOrderDetails.Cost + buyArbitragePairFees * 2);
                        IOrderDetails sellArbitragePairOrderDetails = orderingService.PlaceSellOrder(sellArbitragePairOptions);
                        existingArbitragePair.OverrideCost(null);

                        if (sellArbitragePairOrderDetails.Result == OrderResult.Filled)
                        {
                            loggingService.Info($"{pairConfig.ArbitrageType} arbitrage successful: {marketPair} -> {arbitragePair} -> {existingArbitragePair.Pair}");
                        }
                        else
                        {
                            loggingService.Info($"Unable to arbitrage {options.Pair}. Reason: failed to sell arbitrage pair {arbitragePair}");
                            notificationService.Notify($"Unable to arbitrage {options.Pair}: Failed to sell arbitrage pair {arbitragePair}");
                        }
                    }
                    else
                    {
                        loggingService.Info($"Unable to arbitrage {options.Pair}. Reason: failed to buy arbitrage pair {arbitragePair}");
                        notificationService.Notify($"Unable to arbitrage {options.Pair}: Failed to buy arbitrage pair {arbitragePair}");
                    }
                }
                else
                {
                    loggingService.Info($"Unable to arbitrage {options.Pair}. Reason: failed to buy market pair {marketPair}");
                }
            }
            else
            {
                loggingService.Info($"Unable to arbitrage {options.Pair}: {message}");
            }
        }

        public bool CanBuy(BuyOptions options, out string message)
        {
            IPairConfig pairConfig = this.GetPairConfig(options.Pair);

            if (!options.ManualOrder && !options.Swap && this.IsTradingSuspended)
            {
                message = $"Cancel buy request for {options.Pair}. Reason: trading suspended";
                return false;
            }
            else if (!options.ManualOrder && !options.Swap && !pairConfig.BuyEnabled)
            {
                message = $"Cancel buy request for {options.Pair}. Reason: buying not enabled";
                return false;
            }
            else if (!options.ManualOrder && this.Config.ExcludedPairs.Contains(options.Pair))
            {
                message = $"Cancel buy request for {options.Pair}. Reason: exluded pair";
                return false;
            }
            else if (!options.ManualOrder && !options.Arbitrage && !options.IgnoreExisting && this.Account.HasTradingPair(options.Pair))
            {
                message = $"Cancel buy request for {options.Pair}. Reason: pair already exists";
                return false;
            }
            else if (!options.ManualOrder && !options.Swap && !options.Arbitrage && pairConfig.MaxPairs != 0 && this.Account.GetTradingPairs().Count() >= pairConfig.MaxPairs && !this.Account.HasTradingPair(options.Pair))
            {
                message = $"Cancel buy request for {options.Pair}. Reason: maximum pairs reached";
                return false;
            }
            else if (!options.ManualOrder && !options.Swap && !options.IgnoreBalance && pairConfig.BuyMinBalance != 0 && (this.Account.GetBalance() - options.MaxCost) < pairConfig.BuyMinBalance && this.Exchange.GetPairMarket(options.Pair) == this.Config.Market)
            {
                message = $"Cancel buy request for {options.Pair}. Reason: minimum balance reached";
                return false;
            }
            else if (options.Price != null && options.Price <= 0)
            {
                message = $"Cancel buy request for {options.Pair}. Reason: invalid price";
                return false;
            }
            else if (options.Amount != null && options.Amount <= 0)
            {
                message = $"Cancel buy request for {options.Pair}. Reason: invalid amount";
                return false;
            }
            else if (!options.IgnoreBalance && this.Account.GetBalance() < options.MaxCost && this.Exchange.GetPairMarket(options.Pair) == this.Config.Market)
            {
                message = $"Cancel buy request for {options.Pair}. Reason: not enough balance";
                return false;
            }
            else if (options.Amount == null && options.MaxCost == null || options.Amount != null && options.MaxCost != null)
            {
                message = $"Cancel buy request for {options.Pair}. Reason: either max cost or amount needs to be specified (not both)";
            }
            else if (!options.ManualOrder && !options.Swap && !options.Arbitrage && pairConfig.BuySamePairTimeout > 0 &&
                this.OrderHistory.Any(h => h.Side == OrderSide.Buy && (h.Pair == options.Pair || h.Pair == h.OriginalPair)) &&
                (DateTimeOffset.Now - this.OrderHistory.Where(h => (h.Pair == options.Pair || h.Pair == h.OriginalPair)).Max(h => h.Date)).TotalSeconds < pairConfig.BuySamePairTimeout)
            {
                double elapsedSeconds = (DateTimeOffset.Now - this.OrderHistory.Where(h => (h.Pair == options.Pair || h.Pair == h.OriginalPair)).Max(h => h.Date)).TotalSeconds;
                message = $"Cancel buy request for {options.Pair}. Reason: buy same pair timeout (elapsed: {elapsedSeconds:0.#}, timeout: {pairConfig.BuySamePairTimeout:0.#})";
                return false;
            }

            message = null;
            return true;
        }

        public bool CanSell(SellOptions options, out string message)
        {
            IPairConfig pairConfig = this.GetPairConfig(options.Pair);

            if (!options.ManualOrder && !options.Arbitrage && this.IsTradingSuspended)
            {
                message = $"Cancel sell request for {options.Pair}. Reason: trading suspended";
                return false;
            }
            else if (!options.ManualOrder && !options.Arbitrage && !pairConfig.SellEnabled)
            {
                message = $"Cancel sell request for {options.Pair}. Reason: selling not enabled";
                return false;
            }
            else if (!options.ManualOrder && !options.Arbitrage && this.Config.ExcludedPairs.Contains(options.Pair))
            {
                message = $"Cancel sell request for {options.Pair}. Reason: excluded pair";
                return false;
            }
            else if (!this.Account.HasTradingPair(options.Pair, includeDust: true) && !this.Account.HasTradingPair(this.NormalizePair(options.Pair), includeDust: true))
            {
                message = $"Cancel sell request for {options.Pair}. Reason: pair does not exist";
                return false;
            }
            else if (options.Price != null && options.Price <= 0)
            {
                message = $"Cancel sell request for {options.Pair}. Reason: invalid price";
                return false;
            }
            else if (options.Amount != null && options.Amount <= 0)
            {
                message = $"Cancel sell request for {options.Pair}. Reason: invalid amount";
                return false;
            }
            else if (options.Amount != null && options.Price != null && (options.Amount * options.Price) < this.Config.MinCost)
            {
                message = $"Cancel sell request for {options.Pair}. Reason: dust";
                return false;
            }
            else if (!options.ManualOrder && !options.Arbitrage && (DateTimeOffset.Now - this.Account.GetTradingPair(options.Pair, includeDust: true).OrderDates.DefaultIfEmpty().Max()).
                TotalMilliseconds < (MIN_INTERVAL_BETWEEN_BUY_AND_SELL / Application.Speed))
            {
                message = $"Cancel sell request for {options.Pair}. Reason: pair just bought";
                return false;
            }
            message = null;
            return true;
        }

        public bool CanSwap(SwapOptions options, out string message)
        {
            if (!this.Account.HasTradingPair(options.OldPair))
            {
                message = $"Cancel swap request {options.OldPair} for {options.NewPair}. Reason: pair does not exist";
                return false;
            }
            else if (this.Account.HasTradingPair(options.NewPair))
            {
                message = $"Cancel swap request {options.OldPair} for {options.NewPair}. Reason: pair already exists";
                return false;
            }
            else if (!options.ManualOrder && !this.GetPairConfig(options.OldPair).SellEnabled)
            {
                message = $"Cancel swap request {options.OldPair} for {options.NewPair}. Reason: selling not enabled";
                return false;
            }
            else if (!options.ManualOrder && !this.GetPairConfig(options.NewPair).BuyEnabled)
            {
                message = $"Cancel swap request {options.OldPair} for {options.NewPair}. Reason: buying not enabled";
                return false;
            }
            else if (this.Account.GetBalance() < this.Account.GetTradingPair(options.OldPair).CurrentCost * 0.01M)
            {
                message = $"Cancel swap request {options.OldPair} for {options.NewPair}. Reason: not enough balance";
                return false;
            }
            else if (!this.Exchange.GetMarketPairs(this.Config.Market).Contains(options.NewPair))
            {
                message = $"Cancel swap request {options.OldPair} for {options.NewPair}. Reason: {options.NewPair} is not a valid pair";
                return false;
            }

            message = null;
            return true;
        }

        public bool CanArbitrage(ArbitrageOptions options, out string message)
        {
            if (this.Account.HasTradingPair(options.Pair))
            {
                message = $"Cancel arbitrage request {options.Pair}. Reason: pair already exist";
                return false;
            }
            else if (!options.ManualOrder && !this.GetPairConfig(options.Pair).BuyEnabled)
            {
                message = $"Cancel arbitrage request for {options.Pair}. Reason: buying not enabled";
                return false;
            }
            else if (!this.Exchange.GetMarketPairs(this.Config.Market).Contains(options.Pair))
            {
                message = $"Cancel arbitrage request for {options.Pair}. Reason: {options.Pair} is not a valid pair";
                return false;
            }

            message = null;
            return true;
        }

        public decimal GetPrice(string pair, TradePriceType? priceType = null, bool normalize = true)
        {
            if (normalize)
            {
                if (pair == this.Config.Market + Constants.Markets.USDT)
                {
                    return 1;
                }
            }
            return this.Exchange.GetPrice(pair, priceType ?? this.Config.TradePriceType);
        }

        public decimal CalculateOrderFees(IOrderDetails order)
        {
            decimal orderFees = 0;
            if (order.Fees != 0 && order.FeesCurrency != null)
            {
                if (order.FeesCurrency == this.Config.Market)
                {
                    orderFees = order.Fees;
                }
                else
                {
                    string feesPair = order.FeesCurrency + this.Config.Market;
                    orderFees = this.GetPrice(feesPair, TradePriceType.Ask) * order.Fees;
                }
            }
            return orderFees;
        }

        public bool IsNormalizedPair(string pair)
        {
            return this.Exchange.GetPairMarket(pair) == this.Config.Market;
        }

        public string NormalizePair(string pair)
        {
            return this.Exchange.ChangeMarket(pair, this.Config.Market);
        }

        public void LogOrder(IOrderDetails order)
        {
            this.OrderHistory.Push(order);
        }

        public List<string> GetTrailingBuys()
        {
            return tradingTimedTask.GetTrailingBuys();
        }

        public List<string> GetTrailingSells()
        {
            return tradingTimedTask.GetTrailingSells();
        }

        public void StopTrailingBuy(string pair)
        {
            tradingTimedTask.StopTrailingBuy(pair);
        }

        public void StopTrailingSell(string pair)
        {
            tradingTimedTask.StopTrailingSell(pair);
        }

        private void OnTradingRulesChanged()
        {
            this.Rules = rulesService.GetRules(this.ServiceName);
            this.RulesConfig = this.Rules.GetConfiguration<TradingRulesConfig>();
        }

        protected override void PrepareConfig()
        {
            if (this.Config.ExcludedPairs == null)
            {
                this.Config.ExcludedPairs = new List<string>();
            }

            if (this.Config.DCALevels == null)
            {
                this.Config.DCALevels = new List<DCALevel>();
            }
        }

        private void PauseTasks()
        {
            tasksService.GetTask(nameof(TradingTimedTask)).Pause();
            tasksService.GetTask(nameof(TradingRulesTimedTask)).Pause();
            tasksService.GetTask(nameof(SignalRulesTimedTask)).Pause();
            tasksService.GetTask("BacktestingLoadSnapshotsTimedTask")?.Pause();
        }

        private void ContinueTasks()
        {
            tasksService.GetTask(nameof(TradingTimedTask)).Continue();
            tasksService.GetTask(nameof(TradingRulesTimedTask)).Continue();
            tasksService.GetTask(nameof(SignalRulesTimedTask)).Continue();
            tasksService.GetTask("BacktestingLoadSnapshotsTimedTask")?.Continue();
        }
    }
}
