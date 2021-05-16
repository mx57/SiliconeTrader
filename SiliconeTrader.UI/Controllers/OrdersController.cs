using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SiliconeTrader.Machine.Client;

namespace SiliconeTrader.UI.Controllers
{
    public class OrdersController : BaseController
    {
        private readonly ILogger<DashboardController> _logger;

        public OrdersController(ITradingBotClient botClient, ILogger<DashboardController> logger)
            : base(botClient)
        {
            _logger = logger;
        }

        [HttpPost]
        public IActionResult Buy()
        {
            return this.View();
        }

        [HttpPost]
        public IActionResult BuyDefault()
        {
            return this.View();
        }

        [HttpPost]
        public IActionResult Sell()
        {
            return this.View();
        }

    }
}