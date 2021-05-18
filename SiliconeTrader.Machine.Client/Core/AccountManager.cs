using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using SiliconeTrader.Machine.Client.Core.Abstractions;
using SiliconeTrader.Machine.Client.Models;

namespace SiliconeTrader.Machine.Client.Core
{
    internal class AccountManager : BaseManager, IAccountManager
    {

        private AccountManager(IRestClient restClient, IModelConverter modelConverter)
            : base(restClient, modelConverter)
        {
        }

        public static IAccountManager Create(IRestClient restClient, IModelConverter modelConverter)
        {
            return new AccountManager(restClient, modelConverter);
        }

        public Task Refresh(CancellationToken cancellationToken)
            => this.SendAsync<EmptyResponse>(HttpMethod.Post, "/api/ORCA/v1/account/refresh", cancellationToken);
    }

}