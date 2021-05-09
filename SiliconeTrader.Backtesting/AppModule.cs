using Autofac;
using SiliconeTrader.Core;
using System;

namespace SiliconeTrader.Backtesting
{
    public class AppModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<BacktestingService>().As<IBacktestingService>().As<IConfigurableService>().Named<IConfigurableService>(Constants.ServiceNames.BacktestingService).SingleInstance();

            var backtestingConfig = Application.ConfigProvider.GetSection<BacktestingConfig>(Constants.ServiceNames.BacktestingService);
            if (backtestingConfig.Enabled && backtestingConfig.Replay)
            {
                builder.RegisterType<BacktestingExchangeService>().Named<IExchangeService>(Constants.ServiceNames.BacktestingExchangeService).As<IConfigurableService>().Named<IConfigurableService>(Constants.ServiceNames.BacktestingExchangeService).SingleInstance();
                builder.RegisterType<BacktestingSignalsService>().As<ISignalsService>().As<IConfigurableService>().Named<IConfigurableService>(Constants.ServiceNames.BacktestingSignalsService).SingleInstance();
            }
        }
    }
}
