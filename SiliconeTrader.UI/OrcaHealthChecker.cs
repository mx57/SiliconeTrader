using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SiliconeTrader.UI
{
    public class OrcaHealthChecker : IHealthCheck
    {
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var orcaBase = Environment.GetEnvironmentVariable("ORCA_API_URL");
            var httpClient = new HttpClient();

            try
            {
                var response = await httpClient.GetStringAsync($"{orcaBase}api/ORCA/v1/status");

                return HealthCheckResult.Healthy(orcaBase + " | " + response);
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy(ex.Message);
            }

        }
    }
}
