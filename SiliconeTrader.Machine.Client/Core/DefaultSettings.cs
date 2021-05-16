using System;
using System.Net;

namespace SiliconeTrader.Machine.Client.Core
{
    internal class DefaultSettings
    {
        public static readonly TimeSpan MaxMessageRetryDelay = TimeSpan.FromMinutes(1);
        public static readonly TimeSpan MinMessageRetryDelay = TimeSpan.FromMilliseconds(100);

        public static readonly HttpStatusCode[] TransientErrors =
        {
            HttpStatusCode.RequestTimeout,
            HttpStatusCode.GatewayTimeout,
            HttpStatusCode.TooManyRequests,
        };
    }
}