using SiliconeTrader.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SiliconeTrader.Machine.Models
{
    public class StatusApiModel
    {
        public decimal Balance { get; internal set; }
        public string GlobalRating { get; internal set; }
        public List<string> TrailingBuys { get; internal set; }
        public List<string> TrailingSells { get; internal set; }
        public List<string> TrailingSignals { get; internal set; }
        public bool TradingSuspended { get; internal set; }
        public IEnumerable<IHealthCheck> HealthChecks { get; internal set; }
        public IEnumerable<string> LogEntries { get; internal set; }
    }
}
