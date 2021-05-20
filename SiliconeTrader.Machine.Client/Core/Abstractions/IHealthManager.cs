using System.Threading;
using System.Threading.Tasks;
using SiliconeTrader.Machine.Client.Models;

namespace SiliconeTrader.Machine.Client.Core.Abstractions
{
    public interface IHealthManager
    {
        Task<HealthCheckResult> NodeStatus(CancellationToken cancellationToken);
    }
}