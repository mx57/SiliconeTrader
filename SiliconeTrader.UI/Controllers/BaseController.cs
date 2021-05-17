using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SiliconeTrader.Machine.Client;
using SiliconeTrader.Machine.Client.Models;
using SiliconeTrader.Machine.Client.Models.Responses;
using System.Threading;
using System.Threading.Tasks;

namespace SiliconeTrader.UI.Controllers
{
    public abstract class BaseController : Controller
    {
        // TODO: remove, useless in multi bot scenario
        protected static DefaultViewModel DefaultViewModel;

        protected readonly ITradingBotClient BotClient;

        protected BaseController(ITradingBotClient botClient)
        {
            BotClient = botClient;
            DefaultViewModel = DefaultViewModel.Default;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // TODO: remove this bit of inefficient code.
            if (string.IsNullOrEmpty(DefaultViewModel.InstanceName))
            {
                //InstanceVersionResponse botInstance = await BotClient.Instance.GetVersionInfo(CancellationToken.None);

                DefaultViewModel = new DefaultViewModel
                {
                    InstanceName = "",// botInstance.InstanceName,
                    ReadOnlyMode = true, //botInstance.ReadOnlyMode,
                    Version = "1.1 B", //botInstance.Version
                };
            }

            this.ViewData["Title"] = $"ST:{context.ActionDescriptor.RouteValues["action"]}";
            this.ViewData.Model = DefaultViewModel;

            await base.OnActionExecutionAsync(context, next);
        }

    }
}