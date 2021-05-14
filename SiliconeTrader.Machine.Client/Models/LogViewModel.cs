using System.Collections.Generic;

namespace SiliconeTrader.Machine.Client.Models
{
    public class LogViewModel : DefaultViewModel
    {
        public IEnumerable<string> LogEntries { get; set; }
    }
}