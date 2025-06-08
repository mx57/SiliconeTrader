using Microsoft.Extensions.Logging;
using SiliconeTrader.Machine.Client.Core.Abstractions;
using SiliconeTrader.Core.Models; // For ErrorResponse
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SiliconeTrader.Machine.Client.Core
{
    public class RestClient : IRestClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<RestClient> _logger;
        private readonly IModelConverter _modelConverter;

        public RestClient(HttpClient httpClient, ILogger<RestClient> logger, IModelConverter modelConverter)
        {
            _httpClient = httpClient;
            _logger = logger;
            _modelConverter = modelConverter;
        }

        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                string result = await response.Content?.ReadAsStringAsync();

                if (!string.IsNullOrEmpty(result))
                {
                    ErrorResponse error;
                    try
                    {
                        // if BitGo throws a 404, then the content is not JSON, it's HTML.
                        // which makes deserialization fail
                        error = _modelConverter.Deserialize<ErrorResponse>(result);
                    }
                    catch (System.Exception)
                    {
                        error = ErrorResponse.FromError(result);
                    }

                    _logger.LogError("An HTTP {errorCode} error occurred during BitGo Request: {@error}", response.StatusCode, error);
                }
                else
                {
                    _logger.LogError("An HTTP {errorCode} error occurred during BitGo Request: {@error}", response.StatusCode, response.ReasonPhrase);
                }
            }

            return response;
        }
    }
}