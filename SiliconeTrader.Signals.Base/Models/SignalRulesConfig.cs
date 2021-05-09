using SiliconeTrader.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace SiliconeTrader.Signals.Base
{
    public class SignalRulesConfig : ISignalRulesConfig
    {
        public RuleProcessingMode ProcessingMode { get; set; }
        public double CheckInterval { get; set; }
    }
}
