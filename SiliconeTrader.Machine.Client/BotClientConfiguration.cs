using System;
using System.Collections.Generic;

namespace SiliconeTrader.Machine.Client
{
    public class BotClientConfiguration
    {
        public List<BotConfiguration> BotClients { get; set; }

        internal void EnsureValidConfiguration()
        {
            if (BotClients == null || BotClients.Count == 0)
            {
                throw new ArgumentException(nameof(BotClients));
            }
        }
    }

    public class BotConfiguration
    {
        public string Name { get; set; }
        public string Uri { get; set; }
    }
}