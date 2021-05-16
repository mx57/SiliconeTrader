namespace SiliconeTrader.Machine.Client.Models.Responses
{
    public class InstanceVersionResponse : BotResponse, IInstanceVersion
    { 
        public string InstanceName { get; set; }

        public bool ReadOnlyMode { get; set; }

        public string Version { get; set; }
    }
}