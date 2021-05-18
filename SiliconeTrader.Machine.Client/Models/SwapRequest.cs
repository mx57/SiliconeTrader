namespace SiliconeTrader.Machine.Client.Models
{
    public class SwapRequest : BotRequest
    {
        public string Pair { get; set; }

        public string Swap { get; set; }
    }
}