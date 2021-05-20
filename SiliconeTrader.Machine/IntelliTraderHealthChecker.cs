using Microsoft.Extensions.Diagnostics.HealthChecks;
using SiliconeTrader.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SiliconeTrader.Machine
{
    public class IntelliTraderHealthChecker : Microsoft.Extensions.Diagnostics.HealthChecks.IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            IHealthCheckService healthCheckService = Application.Resolve<IHealthCheckService>();

            IEnumerable<Core.IHealthCheck> checks = healthCheckService.GetHealthChecks();
            IReadOnlyDictionary<string, object> dic = checks.ToDictionary(x => x.Name, v => (object)v);

            if (checks.Any(x => x.Failed))
            {
                return Task.FromResult(
                    HealthCheckResult.Degraded(
                        "WARNING",
                        data: dic));
            }

            return Task.FromResult(
                HealthCheckResult.Healthy(
                    "OK",
                    data: dic));
        }
    }
}
