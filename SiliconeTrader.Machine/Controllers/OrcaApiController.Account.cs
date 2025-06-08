using Microsoft.AspNetCore.Mvc;
using SiliconeTrader.Core;
using System.Threading.Tasks; // Added for Task<>

namespace SiliconeTrader.Machine.Controllers
{
    public partial class OrcaApiController
    {
        [HttpPost("account/refresh")]
        public async Task<IActionResult> RefreshAccount() // Made async
        {
            if (!_coreService.Config.ReadOnlyMode)
            {
                await Task.Run(() => _tradingService.Account.Refresh()); // Wrapped in Task.Run
                return Ok();
            }
            else
            {
                return BadRequest();
            }
        }
    }
}