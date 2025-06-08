using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SiliconeTrader.Machine.Client;
using SiliconeTrader.Core.Models; // For MarketPairsRequest, MarketPairsResponse
using SiliconeTrader.Machine.Client.Models; // For MarketSignalsResponse (if still used directly)

namespace SiliconeTrader.UI.Controllers
{
    public class MarketsController : BaseController
    {
        private readonly ILogger<MarketsController> _logger;

        public MarketsController(ITradingBotClient botClient, ILogger<MarketsController> logger)
            : base(botClient)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return this.View();
        }
        public async Task<IActionResult> MarketSignals()
        {
            MarketSignalsResponse response = await BotClient.Markets.GetMarketSignals(CancellationToken.None);

            return this.Json(response.Signals);
        }

        [HttpPost]
        public async Task<IActionResult> MarketPairs([FromForm]List<string> signalsFilter)
        {
            MarketPairsResponse response = await BotClient.Markets.GetMarketPairs(new MarketPairsRequest
            {
                SignalsFilter = signalsFilter ?? new List<string>()
            }, CancellationToken.None);

            return this.Json(response.MarketPairs);
        }

    }
}