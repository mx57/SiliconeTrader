using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SiliconeTrader.Core;
using SiliconeTrader.Machine.Client;
using SiliconeTrader.Machine.Client.Core;
using SiliconeTrader.Machine.Client.Models;

namespace SiliconeTrader.Machine.Controllers
{
    [Route("api/ORCA/v1")]
    [ApiController]
    public class OrcaApiController : ControllerBase
    {
        #region Buy
        [HttpPost("buy/{pair}/{amount}")]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(422)]
        public IActionResult Buy(string pair, string amount)
        {
            if (Application.Resolve<ICoreService>().Config.ReadOnlyMode)
            {
                return new BadRequestResult();
            }

            if (!decimal.TryParse(amount, out decimal buyAmount))
            {
                return new BadRequestResult();
            }

            if (!string.IsNullOrWhiteSpace(pair) && buyAmount > 0)
            {
                ITradingService tradingService = Application.Resolve<ITradingService>();

                tradingService.Buy(new BuyOptions(pair)
                {
                    Amount = buyAmount,
                    IgnoreExisting = true,
                    ManualOrder = true
                });

                return new OkResult();
            }

            return new BadRequestResult();
        }

        [HttpPost("buy-default/{pair}")]
        public IActionResult BuyDefault(string pair)
        {
            if (Application.Resolve<ICoreService>().Config.ReadOnlyMode)
            {
                return new BadRequestResult();
            }

            if (!string.IsNullOrWhiteSpace(pair))
            {
                ISignalsService signalsService = Application.Resolve<ISignalsService>();
                ITradingService tradingService = Application.Resolve<ITradingService>();
                tradingService.Buy(new BuyOptions(pair)
                {
                    MaxCost = tradingService.GetPairConfig(pair).BuyMaxCost,
                    IgnoreExisting = true,
                    ManualOrder = true,
                    Metadata = new OrderMetadata
                    {
                        BoughtGlobalRating = signalsService.GetGlobalRating()
                    }
                });
                return new OkResult();
            }
            else
            {
                return new BadRequestResult();
            }
        }

        #endregion


        #region Markets
        [HttpPost("market-pairs")]
        [ProducesResponseType(200, Type = typeof(MarketPairsResponse))]
        public MarketPairsResponse MarketPairs(MarketPairsRequest request)
        {
            if (request?.SignalsFilter == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            ICoreService coreService = Application.Resolve<ICoreService>();
            ITradingService tradingService = Application.Resolve<ITradingService>();
            ISignalsService signalsService = Application.Resolve<ISignalsService>();

            IEnumerable<ISignal> allSignals = signalsService.GetAllSignals();

            if (allSignals != null)
            {
                if (request.SignalsFilter.Count > 0)
                {
                    allSignals = allSignals.Where(s => request.SignalsFilter.Contains(s.Name));
                }

                var groupedSignals = allSignals.GroupBy(s => s.Pair).ToDictionary(g => g.Key, g => g.AsEnumerable());

                IEnumerable<MarketPairApiModel> marketPairs = from signalGroup in groupedSignals
                                                              let pair = signalGroup.Key
                                                              let pairConfig = tradingService.GetPairConfig(pair)
                                                              select new MarketPairApiModel
                                                              {
                                                                  Name = pair,
                                                                  TradingViewName = $"{tradingService.Config.Exchange.ToUpperInvariant()}:{pair}",
                                                                  VolumeList = signalGroup.Value.Select(s => new NameValue<long?>(s.Name, s.Volume)),
                                                                  VolumeChangeList = signalGroup.Value.Select(s => new NameValue<double?>(s.Name, s.VolumeChange)),
                                                                  Price = tradingService.GetPrice(pair).ToString("0.00000000"),
                                                                  PriceChangeList = signalGroup.Value.Select(s => new NameValue<decimal?>(s.Name, s.PriceChange)),
                                                                  RatingList = signalGroup.Value.Select(s => new NameValue<double?>(s.Name, s.Rating)),
                                                                  RatingChangeList = signalGroup.Value.Select(s => new NameValue<double?>(s.Name, s.RatingChange)),
                                                                  VolatilityList = signalGroup.Value.Select(s => new NameValue<double?>(s.Name, s.Volatility)),
                                                                  Spread = tradingService.Exchange.GetPriceSpread(pair).ToString("0.00"),
                                                                  ArbitrageList = from market in Enum.GetNames(typeof(ArbitrageMarket)).Where(m => m != tradingService.Config.Market)
                                                                                  let arbitrage = tradingService.Exchange.GetArbitrage(pair, tradingService.Config.Market, new List<ArbitrageMarket> { Enum.Parse<ArbitrageMarket>(market) })
                                                                                  select new ArbitrageInfo
                                                                                  {
                                                                                      Name = $"{arbitrage.Market}-{arbitrage.Type.ToString()[0]}",
                                                                                      Arbitrage = arbitrage.IsAssigned ? arbitrage.Percentage.ToString("0.00") : "N/A"
                                                                                  },
                                                                  SignalRules = signalsService.GetTrailingInfo(pair)?.Select(ti => ti.Rule.Name) ?? Array.Empty<string>(),
                                                                  HasTradingPair = tradingService.Account.HasTradingPair(pair),
                                                                  Config = pairConfig
                                                              };

                return new MarketPairsResponse
                {
                    MarketPairs = marketPairs.ToList()
                };
            }
            else
            {
                return null;
            }
        }

        [HttpGet("market-signals")]
        [ProducesResponseType(200, Type = typeof(MarketSignalsResponse))]
        public MarketSignalsResponse MarketSignals()
        {
            ISignalsService signalsService = Application.Resolve<ISignalsService>();

            return new MarketSignalsResponse
            {
                Signals = signalsService.GetSignalNames()
            };
        }

        #endregion

        #region System
        [HttpGet("account/refresh")]
        public IActionResult RefreshAccount()
        {
            if (!Application.Resolve<ICoreService>().Config.ReadOnlyMode)
            {
                ITradingService tradingService = Application.Resolve<ITradingService>();
                tradingService.Account.Refresh();
                return new OkResult();
            }
            else
            {
                return new BadRequestResult();
            }
        }

        [HttpGet("rules")]
        public RulesViewModel Rules()
        {
            Dictionary<DateTimeOffset, List<TradeResult>> allTades = this.GetTrades();
            var signalRuleStats = new Dictionary<string, SignalRuleStats>();
            foreach (TradeResult trade in allTades.Values.SelectMany(t => t))
            {
                if (trade.IsSuccessful)
                {
                    string signalRule = trade?.Metadata?.SignalRule;
                    if (!string.IsNullOrWhiteSpace(signalRule))
                    {
                        if (!signalRuleStats.TryGetValue(signalRule, out SignalRuleStats ruleStats))
                        {
                            ruleStats = new SignalRuleStats();
                            signalRuleStats.Add(signalRule, ruleStats);
                        }

                        if (!trade.IsSwap)
                        {
                            ruleStats.TotalCost += trade.Cost;
                            ruleStats.TotalProfit += trade.Profit;
                            decimal margin = trade.Profit / (trade.Cost + (trade.Metadata?.AdditionalCosts ?? 0)) * 100;
                            if (trade.OrderDates.Count == 1)
                            {
                                ruleStats.Margin.Add(margin);
                            }
                            else
                            {
                                ruleStats.MarginDCA.Add(margin);
                            }
                        }
                        else
                        {
                            ruleStats.TotalSwaps++;
                        }

                        ruleStats.TotalTrades++;
                        ruleStats.TotalOrders += trade.OrderDates.Count;
                        ruleStats.TotalFees += trade.FeesTotal;
                        ruleStats.Age.Add((trade.SellDate - trade.OrderDates.Min()).TotalDays);
                        ruleStats.DCA.Add((trade.OrderDates.Count - 1) + (trade.Metadata?.AdditionalDCALevels ?? 0));
                    }
                }
            }

            ICoreService coreService = Application.Resolve<ICoreService>();

            var model = new RulesViewModel
            {
                InstanceName = coreService.Config.InstanceName,
                Version = coreService.Version,
                ReadOnlyMode = coreService.Config.ReadOnlyMode,
                SignalRuleStats = signalRuleStats
            };

            return model;
        }

        [HttpGet("instance")]
        public MarketViewModel Instance()
        {
            ICoreService coreService = Application.Resolve<ICoreService>();

            var model = new MarketViewModel
            {
                InstanceName = coreService.Config.InstanceName,
                Version = coreService.Version,
                ReadOnlyMode = coreService.Config.ReadOnlyMode
            };

            return model;
        }

        [HttpGet("log")]
        public LogViewModel Log()
        {
            ICoreService coreService = Application.Resolve<ICoreService>();

            ILoggingService loggingService = Application.Resolve<ILoggingService>();

            var model = new LogViewModel()
            {
                InstanceName = coreService.Config.InstanceName,
                Version = coreService.Version,
                ReadOnlyMode = coreService.Config.ReadOnlyMode,
                LogEntries = loggingService.GetLogEntries().Reverse().Take(500)
            };

            return model;
        }
        [HttpGet("services/restart")]
        public IActionResult RestartServices()
        {
            if (!Application.Resolve<ICoreService>().Config.ReadOnlyMode)
            {
                ICoreService coreService = Application.Resolve<ICoreService>();
                coreService.Restart();
                return new OkResult();
            }
            else
            {
                return new BadRequestResult();
            }
        }

        [HttpPost("config/{configName}/save")]
        public IActionResult SaveConfig(string configName, [FromBody] string configDefinition)
        {

            if (!Application.Resolve<ICoreService>().Config.ReadOnlyMode && !string.IsNullOrWhiteSpace(configName) && !string.IsNullOrWhiteSpace(configDefinition))
            {
                Application.ConfigProvider.SetSectionJson(configName, configDefinition);
                return new OkResult();
            }
            else
            {
                return new BadRequestResult();
            }
        }

        #endregion
        #region Sell
        [HttpPost("sell/{pair}/{amount}")]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(422)]
        public IActionResult Sell(string pair, string amount)
        {
            if (Application.Resolve<ICoreService>().Config.ReadOnlyMode)
            {
                return new BadRequestResult();
            }

            if (!decimal.TryParse(amount, out decimal sellAmount))
            {
                return new BadRequestResult();
            }

            if (!string.IsNullOrWhiteSpace(pair) && sellAmount > 0)
            {
                ITradingService tradingService = Application.Resolve<ITradingService>();

                tradingService.Sell(new SellOptions(pair)
                {
                    Amount = sellAmount,
                    ManualOrder = true
                });

                return new OkResult();
            }

            return new BadRequestResult();
        }

        #endregion
        #region Settings

        [HttpGet("settings")]
        public SettingsViewModel Settings()
        {
            ICoreService coreService = Application.Resolve<ICoreService>();

            ITradingService tradingService = Application.Resolve<ITradingService>();
            IEnumerable<IConfigurableService> allConfigurableServices = Application.Resolve<IEnumerable<IConfigurableService>>();

            var model = new SettingsViewModel()
            {
                InstanceName = coreService.Config.InstanceName,
                Version = coreService.Version,
                ReadOnlyMode = coreService.Config.ReadOnlyMode,
                BuyEnabled = tradingService.Config.BuyEnabled,
                BuyDCAEnabled = tradingService.Config.BuyDCAEnabled,
                SellEnabled = tradingService.Config.SellEnabled,
                TradingSuspended = tradingService.IsTradingSuspended,
                HealthCheckEnabled = coreService.Config.HealthCheckEnabled,
                Configs = allConfigurableServices.Where(s => !s.GetType().Name.Contains(Constants.ServiceNames.BacktestingService)).OrderBy(s => s.ServiceName).ToDictionary(s => s.ServiceName, s => Application.ConfigProvider.GetSectionJson(s.ServiceName))
            };

            return model;
        }

        [HttpPost("settings")]
        public SettingsViewModel Settings(SettingsViewModel model)
        {
            if (!Application.Resolve<ICoreService>().Config.ReadOnlyMode)
            {
                ICoreService coreService = Application.Resolve<ICoreService>();
                ITradingService tradingService = Application.Resolve<ITradingService>();

                coreService.Config.HealthCheckEnabled = model.HealthCheckEnabled;
                tradingService.Config.BuyEnabled = model.BuyEnabled;
                tradingService.Config.BuyDCAEnabled = model.BuyDCAEnabled;
                tradingService.Config.SellEnabled = model.SellEnabled;

                if (model.TradingSuspended)
                {
                    tradingService.SuspendTrading();
                }
                else
                {
                    tradingService.ResumeTrading();
                }
                return this.Settings();
            }
            else
            {
                return this.Settings();
            }
        }

        #endregion

        #region Stats

        [HttpGet("stats")]
        public StatsViewModel Stats()
        {
            ICoreService coreService = Application.Resolve<ICoreService>();

            ITradingService tradingService = Application.Resolve<ITradingService>();
            decimal accountInitialBalance = tradingService.Config.VirtualTrading ? tradingService.Config.VirtualAccountInitialBalance : tradingService.Config.AccountInitialBalance;
            DateTimeOffset accountInitialBalanceDate = tradingService.Config.VirtualTrading ? DateTimeOffset.Now.AddDays(-30) : tradingService.Config.AccountInitialBalanceDate;

            var model = new StatsViewModel
            {
                InstanceName = coreService.Config.InstanceName,
                Version = coreService.Version,
                ReadOnlyMode = coreService.Config.ReadOnlyMode,
                TimezoneOffset = coreService.Config.TimezoneOffset,
                AccountInitialBalance = accountInitialBalance,
                AccountBalance = tradingService.Account.GetTotalBalance(),
                Market = tradingService.Config.Market,
                Balances = new Dictionary<DateTimeOffset, decimal>(),
                Trades = this.GetTrades()
            };

            foreach (KeyValuePair<DateTimeOffset, List<TradeResult>> kvp in model.Trades.OrderBy(k => k.Key))
            {
                DateTimeOffset date = kvp.Key;
                List<TradeResult> trades = kvp.Value;

                model.Balances[date] = accountInitialBalance;

                if (date > accountInitialBalanceDate.Date)
                {
                    for (int d = 1; d < (int)(date - accountInitialBalanceDate.Date).TotalDays; d++)
                    {
                        DateTimeOffset prevDate = date.AddDays(-d);
                        if (model.Trades.ContainsKey(prevDate))
                        {
                            model.Balances[date] += model.Trades[prevDate].Where(t => !t.IsSwap).Sum(t => t.Profit);
                        }
                    }
                }
            }

            return model;
        }

        [HttpGet("status")]
        public StatusApiModel Status()
        {
            ILoggingService loggingService = Application.Resolve<ILoggingService>();
            ITradingService tradingService = Application.Resolve<ITradingService>();
            ISignalsService signalsService = Application.Resolve<ISignalsService>();
            IHealthCheckService healthCheckService = Application.Resolve<IHealthCheckService>();

            var status = new StatusApiModel
            {
                Balance = tradingService.Account.GetBalance(),
                GlobalRating = signalsService.GetGlobalRating()?.ToString("0.000") ?? "N/A",
                TrailingBuys = tradingService.GetTrailingBuys(),
                TrailingSells = tradingService.GetTrailingSells(),
                TrailingSignals = signalsService.GetTrailingSignals(),
                TradingSuspended = tradingService.IsTradingSuspended,
                HealthChecks = healthCheckService.GetHealthChecks().OrderBy(c => c.Name),
                LogEntries = loggingService.GetLogEntries().Reverse().Take(5)
            };
            return status;
        }

        [HttpGet("version")]
        public string Version()
        {
            return Application.Resolve<ICoreService>().Version;
        }

        #endregion

        #region Trading
        [HttpPost("{pair}/swap/{swap}")]
        public IActionResult Swap(string pair, string swap)
        {
            if (!Application.Resolve<ICoreService>().Config.ReadOnlyMode && !string.IsNullOrWhiteSpace(pair) && !string.IsNullOrWhiteSpace(swap))
            {
                ITradingService tradingService = Application.Resolve<ITradingService>();
                tradingService.Swap(new SwapOptions(pair, swap, new OrderMetadata())
                {
                    ManualOrder = true
                });
                return new OkResult();
            }
            else
            {
                return new BadRequestResult();
            }
        }

        [HttpGet("trades/{id}")]
        public TradesViewModel Trades(DateTimeOffset id)
        {
            ICoreService coreService = Application.Resolve<ICoreService>();

            var model = new TradesViewModel()
            {
                InstanceName = coreService.Config.InstanceName,
                Version = coreService.Version,
                ReadOnlyMode = coreService.Config.ReadOnlyMode,
                TimezoneOffset = coreService.Config.TimezoneOffset,
                Date = id,
                Trades = this.GetTrades(id).Values.FirstOrDefault() ?? new List<TradeResult>()
            };

            return model;
        }

        [HttpGet("trading-pairs")]
        public TradingPairResponse TradingPairs()
        {
            ICoreService coreService = Application.Resolve<ICoreService>();
            ITradingService tradingService = Application.Resolve<ITradingService>();

            IEnumerable<TradingPairApiModel> tradingPairs = from tradingPair in tradingService.Account.GetTradingPairs()
                                                            let pairConfig = tradingService.GetPairConfig(tradingPair.Pair)
                                                            select new TradingPairApiModel
                                                            {
                                                                Name = tradingPair.Pair,
                                                                DCA = tradingPair.DCALevel,
                                                                TradingViewName = $"{tradingService.Config.Exchange.ToUpperInvariant()}:{tradingPair.Pair}",
                                                                Margin = tradingPair.CurrentMargin.ToString("0.00"),
                                                                Target = pairConfig.SellMargin.ToString("0.00"),
                                                                CurrentPrice = tradingPair.CurrentPrice.ToString("0.00000000"),
                                                                CurrentSpread = tradingPair.CurrentSpread.ToString("0.00"),
                                                                BoughtPrice = tradingPair.AveragePrice.ToString("0.00000000"),
                                                                Cost = tradingPair.Cost.ToString("0.00000000"),
                                                                CurrentCost = tradingPair.CurrentCost.ToString("0.00000000"),
                                                                Amount = tradingPair.Amount.ToString("0.########"),
                                                                OrderDates = tradingPair.OrderDates.Select(d => d.ToOffset(TimeSpan.FromHours(coreService.Config.TimezoneOffset)).ToString("yyyy-MM-dd HH:mm:ss")),
                                                                OrderIds = tradingPair.OrderIds,
                                                                Age = tradingPair.CurrentAge.ToString("0.00"),
                                                                CurrentRating = tradingPair.Metadata.CurrentRating?.ToString("0.000") ?? "N/A",
                                                                BoughtRating = tradingPair.Metadata.BoughtRating?.ToString("0.000") ?? "N/A",
                                                                SignalRule = tradingPair.Metadata.SignalRule ?? "N/A",
                                                                SwapPair = tradingPair.Metadata.SwapPair,
                                                                TradingRules = pairConfig.Rules,
                                                                IsTrailingSell = tradingService.GetTrailingSells().Contains(tradingPair.Pair),
                                                                IsTrailingBuy = tradingService.GetTrailingBuys().Contains(tradingPair.Pair),
                                                                LastBuyMargin = tradingPair.Metadata.LastBuyMargin?.ToString("0.00") ?? "N/A",
                                                                Config = pairConfig
                                                            };

            return new TradingPairResponse
            {
                TradingPairs = tradingPairs.ToList()
            };
        }

        private Dictionary<DateTimeOffset, List<TradeResult>> GetTrades(DateTimeOffset? date = null)
        {
            ICoreService coreService = Application.Resolve<ICoreService>();
            string logsPath = Path.Combine(Directory.GetCurrentDirectory(), "log");
            var tradeResultPattern = new Regex($"{nameof(TradeResult)} (?<data>\\{{.*\\}})", RegexOptions.Compiled);
            var trades = new Dictionary<DateTimeOffset, List<TradeResult>>();

            if (Directory.Exists(logsPath))
            {
                foreach (string tradesLogFilePath in Directory.EnumerateFiles(logsPath, "*-trades.txt", SearchOption.TopDirectoryOnly))
                {
                    IEnumerable<string> logLines = Misc.Utils.ReadAllLinesWriteSafe(tradesLogFilePath);
                    foreach (string logLine in logLines)
                    {
                        Match match = tradeResultPattern.Match(logLine);
                        if (match.Success)
                        {
                            string data = match.Groups["data"].ToString();
                            string json = Misc.Utils.FixInvalidJson(data.Replace(nameof(OrderMetadata), ""))
                                .Replace("AveragePricePaid", nameof(ITradeResult.AveragePrice)); // Old property migration

                            TradeResult tradeResult = JsonConvert.DeserializeObject<TradeResult>(json);
                            if (tradeResult.IsSuccessful && tradeResult.Metadata?.IsTransitional != true)
                            {
                                DateTimeOffset tradeDate = tradeResult.SellDate.ToOffset(TimeSpan.FromHours(coreService.Config.TimezoneOffset)).Date;
                                if (date == null || date == tradeDate)
                                {
                                    if (!trades.ContainsKey(tradeDate))
                                    {
                                        trades.Add(tradeDate, new List<TradeResult>());
                                    }
                                    trades[tradeDate].Add(tradeResult);
                                }
                            }
                        }
                    }
                }
            }
            return trades;
        }

        #endregion

    }
}