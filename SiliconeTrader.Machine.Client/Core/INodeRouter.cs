using SiliconeTrader.Core;
using SiliconeTrader.Machine.Client.Core.Abstractions;
using SiliconeTrader.Machine.Client.Models;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SiliconeTrader.Machine.Client.Core
{
    public interface INodeRouter
    { }

    //---------------[internet]----------------//
    public interface INode
    {
        Task Initialize();

        /// <summary>
        /// This is what the router uses for REST calls. It could be the same or a different or a new! 
        /// </summary>
        IRestClient Channel { get; }

        /// <summary>
        /// Last node status reported by <see cref="HealthCheckTimedTask"/>
        /// </summary>
        HealthCheckResult LastNodeHealth { get; }


        /// <summary>
        /// timed task runs every minute, reports the health status of the node.
        /// </summary>
        ITimedTask HealthCheckTimedTask { get; }

        IAuthenticationChannel Authenticator { get; }

        /// <summary>
        /// Default implementation checks the <see cref="LastNodeHealth"/>
        /// If it's reported <see cref="HealthCheckResult.Status"/> == Healthy, invokes the Channel.SendAsync()
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        Task<HttpResponseMessage> SendAsync(HttpRequestMessage requestMessage, CancellationToken cancellationToken);
    }


    public interface IAuthenticationChannel
    {


        void Sign(HttpRequestMessage requestMessage);
    }

    public class BinanceChannel : IAuthenticationChannel
    {
        // Assuming BinanceConfiguration is defined elsewhere
        // public BinanceChannel(BinanceConfiguration configuration)
        // {
        //     // ... existing ChannelBuilder logic ...
        // }
        public void Sign(HttpRequestMessage requestMessage) { /* Implementation */ }
    }


    public class CoinbaseChannel : IAuthenticationChannel
    {
        public void Sign(HttpRequestMessage requestMessage) { /* Implementation */ }
    }
    public class KrakenChannel : IAuthenticationChannel
    {
        public void Sign(HttpRequestMessage requestMessage) { /* Implementation */ }
    }

    public interface IAuthenticator
    {

    }

    public class NodeRouter : INodeRouter
    {
        private readonly INode[] Nodes;

        public NodeRouter(INode[] nodes)
        {
            Nodes = nodes;
            foreach (var node in Nodes)
            {
                node.Initialize().Wait(); // Synchronously wait for initialization
            }
        }

        private Task<HttpResponseMessage> Route(HttpRequestMessage requestMessage, CancellationToken cancellationToken)
        {
            return null;
        }
    }
}