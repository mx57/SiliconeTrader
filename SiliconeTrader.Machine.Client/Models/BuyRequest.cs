namespace SiliconeTrader.Machine.Client.Models
{
    public class BuyRequest : BotRequest
    {
        public decimal Amount { get; set; }

        public string Pair { get; set; }
    }
}