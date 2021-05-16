using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SiliconeTrader.Machine.Client;
using SiliconeTrader.Machine.Client.Models;

namespace SiliconeTrader.UI.Controllers
{
    public class BotController : BaseController
    {
        private readonly ILogger<DashboardController> _logger;

        public BotController(ITradingBotClient botClient, ILogger<DashboardController> logger)
            : base(botClient)
        {
            _logger = logger;
        }

        public IActionResult Log()
        {
            return this.View();
        }

        public IActionResult Rules()
        {
            return this.View();
        }

        [HttpPost]
        public IActionResult SaveConfig()
        {
            return this.View();
        }


        public IActionResult Settings()
        {
            return this.View();
        }

        [HttpPost]
        public IActionResult Settings(SettingsViewModel model)
        {
            return this.View();
        }

        public IActionResult SignalNames()
        {
            return this.View();
        }

        public IActionResult Stats()
        {
            return this.View();
        }

        public IActionResult Status()
        {
            return this.View();
        }
        public IActionResult RefreshAccount()
        {
            return this.View();
        }

        public IActionResult RestartServices()
        {
            return this.View();
        }

    }
}