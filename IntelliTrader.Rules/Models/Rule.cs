using IntelliTrader.Core;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace IntelliTrader.Rules
{
    internal class Rule : IRule
    {
        /// <summary>
        /// Enable / disable rule
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Rule name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Rule Action
        /// </summary>
        public RuleAction Action { get; set; }

        /// <summary>
        /// Rule conditions
        /// </summary>
        public IEnumerable<RuleCondition> Conditions { get; set; }

        /// <summary>
        /// Rule trailing
        /// </summary>
        public RuleTrailing Trailing { get; set; }

        /// <summary>
        /// Rule modifiers
        /// </summary>
        public IConfigurationSection Modifiers { get; set; }

        IEnumerable<IRuleCondition> IRule.Conditions => Conditions;
        IRuleTrailing IRule.Trailing => Trailing;

        private object typedModifiersCached;

        public T GetModifiers<T>()
        {
            if (typedModifiersCached == null)
            {
                typedModifiersCached = Modifiers.Get<T>();
            }
            return (T)typedModifiersCached;
        }
    }
}
