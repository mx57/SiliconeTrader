namespace SiliconeTrader.Machine.Client.Models
{
    public class SellRequest : BotRequest
    {
        public decimal Amount { get; set; }

        public string Pair { get; set; }
    }
}