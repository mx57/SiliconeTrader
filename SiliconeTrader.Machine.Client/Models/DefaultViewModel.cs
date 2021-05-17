using SiliconeTrader.Machine.Client.Core;

namespace SiliconeTrader.Machine.Client.Models
{
    public class DefaultViewModel : IInstanceVersion
    {
        public static DefaultViewModel Default => new()
        {
            InstanceName = "",
            ReadOnlyMode = true,
            Version = "",
            Error = null
        };

        public ErrorResponse Error { get; set; }

        public string InstanceName { get; set; }

        public bool ReadOnlyMode { get; set; }

        public string Version { get; set; }
    }
}