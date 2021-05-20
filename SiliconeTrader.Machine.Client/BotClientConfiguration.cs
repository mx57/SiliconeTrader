using System;
using System.Collections.Generic;

namespace SiliconeTrader.Machine.Client
{
    public class BotClientConfiguration
    {
        public List<BotConfiguration> BotClients { get; set; }

        internal void EnsureValidConfiguration()
        {
            if (this.BotClients == null || this.BotClients.Count == 0)
            {
                throw new ArgumentException(nameof(this.BotClients));
            }
        }
    }

    public class BotConfiguration
    {
        public string Name { get; set; }
        public string Uri { get; set; }
    }
}