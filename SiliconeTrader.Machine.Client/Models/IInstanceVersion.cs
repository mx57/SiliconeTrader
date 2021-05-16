namespace SiliconeTrader.Machine.Client.Models
{
    public interface IInstanceVersion
    {
        string InstanceName { get; set; }

        bool ReadOnlyMode { get; set; }

        string Version { get; set; }
    }
}