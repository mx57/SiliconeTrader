namespace SiliconeTrader.Core.Models
{
    public class DefaultViewModel : IInstanceVersion
    {
        public string Version { get; set; }
        public string ProductName { get; set; }
        public string InstanceName { get; set; } // Added
        public bool ReadOnlyMode { get; set; }   // Added
        public ErrorResponse Error { get; set; } // Property name was already 'Error'

        // Added static Default property
        public static DefaultViewModel Default => new DefaultViewModel
        {
            // Initialize with sensible defaults, though BaseController might override
            InstanceName = string.Empty,
            ReadOnlyMode = true, // A safe default
            Version = string.Empty,
            ProductName = string.Empty,
            Error = null
        };
    }
}
