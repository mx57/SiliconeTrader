using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SiliconeTrader.Machine.Client;
using SiliconeTrader.Machine.Client.Core;

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