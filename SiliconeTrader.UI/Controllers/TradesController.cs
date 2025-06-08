using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Routing;
using SiliconeTrader.Machine.Client;
using SiliconeTrader.Machine.Client.Models;

namespace SiliconeTrader.UI.Controllers
{
    public class TradesController : BaseController
    {
        private readonly ILogger<TradesController> _logger;

        public TradesController(ITradingBotClient botClient, ILogger<TradesController> logger)
            : base(botClient)
        {
            _logger = logger;
        }

        public async Task<IActionResult> Index(DateTimeOffset? id)
        {
            TradesViewModel trades = await BotClient.Trading.GetTrades(id ?? DateTimeOffset.Now, CancellationToken.None);

            return this.View(trades);
        }

        [HttpPost]
        public async Task<IActionResult> TradingPairs()
        {
            TradingPairResponse tradingPairs = await BotClient.Trading.GetTradingPairs(CancellationToken.None);

            return this.Json(tradingPairs.TradingPairs);
        }
    }
}