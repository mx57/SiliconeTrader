namespace SiliconeTrader.Core.Models
{
    public interface IInstanceVersion
    {
        string Version { get; } // Keep existing get;
        string ProductName { get; } // Keep existing get;
        string InstanceName { get; } // Added property
        bool ReadOnlyMode { get; }   // Added property
    }
}
