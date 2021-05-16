using SiliconeTrader.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace SiliconeTrader.Rules
{
    internal class RulesConfig : IRulesConfig
    {
        /// <summary>
        /// Rule Modules
        /// </summary>
        public IEnumerable<ModuleRules> Modules { get; set; }
        IEnumerable<IModuleRules> IRulesConfig.Modules => this.Modules;
    }
}
