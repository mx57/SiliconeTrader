using SiliconeTrader.Core;
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
        async Task Initialize()
        {
            // check health, make sure it works
            HttpResponseMessage response = await this.Channel.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/health"), CancellationToken.None);

            response.EnsureSuccessStatusCode();

            // start the health check timer
            this.HealthCheckTimedTask.Start();
        }

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
        Task<HttpResponseMessage> SendAsync(HttpRequestMessage requestMessage, CancellationToken cancellationToken)
        {
            //Analytics.Report(NodeId, "SEND.URI");

            if (this.LastNodeHealth.Status == Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Healthy)
            {
                this.Authenticator.Sign(requestMessage);

                Task<HttpResponseMessage> response = this.Channel.SendAsync(requestMessage, cancellationToken);

                //if( ! response.Authenticate() )
                //     Analytics.Report(NodeId, "AUTH_FAIL");

                //Analytics.Report(NodeId, "RESPONSE.URI");
                return response;
            }

            //Analytics.Report(NodeId, "NodeProblem");

            return null;
        }
    }


    public interface IAuthenticationChannel
    {


        void Sign(HttpRequestMessage requestMessage)
        {
            //ChannelBuilder.Build().Process(requestMessage);
            // will go thru each channel processor item, add whatever value to headers, sign messages,
            // and then returns the auth headers for API requests added to HttpRequestMessage
        }
    }

    //public class BinanceChannel : IAuthenticationChannel
    //{
    //    public BinanceChannel(BinanceConfiguration configuration)
    //    {
    //        ChannelBuilder
    //            .New(configuration)
    //            .Timestamp(header: true, key: "X-TIMESTAMP")
    //            .HmacSigner(secret: "BinanceSecret")
    //            .ApiKey(header: true, key: "X-API-KEY")
    //            .AuthKey(header: true, key:  "X-BIN-AUTH");

    //    }
    //}


    public class CoinbaseChannel : IAuthenticationChannel
    {

    }
    public class KrakenChannel : IAuthenticationChannel
    {

    }

    public interface IAuthenticator
    {

    }

    public class NodeRouter : INodeRouter
    {
        private INode[] Nodes;

        private Task<HttpResponseMessage> Route(HttpRequestMessage requestMessage, CancellationToken cancellationToken)
        {
            return null;
        }
    }
}