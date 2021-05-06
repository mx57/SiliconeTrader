namespace IntelliTrader.Core
{
    internal class CoreConfig : ICoreConfig
    {
        /// <summary>
        /// Enable / disable debug mode
        /// </summary>
        public bool DebugMode { get; set; } = true;

        /// <summary>
        /// Enable / disable health check
        /// </summary>
        public bool HealthCheckEnabled { get; set; } = true;

        /// <summary>
        /// Restart all services when health check fails for a specified number of times in a row. Set to 0 to disable restart
        /// </summary>
        public int HealthCheckFailuresToRestartServices { get; set; } = 3;

        /// <summary>
        /// Interval to check that all the data is up to date and services are running correctly (in seconds)
        /// </summary>
        public double HealthCheckInterval { get; set; } = 180;

        /// <summary>
        /// Suspend trading (disable sells and buys)
        /// if any of the health checks did not pass in a specified period of time (in seconds).
        /// Set to 0 to continue trading even after health check fails
        /// </summary>
        public double HealthCheckSuspendTradingTimeout { get; set; } = 900;

        /// <summary>
        /// Must be alphanumeric and should not contain spaces.
        /// Used by the web interface & notification service
        /// to distinguish between different bot instances (when running multiple)
        /// </summary>
        public string InstanceName { get; set; } = "Main";

        /// <summary>
        /// MD5-encrypted password
        /// TODO: move to a secure one.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Use password authentication
        /// </summary>
        public bool PasswordProtected { get; set; } = true;

        /// <summary>
        /// Timezone offset from UTC (in hours), used by stats
        /// </summary>
        public double TimezoneOffset { get; set; } = 1;
    }
}