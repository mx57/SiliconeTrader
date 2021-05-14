using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;

namespace SiliconeTrader.Machine.Client
{
    public static class ServiceCollectionExtensions
    {
        private static readonly Random _jitterer = new Random();

        public static IHttpClientBuilder AddTradingBotClient<TRestClient, TModelConverter>(
            this IServiceCollection services, IConfiguration configuration)
                where TRestClient : class, IRestClient
                where TModelConverter : class, IModelConverter
        {
            services.Configure<BotClientConfiguration>(configuration.GetSection(nameof(BotClientConfiguration)));

            services
                .AddSingleton<IModelConverter, TModelConverter>()
                .AddSingleton<ITradingBotClient, TradingBotClient>();

            // to make sure we have configured everything correctly during app startup.
            IOptions<BotClientConfiguration> botClientConfig = services.BuildServiceProvider().GetRequiredService<IOptions<BotClientConfiguration>>();

            if (botClientConfig.Value == null)
            {
                throw new ConfigurationException("Bot Client Configuration not found");
            }

            botClientConfig.Value.EnsureValidConfiguration();

            string baseAddress = botClientConfig.Value.BotClients.Single().Uri;

            return services.AddHttpClient<IRestClient, TRestClient>(client =>
            {
                client.BaseAddress = new Uri(baseAddress);

                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            })
            .AddPolicyHandler(GetRetryPolicy());
        }

        private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(2,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) + TimeSpan.FromMilliseconds(_jitterer.Next(0, 100)));
        }
    }
}