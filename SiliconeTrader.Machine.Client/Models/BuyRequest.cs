using SiliconeTrader.Core.Models; // Added using for Core.Models

namespace SiliconeTrader.Machine.Client.Models
{
    public class BuyRequest : BotRequest // BotRequest is now from Core.Models
    {
        public decimal Amount { get; set; }

        public string Pair { get; set; }
    }
}