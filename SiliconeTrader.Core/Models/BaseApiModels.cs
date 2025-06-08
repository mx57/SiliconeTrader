namespace SiliconeTrader.Core.Models
{
    public abstract class BotRequest
    {
        // public string BotId { get; set; } // Example, not specified to add yet
    }

    public abstract class BotResponse : DefaultViewModel // Inherits from Core.Models.DefaultViewModel
    {
        // Common response properties can go here
    }
}
