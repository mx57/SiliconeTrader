using System;
using System.Collections.Generic;
using System.Text;

namespace SiliconeTrader.Core
{
    public interface ISignalRulesConfig
    {
        RuleProcessingMode ProcessingMode { get; }
        double CheckInterval { get; }
    }
}
