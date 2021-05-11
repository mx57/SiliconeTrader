using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using System;
using System.IO;

namespace SiliconeTrader.Core
{
    internal class ConfigProvider : IConfigProvider
    {
        private const string ROOT_CONFIG_DIR = "Config";
        private const string PATHS_CONFIG_PATH = "paths.json";
        private const string PATHS_SECTION_NAME = "Paths";
        private IConfigurationSection paths;

        public ConfigProvider()
        {
            IConfigurationRoot pathsConfig = GetConfig(PATHS_CONFIG_PATH, changedPathsConfig =>
            {
                paths = changedPathsConfig.GetSection(PATHS_SECTION_NAME);
            });
            paths = pathsConfig.GetSection(PATHS_SECTION_NAME);
        }

        public string GetSectionJson(string sectionName)
        {
            try
            {
                string configPath = paths.GetValue<string>(sectionName);
                var fullConfigPath = Path.Combine(Directory.GetCurrentDirectory(), ROOT_CONFIG_DIR, configPath);
                return File.ReadAllText(fullConfigPath);
            }
            catch (Exception ex)
            {
                Application.Resolve<ILoggingService>().Error($"Unable to load config section {sectionName}", ex);
                return null;
            }
        }

        public void SetSectionJson(string sectionName, string definition)
        {
            try
            {
                string configPath = paths.GetValue<string>(sectionName);
                var fullConfigPath = Path.Combine(Directory.GetCurrentDirectory(), ROOT_CONFIG_DIR, configPath);
                File.WriteAllText(fullConfigPath, definition);
            }
            catch (Exception ex)
            {
                Application.Resolve<ILoggingService>().Error($"Unable to save config section {sectionName}", ex);
            }
        }

        public T GetSection<T>(string sectionName, Action<T> onChange = null)
        {
            IConfigurationSection configSection = GetSection(sectionName, changedConfigSection =>
            {
                onChange?.Invoke(changedConfigSection.Get<T>());
            });
            return configSection.Get<T>();
        }

        public IConfigurationSection GetSection(string sectionName, Action<IConfigurationSection> onChange = null)
        {
            string configPath = paths.GetValue<string>(sectionName);
            IConfigurationRoot configRoot = GetConfig(configPath, changedConfigRoot =>
            {
                onChange?.Invoke(changedConfigRoot.GetSection(sectionName));
            });
            return configRoot.GetSection(sectionName);
        }

        private IConfigurationRoot GetConfig(string configPath, Action<IConfigurationRoot> onChange)
        {
            var configRootDir = Path.Combine(Directory.GetCurrentDirectory(), ROOT_CONFIG_DIR);
            var fullConfigPath = new DirectoryInfo(configRootDir).FullName;

            if (!File.Exists(Path.Combine(configRootDir, configPath)))
            {
                /*
orca-bot    |  ---> System.IO.FileNotFoundException: [
                "/app/Microsoft.Extensions.DependencyModel.dll","/app/ExchangeSharp.pdb","/app/SiliconeTrader.Signals.TradingView.pdb",
                "/app/System.IdentityModel.Tokens.Jwt.dll","/app/web.config","/app/SiliconeTrader.Machine.runtimeconfig.json",
                "/app/Microsoft.AspNetCore.Authentication.OpenIdConnect.dll","/app/SiliconeTrader.Signals.TradingView.dll",
                "/app/Serilog.Sinks.RollingFile.dll","/app/SiliconeTrader.Trading.dll","/app/appsettings.json",
                "/app/Microsoft.AspNetCore.Authentication.JwtBearer.dll","/app/SiliconeTrader.Backtesting.pdb",
                "/app/Swashbuckle.AspNetCore.Swagger.dll","/app/Serilog.Enrichers.Environment.dll",
                "/app/SiliconeTrader.Exchange.Base.dll","/app/Swashbuckle.AspNetCore.SwaggerGen.dll",
                "/app/Microsoft.IdentityModel.Protocols.dll","/app/SiliconeTrader.Exchange.Binance.pdb",
                "/app/Microsoft.IdentityModel.Protocols.OpenIdConnect.dll","/app/SiliconeTrader.Machine.pdb",
                "/app/Microsoft.OpenApi.dll","/app/SiliconeTrader.Signals.Base.dll","/app/Serilog.Settings.Configuration.dll",
                "/app/SiliconeTrader.Machine.dll","/app/Microsoft.IdentityModel.JsonWebTokens.dll",
                "/app/SiliconeTrader.Signals.Base.pdb","/app/SiliconeTrader.Machine.deps.json",
                "/app/Microsoft.DotNet.InternalAbstractions.dll","/app/Serilog.Sinks.File.dll",
                "/app/Azure.Identity.dll","/app/SiliconeTrader.Machine","/app/SiliconeTrader.Rules.dll",
                "/app/Serilog.Sinks.Console.dll","/app/SiliconeTrader.Backtesting.dll","/app/Microsoft.Identity.Web.dll",
                "/app/Azure.Security.KeyVault.Certificates.dll","/app/Swashbuckle.AspNetCore.SwaggerUI.dll",
                "/app/ZeroFormatter.Interfaces.dll","/app/SiliconeTrader.Core.pdb","/app/Microsoft.Identity.Client.Extensions.Msal.dll",
                "/app/Superpower.dll","/app/System.Security.Cryptography.ProtectedData.dll","/app/ExchangeSharp.dll",
                "/app/Microsoft.Identity.Client.dll","/app/Autofac.dll","/app/Microsoft.IdentityModel.Tokens.dll",
                "/app/SiliconeTrader.Rules.pdb","/app/Newtonsoft.Json.dll","/app/SiliconeTrader.Core.dll","/app/Serilog.dll",
                "/app/Azure.Security.KeyVault.Secrets.dll","/app/Microsoft.Bcl.AsyncInterfaces.dll",
                "/app/Serilog.Filters.Expressions.dll","/app/Azure.Core.dll","/app/SiliconeTrader.Trading.pdb",
                "/app/Telegram.Bot.dll","/app/SiliconeTrader.Exchange.Binance.dll","/app/ZeroFormatter.dll",
                "/app/Microsoft.IdentityModel.Logging.dll","/app/Microsoft.AspNet.SignalR.Client.dll","/app/appsettings.Development.json",
                "/app/SiliconeTrader.Exchange.Base.pdb","/app/runtimes/win/lib/netstandard2.0/System.Security.Cryptography.ProtectedData.dll"]
orca-bot    |    at SiliconeTrader.Core.ConfigProvider.GetConfig(String configPath, Action`1 onChange) in /src/SiliconeTrader.Core/Models/Config/ConfigProvider.cs:line 80
                 
                problem: /app root doesn't have Config files?
                 */


                throw new FileNotFoundException(JsonConvert.SerializeObject(Directory.GetFiles(Directory.GetCurrentDirectory(), "*.*", SearchOption.AllDirectories)));
            }

            var configBuilder = new ConfigurationBuilder()
                 .SetBasePath(fullConfigPath)
                 .AddJsonFile(configPath, optional: false, reloadOnChange: true)
                 .AddEnvironmentVariables();

            var configRoot = configBuilder.Build();
            ChangeToken.OnChange(configRoot.GetReloadToken, () => onChange(configRoot));
            return configRoot;
        }
    }
}
