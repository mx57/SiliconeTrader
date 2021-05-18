using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SiliconeTrader.Machine.Client;
using SiliconeTrader.Machine.Client.Models;
using SiliconeTrader.UI.Models;

namespace SiliconeTrader.UI.Controllers
{
    public class OrdersController : BaseController
    {
        private readonly ILogger<DashboardController> _logger;

        public OrdersController(ITradingBotClient botClient, ILogger<DashboardController> logger)
            : base(botClient)
        {
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Buy(OrderModel order)
        {
            await BotClient.Orders.Buy(new BuyRequest
            {
                Amount = order.Amount,
                Pair = order.Pair
            }, CancellationToken.None);

            return this.Ok();
        }

        [HttpPost]
        public async Task<IActionResult> BuyDefault(OrderModel order)
        {
            await BotClient.Orders.BuyDefault(new BuyRequest

            {
                Amount = order.Amount,
                Pair = order.Pair
            }, CancellationToken.None);

            return this.Ok();
        }

        [HttpPost]
        public async Task<IActionResult> Sell(OrderModel order)
        {
            await BotClient.Orders.Sell(new SellRequest
            {
                Amount = order.Amount,
                Pair = order.Pair
            }, CancellationToken.None);

            return this.Ok();
        }

        [HttpPost]
        public async Task<IActionResult> Swap(OrderModel order)
        {
            await BotClient.Orders.Swap(new SwapRequest
            {
                Swap = order.Swap,
                Pair = order.Pair
            }, CancellationToken.None);

            return this.Ok();
        }
    }
}