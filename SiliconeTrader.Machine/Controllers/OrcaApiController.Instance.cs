using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using SiliconeTrader.Core;
using SiliconeTrader.Machine.Client.Models;

namespace SiliconeTrader.Machine.Controllers
{
    public partial class OrcaApiController
    {
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

        [HttpPost("services/restart")]
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

        [HttpGet("rules")]
        public RulesViewModel Rules()
        {
            ICoreService coreService = Application.Resolve<ICoreService>();
            var signalRuleStats = new Dictionary<string, SignalRuleStats>();

            Dictionary<DateTimeOffset, List<TradeResult>> allTades = GetTrades();

            foreach (TradeResult trade in allTades.Values.SelectMany(t => t))
            {
                if (!trade.IsSuccessful)
                {
                    continue;
                }

                string signalRule = trade?.Metadata?.SignalRule;
                if (string.IsNullOrWhiteSpace(signalRule))
                {
                    continue;
                }

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

            var model = new RulesViewModel
            {
                InstanceName = coreService.Config.InstanceName,
                Version = coreService.Version,
                ReadOnlyMode = coreService.Config.ReadOnlyMode,
                SignalRuleStats = signalRuleStats
            };

            return model;
        }

        [HttpPost("config/{configName}/save")]
        public IActionResult SaveConfig(string configName, SaveConfigRequest request)
        {
            if (request == null)
            {
                return new BadRequestResult();
            }

            if (!Application.Resolve<ICoreService>().Config.ReadOnlyMode
                && !string.IsNullOrWhiteSpace(configName)
                && !string.IsNullOrWhiteSpace(request.ConfigDefinition))
            {
                Application.ConfigProvider.SetSectionJson(configName, request.ConfigDefinition);

                return new OkResult();
            }
            else
            {
                return new BadRequestResult();
            }
        }

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
                Configs = allConfigurableServices
                    .Where(s => !s.GetType().Name.Contains(Constants.ServiceNames.BacktestingService))
                    .OrderBy(s => s.ServiceName)
                    .ToDictionary(s => s.ServiceName, s => Application.ConfigProvider.GetSectionJson(s.ServiceName))
            };

            return model;
        }

        [HttpPost("settings")]
        public SettingsViewModel Settings(SaveSettingsRequest model)
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

        [HttpGet("stats")]
        public StatsViewModel Stats()
        {
            ICoreService coreService = Application.Resolve<ICoreService>();
            ITradingService tradingService = Application.Resolve<ITradingService>();

            decimal accountInitialBalance = tradingService.Config.VirtualTrading
                ? tradingService.Config.VirtualAccountInitialBalance
                : tradingService.Config.AccountInitialBalance;

            DateTimeOffset accountInitialBalanceDate = tradingService.Config.VirtualTrading
                ? DateTimeOffset.Now.AddDays(-30)
                : tradingService.Config.AccountInitialBalanceDate;

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
                Trades = GetTrades()
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

        [HttpGet("status")] // TODO
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

        [HttpGet("version")] // TODO
        public string Version()
        {
            return Application.Resolve<ICoreService>().Version;
        }
    }
}