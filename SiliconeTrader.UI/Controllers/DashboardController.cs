using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SiliconeTrader.Machine.Client;
using SiliconeTrader.Core.Models; // For DefaultViewModel
using SiliconeTrader.Machine.Client.Models.Responses; // For InstanceVersionResponse

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

        public async Task<IActionResult> Index()
        {
            InstanceVersionResponse instance = await BotClient.Instance.GetVersionInfo(CancellationToken.None);

            return this.View(new DefaultViewModel
            {
                InstanceName = instance.InstanceName,
                ReadOnlyMode = instance.ReadOnlyMode,
                Version = instance.Version
            });
        }
    }
}