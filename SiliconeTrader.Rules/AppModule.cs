using Autofac;
using SiliconeTrader.Core;
using System;

namespace SiliconeTrader.Rules
{
    public class AppModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<RulesService>().As<IRulesService>().As<IConfigurableService>().Named<IConfigurableService>(Constants.ServiceNames.RulesService).SingleInstance();
        }
    }
}
