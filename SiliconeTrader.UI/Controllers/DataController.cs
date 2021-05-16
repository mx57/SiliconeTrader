using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SiliconeTrader.Machine.Client;
using SiliconeTrader.Machine.Client.Core;

namespace SiliconeTrader.UI.Controllers
{
    public class DataController : BaseController
    {
        private readonly ILogger<DashboardController> _logger;

        public DataController(ITradingBotClient botClient, ILogger<DashboardController> logger)
            : base(botClient)
        {
            _logger = logger;
        }

        public IActionResult Market()
        {
            return this.View();
        }

        [HttpPost]
        public IActionResult MarketPairs(List<string> signalsFilter)
        {
            return this.View();
        }

        public IActionResult Trades(DateTimeOffset id)
        {
            return this.View();
        }

        [HttpPost]
        public async Task<IActionResult> TradingPairs()
        {
            TradingPairResponse tradingPairs = await BotClient.Trading.GetTradingPairs(CancellationToken.None);

            return this.Json(tradingPairs.TradingPairs);
        }
    }
}