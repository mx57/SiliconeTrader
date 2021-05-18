namespace SiliconeTrader.Machine.Client.Models
{
    public class SaveConfigRequest : BotRequest
    {
        public string ConfigDefinition { get; set; }

        public string Name { get; set; }
    }
}