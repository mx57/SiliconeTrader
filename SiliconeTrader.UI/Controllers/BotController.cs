using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SiliconeTrader.Machine.Client;
using SiliconeTrader.Machine.Client.Models;
using SiliconeTrader.UI.Models;

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

        public async Task<IActionResult> Log()
        {
            LogViewModel log = await BotClient.Instance.GetLog(CancellationToken.None);

            return this.View(log);
        }

        public async Task<IActionResult> Rules()
        {
            RulesViewModel rules = await BotClient.Instance.GetRules(CancellationToken.None);

            return this.View(rules);
        }

        [HttpPost]
        public async Task<IActionResult> SaveConfig(UpdateConfigModel model)
        {
            await BotClient.Instance.SaveConfig(model.Name, model.Definition, CancellationToken.None);

            return this.Ok();
        }


        public async Task<IActionResult> Settings()
        {
            SettingsViewModel settings = await BotClient.Instance.GetSettings(CancellationToken.None);

            return this.View(settings);
        }

        [HttpPost]
        public async Task<IActionResult> Settings(SaveSettingsRequest settings)
        {
            SettingsViewModel savedSettings = await BotClient.Instance.SaveSettings(settings, CancellationToken.None);

            return this.View(savedSettings);
        }

        public async Task<IActionResult> Stats()
        {
            StatsViewModel stats = await BotClient.Instance.GetStats(CancellationToken.None);

            return this.View(stats);
        }

        public IActionResult Status()
        {
            return this.View();
        }

        [HttpPost]
        public async Task<IActionResult> Reset()
        {
            await BotClient.Instance.RestartServices(CancellationToken.None);

            return this.Ok();
        }
    }
}