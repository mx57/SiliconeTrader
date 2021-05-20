using System.Collections.Generic;

namespace SiliconeTrader.Machine.Client.Models
{
    public class LogViewModel : BotResponse
    {
        public IEnumerable<string> LogEntries { get; set; }
    }
}