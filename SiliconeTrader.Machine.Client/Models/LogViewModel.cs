using System.Collections.Generic;
using SiliconeTrader.Core.Models;

namespace SiliconeTrader.Machine.Client.Models
{
    public class LogViewModel : BotResponse
    {
        public IEnumerable<string> LogEntries { get; set; }
    }
}