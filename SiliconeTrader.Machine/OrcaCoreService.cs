using Microsoft.Extensions.Hosting;
using SiliconeTrader.Core;
using System.Threading;
using System.Threading.Tasks;

namespace SiliconeTrader.Machine
{
    internal class OrcaCoreService : BackgroundService
    {
        private readonly ICoreService _coreService;

        public OrcaCoreService()
        {
            // from here, everything Autofac.
            // IntelliTrader code:
            // TODO: IServiceCollection.
            _coreService = Application.Resolve<ICoreService>();
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _coreService.Start();

            return Task.CompletedTask;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            // Stops all trading services.
            _coreService.Stop();

            return base.StopAsync(cancellationToken);
        }
    }
}