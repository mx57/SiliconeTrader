using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Collections.Generic;

namespace SiliconeTrader.Machine.Client
{
    public class HealthCheckResult : BotResponse
    {
        public Dictionary<string, object> Data { get; set; }

        public string Description { get; set; }

        public HealthStatus Status { get; set; }
    }
}