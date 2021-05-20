using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Text;

namespace SiliconeTrader.Core
{
    public abstract class ConfigrableServiceBase<TConfig> : IConfigurableService
        where TConfig : class
    {
        private const double DELAY_BETWEEN_CONFIG_RELOADS_MILLISECONDS = 500;

        public abstract string ServiceName { get; }

        public TConfig Config
        {
            get
            {
                lock (syncRoot)
                {
                    if (config == null)
                    {
                        config = this.RawConfig.Get<TConfig>();
                        this.PrepareConfig();
                    }
                    return config;
                }
            }
        }

        public IConfigurationSection RawConfig
        {
            get
            {
                lock (syncRoot)
                {
                    if (rawConfig == null)
                    {
                        rawConfig = Application.ConfigProvider.GetSection(this.ServiceName, this.OnRawConfigChanged);
                    }
                    return rawConfig;
                }
            }
        }

        private TConfig config;
        private IConfigurationSection rawConfig;
        private DateTimeOffset lastReloadDate;
        private object syncRoot = new object();

        protected virtual void PrepareConfig() { }
        protected virtual void OnConfigReloaded() { }

        private void OnRawConfigChanged(IConfigurationSection changedRawConfig)
        {
            lock (syncRoot)
            {
                rawConfig = changedRawConfig;
                config = null;
            }

            if ((DateTimeOffset.Now - lastReloadDate).TotalMilliseconds > DELAY_BETWEEN_CONFIG_RELOADS_MILLISECONDS)
            {
                lastReloadDate = DateTimeOffset.Now;
                this.PrepareConfig();
                this.OnConfigReloaded();
                Application.Resolve<ILoggingService>().Info($"{this.ServiceName} configuration reloaded");
            }
        }
    }
}
