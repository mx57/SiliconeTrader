using Autofac;
using SiliconeTrader.Core;
using SiliconeTrader.Signals.Base;

namespace SiliconeTrader.Signals.TradingView
{
    public class AppModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<TradingViewCryptoSignalReceiver>().As<ISignalReceiver>().Named<ISignalReceiver>(nameof(TradingViewCryptoSignalReceiver));
        }
    }
}
