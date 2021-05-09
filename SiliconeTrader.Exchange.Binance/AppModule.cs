using Autofac;
using SiliconeTrader.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace SiliconeTrader.Exchange.Binance
{
    public class AppModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<BinanceExchangeService>().Named<IExchangeService>("Binance").As<IConfigurableService>().Named<IConfigurableService>("ExchangeBinance").SingleInstance().PreserveExistingDefaults();
        }
    }
}
