using SiliconeTrader.Machine.Client.Core;

namespace SiliconeTrader.Machine.Client
{
    public abstract class BotResponse
    {
        public ErrorResponse Error { get; set; }
    }
}