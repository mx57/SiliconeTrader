using System;
using System.Collections.Generic;
using System.Text;

namespace SiliconeTrader.Core
{
    public interface ICoreConfig
    {
        bool ReadOnlyMode { get; }
        bool DebugMode { get; }  
        string InstanceName { get; }
        double TimezoneOffset { get; }
        bool HealthCheckEnabled { get; set; }
        double HealthCheckInterval { get; }
        double HealthCheckSuspendTradingTimeout { get; }
        int HealthCheckFailuresToRestartServices { get; }
    }
}
