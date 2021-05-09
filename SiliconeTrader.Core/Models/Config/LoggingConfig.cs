using System;
using System.Collections.Generic;
using System.Text;

namespace SiliconeTrader.Core
{
    internal class LoggingConfig : ILoggingConfig
    {
        public bool Enabled { get; set; } = true;
    }
}
