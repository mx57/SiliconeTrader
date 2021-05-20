using Autofac;
using SiliconeTrader.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace SiliconeTrader.Trading
{
    public class AppModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<TradingService>().As<ITradingService>().As<IConfigurableService>().Named<IConfigurableService>(Constants.ServiceNames.TradingService).SingleInstance();
            builder.RegisterType<OrderingService>().As<IOrderingService>().SingleInstance();
        }
    }
}
