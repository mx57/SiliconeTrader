using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SiliconeTrader.Machine.Client;

namespace SiliconeTrader.UI.Controllers
{
    public class DashboardController : BaseController
    {
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(ITradingBotClient botClient, ILogger<DashboardController> logger)
            : base(botClient)
        {
            _logger = logger;
        }

        public IActionResult Help()
        {
            return this.View();
        }

        public IActionResult Index()
        {
            return this.View();
        }
    }
}