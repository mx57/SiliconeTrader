using Microsoft.AspNetCore.Mvc;
using SiliconeTrader.Core;
using SiliconeTrader.Machine.Client.Models; // Assuming request DTOs are here
using SiliconeTrader.Core.Models; // For BotRequest if needed by request DTOs
using System.Threading.Tasks;

namespace SiliconeTrader.Machine.Controllers
{
    public partial class OrcaApiController
    {
        [HttpPost("buy")]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(422)]
        public async Task<IActionResult> Buy([FromBody] BuyRequest buyRequest)
        {
            if (_coreService.Config.ReadOnlyMode)
            {
                return BadRequest("Read-only mode enabled.");
            }

            if (buyRequest == null || string.IsNullOrWhiteSpace(buyRequest.Pair) || buyRequest.Amount <= 0)
            {
                return BadRequest("Invalid buy request.");
            }

            await Task.Run(() => _tradingService.Buy(new BuyOptions(buyRequest.Pair)
            {
                Amount = buyRequest.Amount,
                IgnoreExisting = true,
                ManualOrder = true
            }));

            return Ok();
        }

        [HttpPost("buy-default")]
        public async Task<IActionResult> BuyDefault([FromBody] BuyRequest buyRequest)
        {
            if (_coreService.Config.ReadOnlyMode)
            {
                return BadRequest("Read-only mode enabled.");
            }

            if (buyRequest == null || string.IsNullOrWhiteSpace(buyRequest.Pair))
            {
                return BadRequest("Invalid buy request.");
            }

            await Task.Run(() => _tradingService.Buy(new BuyOptions(buyRequest.Pair)
            {
                MaxCost = _tradingService.GetPairConfig(buyRequest.Pair).BuyMaxCost,
                IgnoreExisting = true,
                ManualOrder = true,
                Metadata = new OrderMetadata
                {
                    BoughtGlobalRating = _signalsService.GetGlobalRating()
                }
            }));

            return Ok();
        }

        [HttpPost("sell")]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(422)]
        public async Task<IActionResult> Sell([FromBody] SellRequest sellRequest)
        {
            if (_coreService.Config.ReadOnlyMode)
            {
                return BadRequest("Read-only mode enabled.");
            }

            if (sellRequest == null || string.IsNullOrWhiteSpace(sellRequest.Pair) || sellRequest.Amount <= 0)
            {
                return BadRequest("Invalid sell request.");
            }

            await Task.Run(() => _tradingService.Sell(new SellOptions(sellRequest.Pair)
            {
                Amount = sellRequest.Amount,
                ManualOrder = true
            }));

            return Ok();
        }

        [HttpPost("swap")]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(422)]
        public async Task<IActionResult> Swap([FromBody] SwapRequest swapRequest)
        {
            if (_coreService.Config.ReadOnlyMode)
            {
                return BadRequest("Read-only mode enabled.");
            }

            if (swapRequest == null || string.IsNullOrWhiteSpace(swapRequest.Pair) || string.IsNullOrWhiteSpace(swapRequest.Swap))
            {
                 return BadRequest("Invalid swap request.");
            }

            await Task.Run(() => _tradingService.Swap(new SwapOptions(swapRequest.Pair, swapRequest.Swap, new OrderMetadata())
            {
                ManualOrder = true
            }));

            return Ok();
        }
    }
}