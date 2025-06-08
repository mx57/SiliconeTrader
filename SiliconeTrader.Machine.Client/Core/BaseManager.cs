using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SiliconeTrader.Machine.Client.Core.Abstractions;
using SiliconeTrader.Core.Models; // For BotRequest, BotResponse, ErrorResponse

namespace SiliconeTrader.Machine.Client.Core
{
    internal abstract class BaseManager
    {
        protected const string MediaType = "application/json";

        protected readonly IModelConverter ModelConverter;
        protected readonly IRestClient RestClient;

        protected BaseManager(IRestClient restClient, IModelConverter modelConverter)
        {
            RestClient = restClient;
            ModelConverter = modelConverter;
        }

        /// <summary>
        /// Sends a HTTP GET request with query params attached to the query. There is no need to include '?query-data' in the uri
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="queryParams"></param>
        /// <param name="uri"></param>
        /// <param name="requiredPermissions"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected async Task<TResult> GetAsync<TRequest, TResult>(TRequest queryParams, string uri, CancellationToken cancellationToken)
            where TRequest : BotRequest
            where TResult : BotResponse, new()
        {
            string requestQuery = ModelConverter.ToHttpQuery(queryParams);

            // if the query char (?) included, then don't add it again.
            string requestUri = uri.Contains('?') ? uri + requestQuery : $"{uri}?{requestQuery}";

            using (var request = new HttpRequestMessage(HttpMethod.Get, requestUri))
            {
                request.Content = new StringContent("", Encoding.UTF8, MediaType);

                return await this.HandleResponse<TResult>(request, cancellationToken).ConfigureAwait(false);
            }
        }

        protected async Task<ErrorResponse> HandleTransientErrorResponse(HttpResponseMessage response)
        {
            if (DefaultSettings.TransientErrors.Contains(response.StatusCode))
            {
                throw new TransientException(response.StatusCode);
            }

            string result = await response.Content?.ReadAsStringAsync();

            if (string.IsNullOrEmpty(result))
            {
                return new ErrorResponse { ErrorMessage = response.StatusCode.ToString() };
            }

            if (!response.IsSuccessStatusCode)
            {
                try
                {
                    // if an error causes HTML to be returned, deserialization fails.
                    return ModelConverter.Deserialize<ErrorResponse>(result);
                }
                catch (Exception)
                {
                    return ErrorResponse.FromError(result);
                }
            }

            return null;
        }

        protected async Task<TResult> SendAsync<TResult>(HttpMethod httpMethod, string uri, CancellationToken cancellationToken)
            where TResult : BotResponse, new()
        {
            using (var request = new HttpRequestMessage(httpMethod, uri))
            {
                if (httpMethod != HttpMethod.Get)
                {
                    request.Content = new StringContent("", Encoding.UTF8, MediaType);
                }

                return await this.HandleResponse<TResult>(request, cancellationToken).ConfigureAwait(false);
            }
        }

        protected async Task<TResult> SendAsync<TRequest, TResult>(TRequest requestObj, HttpMethod httpMethod, string uri, CancellationToken cancellationToken)
            where TRequest : BotRequest
            where TResult : BotResponse, new()
        {
            string requestContent = ModelConverter.Serialize(requestObj);

            using (var request = new HttpRequestMessage(httpMethod, uri))
            {
                request.Content = new StringContent(requestContent, Encoding.UTF8, MediaType);

                return await this.HandleResponse<TResult>(request, cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task<TResult> HandleResponse<TResult>(HttpRequestMessage request, CancellationToken cancellationToken)
            where TResult : BotResponse, new()
        {
            using (HttpResponseMessage response = await RestClient.SendAsync(request, cancellationToken).ConfigureAwait(false))
            {
                ErrorResponse error = await this.HandleTransientErrorResponse(response);
                TResult result;

                if (error == null)
                {
                    string responseJson = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                    result = ModelConverter.Deserialize<TResult>(responseJson);
                }
                else
                {
                    result = new TResult
                    {
                        Error = error
                    };
                }

                return result;
            }
        }
    }
}