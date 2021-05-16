using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SiliconeTrader.Machine.Client;
using SiliconeTrader.Machine.Client.Models;
using SiliconeTrader.UI.Models;

namespace SiliconeTrader.UI.Controllers
{
    public class HomeController : BaseController
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, ITradingBotClient botClient)
            : base(botClient)
        {
            _logger = logger;
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return this.View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? this.HttpContext.TraceIdentifier });
        }

        public IActionResult Index()
        {
            return this.View(new DefaultViewModel
            {
                Version = DefaultViewModel?.Version,
                InstanceName = DefaultViewModel?.InstanceName,
                ReadOnlyMode = DefaultViewModel?.ReadOnlyMode ?? true
            });
        }

        public IActionResult Privacy()
        {
            return this.View();
        }
    }
}