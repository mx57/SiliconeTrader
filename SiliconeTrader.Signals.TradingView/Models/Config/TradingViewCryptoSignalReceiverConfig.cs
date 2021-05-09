namespace SiliconeTrader.Signals.TradingView
{
    internal class TradingViewCryptoSignalReceiverConfig
    {
        public double PollingInterval { get; set; } = 7;

        public string RequestData { get; set; } = "{\"filter\":[{\"left\":\"exchange\",\"operation\":\"equal\",\"right\":\"%EXCHANGE%\"},{\"left\":\"name\",\"operation\":\"match\",\"right\":\"%MARKET%\"}],\"columns\":[\"name\",\"close%PERIOD%\",\"change%PERIOD%\",\"volume%PERIOD%\",\"Recommend.All%PERIOD%\",\"Volatility%VOLATILITY%\"],\"options\":{\"lang\":\"en\"},\"range\":[0,500]}";

        public string RequestUrl { get; set; } = "https://scanner.tradingview.com/crypto/scan";

        public int SignalPeriod { get; set; } = 15;

        public string VolatilityPeriod { get; set; } = "Day";
    }
}