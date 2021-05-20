using Microsoft.AspNetCore.Mvc;
using SiliconeTrader.Core;
using SiliconeTrader.Machine.Client.Models;

namespace SiliconeTrader.Machine.Controllers
{
    public partial class OrcaApiController
    {
        [HttpPost("buy")]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(422)]
        public IActionResult Buy(BuyRequest buyRequest)
        {
            if (Application.Resolve<ICoreService>().Config.ReadOnlyMode)
            {
                return new BadRequestResult();
            }

            if (!string.IsNullOrWhiteSpace(buyRequest.Pair) && buyRequest.Amount > 0)
            {
                ITradingService tradingService = Application.Resolve<ITradingService>();

                tradingService.Buy(new BuyOptions(buyRequest.Pair)
                {
                    Amount = buyRequest.Amount,
                    IgnoreExisting = true,
                    ManualOrder = true
                });

                return new OkResult();
            }

            return new BadRequestResult();
        }

        [HttpPost("buy-default")]
        public IActionResult BuyDefault(BuyRequest buyRequest)
        {
            if (Application.Resolve<ICoreService>().Config.ReadOnlyMode)
            {
                return new BadRequestResult();
            }

            if (!string.IsNullOrWhiteSpace(buyRequest.Pair))
            {
                ISignalsService signalsService = Application.Resolve<ISignalsService>();
                ITradingService tradingService = Application.Resolve<ITradingService>();

                tradingService.Buy(new BuyOptions(buyRequest.Pair)
                {
                    MaxCost = tradingService.GetPairConfig(buyRequest.Pair).BuyMaxCost,
                    IgnoreExisting = true,
                    ManualOrder = true,
                    Metadata = new OrderMetadata
                    {
                        BoughtGlobalRating = signalsService.GetGlobalRating()
                    }
                });

                return new OkResult();
            }
            else
            {
                return new BadRequestResult();
            }
        }

        [HttpPost("sell")]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(422)]
        public IActionResult Sell(SellRequest sellRequest)
        {
            if (Application.Resolve<ICoreService>().Config.ReadOnlyMode)
            {
                return new BadRequestResult();
            }

            if (!string.IsNullOrWhiteSpace(sellRequest.Pair) && sellRequest.Amount > 0)
            {
                ITradingService tradingService = Application.Resolve<ITradingService>();

                tradingService.Sell(new SellOptions(sellRequest.Pair)
                {
                    Amount = sellRequest.Amount,
                    ManualOrder = true
                });

                return new OkResult();
            }

            return new BadRequestResult();
        }

        [HttpPost("swap")]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(422)]
        public IActionResult Swap(SwapRequest swapRequest)
        {
            if (!Application.Resolve<ICoreService>().Config.ReadOnlyMode
                && !string.IsNullOrWhiteSpace(swapRequest.Pair)
                && !string.IsNullOrWhiteSpace(swapRequest.Swap))
            {
                ITradingService tradingService = Application.Resolve<ITradingService>();

                tradingService.Swap(new SwapOptions(swapRequest.Pair, swapRequest.Swap, new OrderMetadata())
                {
                    ManualOrder = true
                });

                return new OkResult();
            }
            else
            {
                return new BadRequestResult();
            }
        }
    }
}