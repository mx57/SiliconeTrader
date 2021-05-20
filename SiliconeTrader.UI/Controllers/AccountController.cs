using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SiliconeTrader.Machine.Client;

namespace SiliconeTrader.UI.Controllers
{
    public class AccountController : BaseController
    {
        private readonly ILogger<DashboardController> _logger;

        public AccountController(ITradingBotClient botClient, ILogger<DashboardController> logger)
            : base(botClient)
        {
            _logger = logger;
        }


        public IActionResult Logout()
        {
            return this.View();
        }

        [HttpPost]
        public async Task<IActionResult> Refresh()
        {
            await BotClient.Account.Refresh(CancellationToken.None);

            return this.Ok();
        }

    }
}