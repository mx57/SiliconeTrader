using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks; // Added for Task<>
using Microsoft.AspNetCore.Mvc;
using SiliconeTrader.Core;
using SiliconeTrader.Machine.Client.Models;

namespace SiliconeTrader.Machine.Controllers
{
    public partial class OrcaApiController
    {
        [HttpGet("instance")]
        public ActionResult<MarketViewModel> Instance() // Changed return type
        {
            var model = new MarketViewModel
            {
                InstanceName = _coreService.Config.InstanceName,
                Version = _coreService.Version,
                ReadOnlyMode = _coreService.Config.ReadOnlyMode
            };

            return Ok(model); // Return Ok(model)
        }

        [HttpGet("log")]
        public async Task<ActionResult<LogViewModel>> Log() // Made async, changed return type
        {
            var logEntries = await Task.Run(() => _loggingService.GetLogEntries().Reverse().Take(500).ToList()); // Wrapped in Task.Run
            var model = new LogViewModel()
            {
                InstanceName = _coreService.Config.InstanceName,
                Version = _coreService.Version,
                ReadOnlyMode = _coreService.Config.ReadOnlyMode,
                LogEntries = logEntries
            };

            return Ok(model); // Return Ok(model)
        }

        [HttpPost("services/restart")]
        public async Task<IActionResult> RestartServices() // Made async
        {
            if (!_coreService.Config.ReadOnlyMode)
            {
                await Task.Run(() => _coreService.Restart()); // Wrapped in Task.Run
                return Ok(); // Return Ok()
            }
            else
            {
                return BadRequest(); // Return BadRequest()
            }
        }

        [HttpGet("rules")]
        public async Task<ActionResult<RulesViewModel>> Rules() // Changed return type
        {
            var signalRuleStats = new Dictionary<string, SignalRuleStats>();

            Dictionary<DateTimeOffset, List<TradeResult>> allTades = await GetTradesAsync();

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
                InstanceName = _coreService.Config.InstanceName,
                Version = _coreService.Version,
                ReadOnlyMode = _coreService.Config.ReadOnlyMode,
                SignalRuleStats = signalRuleStats
            };

            return Ok(model); // Return Ok(model)
        }

        [HttpPost("config/{configName}/save")]
        public async Task<IActionResult> SaveConfig(string configName, [FromBody] SaveConfigRequest request) // Added configName route parameter
        {
            if (request == null)
            {
                return BadRequest("Request cannot be null.");
            }
            // configName is now a parameter, so this specific check might be redundant if routing handles missing route params, but good for robustness.
            if (string.IsNullOrWhiteSpace(configName))
            {
                return BadRequest("Config name cannot be empty.");
            }
            if (string.IsNullOrWhiteSpace(request.ConfigDefinition))
            {
                return BadRequest("Config definition cannot be empty.");
            }

            if (!_coreService.Config.ReadOnlyMode)
            {
                await Task.Run(() => _configProvider.SetSectionJson(configName, request.ConfigDefinition)); // Wrapped in Task.Run
                return Ok();
            }
            else
            {
                return BadRequest("Cannot save config in read-only mode.");
            }
        }

        [HttpGet("settings")]
        public async Task<ActionResult<SettingsViewModel>> Settings() // Made async, changed return type
        {
            var configs = await Task.Run(() => _allConfigurableServices // Use injected _allConfigurableServices
                .Where(s => !s.GetType().Name.Contains(Constants.ServiceNames.BacktestingService))
                .OrderBy(s => s.ServiceName)
                .ToDictionary(s => s.ServiceName, s => _configProvider.GetSectionJson(s.ServiceName))); // Use injected _configProvider

            var model = new SettingsViewModel()
            {
                InstanceName = _coreService.Config.InstanceName,
                Version = _coreService.Version,
                ReadOnlyMode = _coreService.Config.ReadOnlyMode,
                BuyEnabled = _tradingService.Config.BuyEnabled,
                BuyDCAEnabled = _tradingService.Config.BuyDCAEnabled,
                SellEnabled = _tradingService.Config.SellEnabled,
                TradingSuspended = _tradingService.IsTradingSuspended,
                HealthCheckEnabled = _coreService.Config.HealthCheckEnabled,
                Configs = configs
            };

            return Ok(model); // Return Ok(model)
        }

        [HttpPost("settings")]
        public async Task<ActionResult<SettingsViewModel>> Settings([FromBody] SaveSettingsRequest model) // Added [FromBody], Made async
        {
            if (!_coreService.Config.ReadOnlyMode)
            {
                // These are config property sets, likely quick, no Task.Run needed unless services do I/O on set.
                _coreService.Config.HealthCheckEnabled = model.HealthCheckEnabled;
                _tradingService.Config.BuyEnabled = model.BuyEnabled;
                _tradingService.Config.BuyDCAEnabled = model.BuyDCAEnabled;
                _tradingService.Config.SellEnabled = model.SellEnabled;

                if (model.TradingSuspended)
                {
                    await Task.Run(() => _tradingService.SuspendTrading()); // Wrapped in Task.Run
                }
                else
                {
                    await Task.Run(() => _tradingService.ResumeTrading()); // Wrapped in Task.Run
                }
                // Re-fetch settings to return updated view
                var settingsViewModel = await Settings(); // Await the async GET Settings()
                return settingsViewModel;
            }
            else
            {
                // In read-only mode, just return current settings
                var settingsViewModel = await Settings();
                return settingsViewModel;
            }
        }

        [HttpGet("stats")]
        public async Task<ActionResult<StatsViewModel>> Stats() // Changed return type
        {
            decimal accountInitialBalance = _tradingService.Config.VirtualTrading
                ? _tradingService.Config.VirtualAccountInitialBalance
                : _tradingService.Config.AccountInitialBalance;

            DateTimeOffset accountInitialBalanceDate = _tradingService.Config.VirtualTrading
                ? DateTimeOffset.Now.AddDays(-30)
                : _tradingService.Config.AccountInitialBalanceDate;

            var model = new StatsViewModel
            {
                InstanceName = _coreService.Config.InstanceName,
                Version = _coreService.Version,
                ReadOnlyMode = _coreService.Config.ReadOnlyMode,
                TimezoneOffset = _coreService.Config.TimezoneOffset,
                AccountInitialBalance = accountInitialBalance,
                AccountBalance = _tradingService.Account.GetTotalBalance(),
                Market = _tradingService.Config.Market,
                Balances = new Dictionary<DateTimeOffset, decimal>(),
                Trades = await GetTradesAsync()
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
        public async Task<ActionResult<StatusApiModel>> Status() // Made async, changed return type
        {
            // Using injected services
            var balance = await Task.Run(() => _tradingService.Account.GetBalance());
            var globalRating = await Task.Run(() => _signalsService.GetGlobalRating());
            var trailingBuys = await Task.Run(() => _tradingService.GetTrailingBuys());
            var trailingSells = await Task.Run(() => _tradingService.GetTrailingSells());
            var trailingSignals = await Task.Run(() => _signalsService.GetTrailingSignals());
            var healthChecks = await Task.Run(() => _healthCheckService.GetHealthChecks().OrderBy(c => c.Name).ToList()); // ToList after Task.Run
            var logEntries = await Task.Run(() => _loggingService.GetLogEntries().Reverse().Take(5).ToList()); // ToList after Task.Run

            var status = new StatusApiModel
            {
                Balance = balance,
                GlobalRating = globalRating?.ToString("0.000") ?? "N/A",
                TrailingBuys = trailingBuys,
                TrailingSells = trailingSells,
                TrailingSignals = trailingSignals,
                TradingSuspended = _tradingService.IsTradingSuspended, // This is likely a quick property access
                HealthChecks = healthChecks,
                LogEntries = logEntries
            };
            return Ok(status);
        }

        [HttpGet("version")]
        public ActionResult<string> Version() // Changed return type
        {
            return Ok(_coreService.Version); // Return Ok(value)
        }
    }
}