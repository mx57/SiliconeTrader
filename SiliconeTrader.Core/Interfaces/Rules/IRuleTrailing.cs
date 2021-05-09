using System;
using System.Collections.Generic;
using System.Text;

namespace SiliconeTrader.Core
{
    public interface IRuleTrailing
    {
        bool Enabled { get; }
        int MinDuration { get; }
        int MaxDuration { get; }
        IEnumerable<IRuleCondition> StartConditions { get; }
    }
}
