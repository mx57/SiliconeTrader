using SiliconeTrader.Core;
using System.Collections.Generic;

namespace SiliconeTrader.Machine.Client.Models
{
    public class StatusApiModel
    {
        public decimal Balance { get; set; }

        public string GlobalRating { get; set; }

        public IEnumerable<IHealthCheck> HealthChecks { get; set; }

        public IEnumerable<string> LogEntries { get; set; }

        public bool TradingSuspended { get; set; }

        public List<string> TrailingBuys { get; set; }

        public List<string> TrailingSells { get; set; }

        public List<string> TrailingSignals { get; set; }
    }
}