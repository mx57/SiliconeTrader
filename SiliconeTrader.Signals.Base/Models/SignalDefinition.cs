using SiliconeTrader.Core;
using Microsoft.Extensions.Configuration;

namespace SiliconeTrader.Signals.Base
{
    public class SignalDefinition : ISignalDefinition
    {
        /// <summary>
        /// Signal receiver configuration
        /// </summary>
        public IConfigurationSection Configuration { get; set; }

        /// <summary>
        /// Signal name
        /// </summary>
        public string Name { get; set; } = "TV-15m";

        /// <summary>
        /// Signal receiver name
        /// </summary>
        public string Receiver { get; set; } = "TradingViewCryptoSignalReceiver";
    }
}