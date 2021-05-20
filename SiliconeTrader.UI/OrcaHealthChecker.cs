using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SiliconeTrader.UI
{
    public class OrcaHealthChecker : IHealthCheck
    {
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            string orcaBase = Environment.GetEnvironmentVariable("ORCA_API_URL");
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri(orcaBase)
            };

            try
            {
                string response = await httpClient.GetStringAsync("api/ORCA/v1/status");

                return HealthCheckResult.Healthy(orcaBase + " | " + response);
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy(ex.Message);
            }

        }
    }
}
