using Microsoft.AspNetCore.Mvc;
using SiliconeTrader.Core;

namespace SiliconeTrader.Machine.Controllers
{
    public partial class OrcaApiController
    {
        [HttpPost("account/refresh")]
        public IActionResult RefreshAccount()
        {
            if (!Application.Resolve<ICoreService>().Config.ReadOnlyMode)
            {
                ITradingService tradingService = Application.Resolve<ITradingService>();
                tradingService.Account.Refresh();
                return new OkResult();
            }
            else
            {
                return new BadRequestResult();
            }
        }
    }
}