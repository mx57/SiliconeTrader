namespace SiliconeTrader.Core
{
    /// <summary>
    /// DCALevels setting is an array of DCA levels.
    /// There is no limit to the number of levels.
    /// The only mandatory setting for each level is <see cref="DCALevel.Margin"/>,
    /// which specifies when to trigger the DCA.
    /// All other settings are optional and when omitted will use the default values as defined above.
    /// </summary>
    public class DCALevel
    {
        /// <summary>
        /// Current cost multiplier. To double down set to 1 (Optional)
        /// </summary>
        public decimal? BuyMultiplier { get; set; } 

        /// <summary>
        /// Rebuy same pair timeout (in seconds) (Optional)
        /// </summary>
        public double? BuySamePairTimeout { get; set; } 

        /// <summary>
        /// Buy trailing percentage (should be a negative number, set to 0 to disable trailing) (Optional)
        /// </summary>
        public decimal? BuyTrailing { get; set; } 

        /// <summary>
        /// Action to take after hitting the StopMargin. Possible values: Buy, Cancel (Optional)
        /// </summary>
        public BuyTrailingStopAction? BuyTrailingStopAction { get; set; } 

        /// <summary>
        /// Stop trailing and place buy order immediately when margin hits the specified value. (Optional)
        /// </summary>
        public decimal? BuyTrailingStopMargin { get; set; } 

        /// <summary>
        /// When to trigger the DCA
        /// </summary>
        public decimal Margin { get; set; } = -1.50M;

        /// <summary>
        /// Minimum percentage increase to start trailing (Optional)
        /// </summary>
        public decimal? SellMargin { get; set; } 

        /// <summary>
        /// Sell trailing percentage (set to 0 to disable trailing) (Optional)
        /// </summary>
        public decimal? SellTrailing { get; set; } 

        /// <summary>
        /// Action to take after hitting the StopMargin. Possible values: Sell, Cancel (Optional)
        /// </summary>
        public SellTrailingStopAction? SellTrailingStopAction { get; set; } 

        /// <summary>
        /// Stop trailing and place buy order immediately when margin hits the specified value. (Optional)
        /// </summary>
        public decimal? SellTrailingStopMargin { get; set; } 
    }
}