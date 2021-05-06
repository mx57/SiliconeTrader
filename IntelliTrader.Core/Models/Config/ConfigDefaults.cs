namespace IntelliTrader.Core
{
    public sealed class ConfigDefaults
    {
        /// <summary>
        /// Current cost multiplier. To double down set to 1
        /// </summary>
        public const decimal BuyDCAMultiplier = 1;

        /// <summary>
        /// Rebuy same pair timeout (in seconds)
        /// </summary>
        public const double BuyDCASamePairTimeout = 4200;

        /// <summary>
        /// Buy trailing percentage (should be a negative number, set to 0 to disable trailing)
        /// </summary>
        public const decimal BuyDCATrailing = -1.5M;

        /// <summary>
        /// Stop trailing and place buy order immediately when margin hits the specified value
        /// </summary>
        public const decimal BuyDCATrailingStopMargin = 0.4M;

        /// <summary>
        /// Action to take after hitting the StopMargin. Possible values: Buy, Cancel
        /// </summary>
        public const BuyTrailingStopAction BuyDCATrailingStopAction = BuyTrailingStopAction.Cancel;

        /// <summary>
        /// Minimum percentage increase to start trailing
        /// </summary>
        public const decimal SellDCAMargin = 1.50M;

        /// <summary>
        /// Sell trailing percentage (set to 0 to disable trailing)
        /// </summary>

        public const decimal SellDCATrailing = 0.50M;

        /// <summary>
        /// Stop trailing and place sell order immediately when margin hits the specified value
        /// </summary>
        public const decimal SellDCATrailingStopMargin = 1.25M;
         
        /// <summary>
        /// Action to take after hitting the StopMargin. Possible values: Sell, Cancel
        /// </summary>
        public const SellTrailingStopAction SellDCATrailingStopAction = SellTrailingStopAction.Sell;
    }
}
