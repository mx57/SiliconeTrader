using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using SiliconeTrader.Machine.Client.Models;
using SiliconeTrader.Machine.Client.Models.Responses;

namespace SiliconeTrader.Machine.Client.Core
{
    public interface IInstanceManager
    {
        Task<LogViewModel> GetLog(CancellationToken cancellationToken);

        Task<RulesViewModel> GetRules(CancellationToken cancellationToken);

        Task<SettingsViewModel> GetSettings(CancellationToken cancellationToken);

        Task<StatsViewModel> GetStats(CancellationToken cancellationToken);

        Task<InstanceVersionResponse> GetVersionInfo(CancellationToken cancellationToken);

        Task RestartServices(CancellationToken cancellationToken);

        Task SaveConfig(string name, string definition, CancellationToken cancellationToken);

        Task<SettingsViewModel> SaveSettings(SaveSettingsRequest settings, CancellationToken cancellationToken);
    }

    public class SaveConfigRequest : BotRequest
    {
        public string ConfigDefinition { get; set; }

        public string Name { get; set; }
    }

    internal class EmptyResponse : BotResponse
    {
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

        public Task<LogViewModel> GetLog(CancellationToken cancellationToken)
            => this.SendAsync<LogViewModel>(HttpMethod.Get, "/api/ORCA/v1/log", cancellationToken);

        public Task<RulesViewModel> GetRules(CancellationToken cancellationToken)
            => this.SendAsync<RulesViewModel>(HttpMethod.Get, "/api/ORCA/v1/rules", cancellationToken);

        public Task<SettingsViewModel> GetSettings(CancellationToken cancellationToken)
            => this.SendAsync<SettingsViewModel>(HttpMethod.Get, "/api/ORCA/v1/settings", cancellationToken);

        public Task<StatsViewModel> GetStats(CancellationToken cancellationToken)
            => this.SendAsync<StatsViewModel>(HttpMethod.Get, "/api/ORCA/v1/stats", cancellationToken);

        public Task<InstanceVersionResponse> GetVersionInfo(CancellationToken cancellationToken)
            => this.SendAsync<InstanceVersionResponse>(HttpMethod.Get, "/api/ORCA/v1/instance", cancellationToken);

        public Task RestartServices(CancellationToken cancellationToken)
            => this.SendAsync<EmptyResponse>(HttpMethod.Post, "/api/ORCA/v1/services/restart", cancellationToken);

        public Task SaveConfig(string name, string definition, CancellationToken cancellationToken)
            => this.SendAsync<SaveConfigRequest, EmptyResponse>(
                    new SaveConfigRequest
                    {
                        Name = name,
                        ConfigDefinition = definition
                    },
                    HttpMethod.Post,
                    $"/api/ORCA/v1/config/{name}/save",
                    cancellationToken);

        public Task<SettingsViewModel> SaveSettings(SaveSettingsRequest settings, CancellationToken cancellationToken)
            => this.SendAsync<SaveSettingsRequest, SettingsViewModel>(
                    settings,
                    HttpMethod.Post,
                    $"/api/ORCA/v1/settings",
                    cancellationToken);
    }
}