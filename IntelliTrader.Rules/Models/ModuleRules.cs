using IntelliTrader.Core;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace IntelliTrader.Rules
{
    internal class ModuleRules : IModuleRules
    {

        /// <summary>
        /// Module name
        /// </summary>
        public string Module { get; set; }

        /// <summary>
        /// Module configuration
        /// </summary>
        public IConfigurationSection Configuration { get; set; }

        /// <summary>
        /// Rules configuration
        /// </summary>
        public IEnumerable<Rule> Entries { get; set; }
        IEnumerable<IRule> IModuleRules.Entries => Entries;

        public T GetConfiguration<T>()
        {
            return Configuration.Get<T>();
        }
    }
}