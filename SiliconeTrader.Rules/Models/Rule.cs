﻿using SiliconeTrader.Core;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace SiliconeTrader.Rules
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

        IEnumerable<IRuleCondition> IRule.Conditions => this.Conditions;
        IRuleTrailing IRule.Trailing => this.Trailing;

        private object typedModifiersCached;

        public T GetModifiers<T>()
        {
            if (typedModifiersCached == null)
            {
                typedModifiersCached = this.Modifiers.Get<T>();
            }
            return (T)typedModifiersCached;
        }
    }
}
