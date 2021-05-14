using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SiliconeTrader.Machine.Client
{
    public interface IRestClient
    {
        Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken);
    }
}