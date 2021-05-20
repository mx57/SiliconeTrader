using System.Threading;
using System.Threading.Tasks;
using SiliconeTrader.Machine.Client.Models;
using SiliconeTrader.Machine.Client.Models.Responses;

namespace SiliconeTrader.Machine.Client.Core.Abstractions
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
}