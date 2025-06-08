using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SiliconeTrader.Core;

namespace SiliconeTrader.Machine.Controllers
{
    [Route("api/ORCA/v1")]
    [ApiController]
    public partial class OrcaApiController : ControllerBase
    {
        private readonly ICoreService _coreService;
        private readonly ITradingService _tradingService;
        private readonly ILoggingService _loggingService;
        private readonly ISignalsService _signalsService;
        private readonly IHealthCheckService _healthCheckService;
        private readonly IEnumerable<IConfigurableService> _allConfigurableServices;
        private readonly IConfigProvider _configProvider;
        private readonly Core.Interfaces.Services.IMarketDataService _marketDataService; // Added IMarketDataService

        public OrcaApiController(
            ICoreService coreService,
            ITradingService tradingService,
            ILoggingService loggingService,
            ISignalsService signalsService,
            IHealthCheckService healthCheckService,
            IEnumerable<IConfigurableService> allConfigurableServices,
            IConfigProvider configProvider,
            Core.Interfaces.Services.IMarketDataService marketDataService) // Added IMarketDataService
        {
            _coreService = coreService ?? throw new ArgumentNullException(nameof(coreService));
            _tradingService = tradingService ?? throw new ArgumentNullException(nameof(tradingService));
            _loggingService = loggingService ?? throw new ArgumentNullException(nameof(loggingService));
            _signalsService = signalsService ?? throw new ArgumentNullException(nameof(signalsService));
            _healthCheckService = healthCheckService ?? throw new ArgumentNullException(nameof(healthCheckService));
            _allConfigurableServices = allConfigurableServices ?? throw new ArgumentNullException(nameof(allConfigurableServices));
            _configProvider = configProvider ?? throw new ArgumentNullException(nameof(configProvider));
            _marketDataService = marketDataService ?? throw new ArgumentNullException(nameof(marketDataService)); // Added IMarketDataService
        }

        // Made instance method, async, and uses injected _coreService
        private async Task<Dictionary<DateTimeOffset, List<TradeResult>>> GetTradesAsync(DateTimeOffset? date = null)
        {
            string logsPath = Path.Combine(Directory.GetCurrentDirectory(), "log");
            var tradeResultPattern = new Regex($"{nameof(TradeResult)} (?<data>\\{{.*\\}})", RegexOptions.Compiled);
            var trades = new Dictionary<DateTimeOffset, List<TradeResult>>();

            if (Directory.Exists(logsPath))
            {
                foreach (string tradesLogFilePath in Directory.EnumerateFiles(logsPath, "*-trades.txt", SearchOption.TopDirectoryOnly))
                {
                    string[] logLines = await Misc.Utils.ReadAllLinesWriteSafeAsync(tradesLogFilePath); // Use new async method
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
                                DateTimeOffset tradeDate = tradeResult.SellDate.ToOffset(TimeSpan.FromHours(_coreService.Config.TimezoneOffset)).Date;
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
    }
}