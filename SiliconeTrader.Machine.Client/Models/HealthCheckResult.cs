using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Collections.Generic;
using SiliconeTrader.Core.Models; // Added for BotResponse

namespace SiliconeTrader.Machine.Client.Models
{
    public class HealthCheckResult : BotResponse
    {
        public Dictionary<string, object> Data { get; set; }

        public string Description { get; set; }

        public HealthStatus Status { get; set; }
    }
}