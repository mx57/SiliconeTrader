using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using SiliconeTrader.Machine.Client.Models.Responses;

namespace SiliconeTrader.Machine.Client.Core
{
    public interface IInstanceManager
    {
        Task<InstanceVersionResponse> GetVersionInfo(CancellationToken cancellationToken);
    }

    internal class InstanceManager : BaseManager, IInstanceManager
    {
        private InstanceManager(IRestClient restClient, IModelConverter modelConverter)
             : base(restClient, modelConverter)
        {
        }

        public static IInstanceManager Create(IRestClient restClient, IModelConverter modelConverter)
        {
            return new InstanceManager(restClient, modelConverter);
        }


        public Task<InstanceVersionResponse> GetVersionInfo(CancellationToken cancellationToken)
           => this.SendAsync<InstanceVersionResponse>(HttpMethod.Get, "/api/ORCA/v1/instance", cancellationToken);
    }
}