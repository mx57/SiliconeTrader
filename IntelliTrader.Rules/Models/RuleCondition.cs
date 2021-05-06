using IntelliTrader.Core;
using System.Collections.Generic;

namespace IntelliTrader.Rules
{
    internal class RuleCondition : IRuleCondition
    {
        /// <summary>
        /// Market to look for the arbitrage.
        /// Available values: ETH, BNB, USDT.
        /// When omitted all markets will be considered
        /// </summary>
        public ArbitrageMarket? ArbitrageMarket { get; set; }

        /// <summary>
        /// Type of arbitrage to look for.
        /// Available values: Direct, Reverse.
        /// When omitted both types will be considered
        /// </summary>
        public ArbitrageType? ArbitrageType { get; set; }

        /// <summary>
        /// Maximum trading pair's age (in days, e.g. 1.5 is 36 hours)
        /// </summary>
        public double? MaxAge { get; set; }

        /// <summary>
        /// Maximum trading pair's total purchase amount
        /// </summary>
        public decimal? MaxAmount { get; set; }

        /// <summary>
        /// Maximum triangular arbitrage value
        /// </summary>
        public decimal? MaxArbitrage { get; set; }

        /// <summary>
        /// Maximum trading pair's total current cost
        /// </summary>
        public decimal? MaxCost { get; set; }

        /// <summary>
        /// Maximum trading pair's DCA level
        /// </summary>
        public int? MaxDCALevel { get; set; }

        /// <summary>
        /// Maximum global rating calculated from all individual coins ratings.
        /// Is a value between -1 and 1. Value +0.2 is considered a bullish market
        /// </summary>
        public double? MaxGlobalRating { get; set; }

        /// <summary>
        /// Maximum trading pair's age since last buy (in days, e.g. 1.5 is 36 hours)
        /// </summary>
        public double? MaxLastBuyAge { get; set; }

        /// <summary>
        /// Maximum trading pair's margin
        /// </summary>
        public decimal? MaxMargin { get; set; }

        /// <summary>
        /// The maximal rate of change of price withing specified period frame (percentage)
        /// </summary>
        public decimal? MaxMarginChange { get; set; }

        /// <summary>
        /// Maximum current price of the coin
        /// </summary>
        public decimal? MaxPrice { get; set; }

        /// <summary>
        /// The maximal rate of change of price withing specified period frame (percentage)
        /// </summary>
        public decimal? MaxPriceChange { get; set; }

        /// <summary>
        /// Maximal ration of a coin within the specified signal's period.
        /// Expect a value between -1 and 1.
        /// In a normal market do not expect it to go over 0.6
        /// </summary>
        public double? MaxRating { get; set; }

        /// <summary>
        /// The maximal rate of change of coins rating (percentage).
        /// For a reference values let the bot run and then have a look in the dashboard
        /// </summary>
        public double? MaxRatingChange { get; set; }

        /// <summary>
        /// Maximum difference between current bid and ask price (percentage)
        /// </summary>
        public decimal? MaxSpread { get; set; }

        /// <summary>
        /// Maximum average volatility of a coin within its own specified timeframe
        /// </summary>
        public double? MaxVolatility { get; set; }

        /// <summary>
        /// Maximum coin volume within the specified signal's period.
        /// Do not expect 24h volume in this category
        /// </summary>
        public long? MaxVolume { get; set; }

        /// <summary>
        /// The maximal rate of change of volume withing specified period frame (percentage)
        /// </summary>
        public double? MaxVolumeChange { get; set; }

        /// <summary>
        /// Minimum trading pair's age (in days, e.g. 1.5 is 36 hours)
        /// </summary>
        public double? MinAge { get; set; }

        /// <summary>
        /// Minimum trading pair's total purchase amount
        /// </summary>
        public decimal? MinAmount { get; set; }

        /// <summary>
        /// Minimum triangular arbitrage value
        /// </summary>
        public decimal? MinArbitrage { get; set; }

        /// <summary>
        /// Minimum trading pair's total current cost
        /// </summary>
        public decimal? MinCost { get; set; }

        /// <summary>
        /// Minimum trading pair's DCA level
        /// </summary>
        public int? MinDCALevel { get; set; }

        /// <summary>
        /// Minimal global rating calculated from all individual coins ratings.
        /// Is a value between -1 and 1. Value -0.2 is considered a bearish market
        /// </summary>
        public double? MinGlobalRating { get; set; }

        /// <summary>
        /// Minimum trading pair's age since last buy (in days, e.g. 1.5 is 36 hours)
        /// </summary>
        public double? MinLastBuyAge { get; set; }

        /// <summary>
        /// Minimum trading pair's margin
        /// </summary>
        public decimal? MinMargin { get; set; }

        /// <summary>
        /// The minimal rate of change of price withing specified period frame (percentage)
        /// </summary>
        public decimal? MinMarginChange { get; set; }

        /// <summary>
        /// Minimum current price of the coin
        /// </summary>
        public decimal? MinPrice { get; set; }

        /// <summary>
        /// The minimal rate of change of price withing specified period frame (percentage)
        /// </summary>
        public decimal? MinPriceChange { get; set; }

        /// <summary>
        /// Minimal rating of a coin within the specified signal's period.
        /// Expect a value between -1 and 1.
        /// A good place to start is 0.3+
        /// </summary>
        public double? MinRating { get; set; }

        /// <summary>
        /// The minimal rate of change of coins rating (percentage).
        /// For a reference values let the bot run and then have a look in the dashboard
        /// </summary>
        public double? MinRatingChange { get; set; }

        /// <summary>
        /// Minimum difference between current bid and ask price (percentage)
        /// </summary>
        public decimal? MinSpread { get; set; }

        /// <summary>
        /// Minimum average volatility of a coin within its own specified timeframe
        /// </summary>
        public double? MinVolatility { get; set; }

        /// <summary>
        /// Minimum coin volume within the specified signal's period.
        /// Do not expect 24h volume in this category
        /// </summary>
        public long? MinVolume { get; set; }

        /// <summary>
        /// The minimal rate of change of volume withing specified period frame (percentage)
        /// </summary>
        public double? MinVolumeChange { get; set; }

        /// <summary>
        /// List of pairs to not apply the rule to
        /// </summary>
        public List<string> NotPairs { get; set; }

        /// <summary>
        /// List of signal rules that were not used to buy a pair
        /// </summary>
        public List<string> NotSignalRules { get; set; }

        /// <summary>
        /// List of pairs to directly apply the rule to
        /// </summary>
        public List<string> Pairs { get; set; }

        /// <summary>
        /// Signal with which to calculate signal-specific conditions.
        /// eg: "TV-1m"
        /// </summary>
        public string Signal { get; set; }

        /// <summary>
        /// List of signal rules that were used to buy a pair
        /// </summary>
        public List<string> SignalRules { get; set; }
    }
}