using IntelliTrader.Core;
using System.Collections.Generic;

namespace IntelliTrader.Signals.Base
{
    public class SignalsConfig : ISignalsConfig
    {
        /// <summary>
        /// Signal source definitions
        /// </summary>
        public IEnumerable<SignalDefinition> Definitions { get; set; }

        IEnumerable<ISignalDefinition> ISignalsConfig.Definitions => Definitions;

        /// <summary>
        /// Enable / disable signals
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Signals to calculate the Global Rating from
        /// Defaults: ["TV-5m","TV-15m","TV-1h"]
        /// </summary>
        public IEnumerable<string> GlobalRatingSignals { get; set; } = new string[] {
            "TV-5m",
            "TV-15m",
            "TV-1h"
        };
    }
}