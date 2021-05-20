using System;
using System.Collections.Generic;
using System.Text;

namespace SiliconeTrader.Core
{
    public interface IRulesConfig
    {
        IEnumerable<IModuleRules> Modules { get; }
    }
}
