using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SiliconeTrader.Machine.Client.Core
{
    public interface IHealthManager
    {
        Task<HealthCheckResult> NodeStatus(CancellationToken cancellationToken);
    }

    internal class HealthManager : BaseManager, IHealthManager
    {
        private HealthManager(IRestClient restClient, IModelConverter modelConverter)
            : base(restClient, modelConverter)
        {
        }

        public static IHealthManager Create(IRestClient restClient, IModelConverter modelConverter)
        {
            return new HealthManager(restClient, modelConverter);
        }

        public Task<HealthCheckResult> NodeStatus(CancellationToken cancellationToken)
            => this.SendAsync<HealthCheckResult>(
                    HttpMethod.Get,
                    "/node-status",
                    cancellationToken);
    }
}