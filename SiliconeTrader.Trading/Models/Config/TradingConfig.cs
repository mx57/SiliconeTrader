using SiliconeTrader.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SiliconeTrader.Trading
{
    /// <summary>
    /// This bot trades at market price.
    /// </summary>
    internal class TradingConfig : ITradingConfig
    {
        // TODO: move to their own homes!
        #region General Config

        /// <summary>
        /// Enable / disable trading
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Market to trade on - BTC, ETH, USDT, etc.
        /// </summary>
        public string Market { get; set; } = "BTC";

        /// <summary>
        /// Exchange to trade on. Curently only Binance is supported. 
        /// <see cref="ExchangeSharp.ExchangeAPI"/>
        /// </summary>
        public string Exchange { get; set; } = "Binance";

        /// <summary>
        /// Maximum pairs to trade with
        /// </summary>
        public int MaxPairs { get; set; } = 16;

        /// <summary>
        /// Ignore pairs with the specified market value or lower (dust)
        /// </summary>
        public decimal MinCost { get; set; } = 0.000999M;

        /// <summary>
        /// Pairs excluded from trading
        /// </summary>
        public List<string> ExcludedPairs { get; set; } = new List<string> { "BNBBTC" };

        /// <summary>
        /// Price type to use for trading. Available values: Last, Ask, Bid
        /// </summary>
        public TradePriceType TradePriceType { get; set; } = TradePriceType.Last;

        #endregion

        #region Buy Config

        /// <summary>
        /// Enable / disable buying
        /// </summary>
        public bool BuyEnabled { get; set; } = true;

        /// <summary>
        /// Supported types: Market. Limit is not currently supported
        /// </summary>
        public OrderType BuyType { get; set; } = OrderType.Market;

        /// <summary>
        /// Maximum cost when buying a new pair
        /// </summary>
        public decimal BuyMaxCost { get; set; } = 0.0012M;

        /// <summary>
        /// Maximum cost multiplier
        /// </summary>
        public decimal BuyMultiplier { get; set; } = 1;

        /// <summary>
        /// Minimum account balance to buy new pairs
        /// </summary>
        public decimal BuyMinBalance { get; set; } = 0;

        /// <summary>
        /// Rebuy same pair timeout (in seconds)
        /// </summary>
        public double BuySamePairTimeout { get; set; } = 900;

        /// <summary>
        /// Buy trailing percentage (should be a negative number, set to 0 to disable trailing)
        /// </summary>
        public decimal BuyTrailing { get; set; } = -0.15M;

        /// <summary>
        /// Stop trailing and place buy order immediately when margin hits the specified value
        /// </summary>
        public decimal BuyTrailingStopMargin { get; set; } = 0.05M;

        /// <summary>
        /// Action to take after hitting the StopMargin. Possible values: Buy, Cancel
        /// </summary>
        public BuyTrailingStopAction BuyTrailingStopAction { get; set; } = BuyTrailingStopAction.Buy;

        #endregion

        #region Buying DCA (Dollar Cost Avg)

        /// <summary>
        /// Enable / disable buying DCA
        /// </summary>
        public bool BuyDCAEnabled { get; set; } = true;

        /// <summary>
        /// Current cost multiplier. To double down set to 1
        /// </summary>
        public decimal BuyDCAMultiplier { get; set; } = ConfigDefaults.BuyDCAMultiplier;

        /// <summary>
        /// Minimum account balance to DCA
        /// </summary>
        public decimal BuyDCAMinBalance { get; set; } = 0;

        /// <summary>
        /// Rebuy same pair timeout (in seconds)
        /// </summary>
        public double BuyDCASamePairTimeout { get; set; } = ConfigDefaults.BuyDCASamePairTimeout;

        /// <summary>
        /// Buy trailing percentage (should be a negative number, set to 0 to disable trailing)
        /// </summary>
        public decimal BuyDCATrailing { get; set; } = ConfigDefaults.BuyDCATrailing;

        /// <summary>
        /// Stop trailing and place buy order immediately when margin hits the specified value
        /// </summary>
        public decimal BuyDCATrailingStopMargin { get; set; } = ConfigDefaults.BuyDCATrailingStopMargin;

        /// <summary>
        /// Action to take after hitting the StopMargin. Possible values: Buy, Cancel
        /// </summary>
        public BuyTrailingStopAction BuyDCATrailingStopAction { get; set; } = ConfigDefaults.BuyDCATrailingStopAction;

        #endregion

        #region Sell Config

        /// <summary>
        /// Enable / disable selling
        /// </summary>
        public bool SellEnabled { get; set; } = true;

        /// <summary>
        /// Supported types: Market. Limit is not currently supported
        /// </summary>
        public OrderType SellType { get; set; } = OrderType.Market;

        /// <summary>
        /// Minimum percentage increase to start trailing
        /// </summary>
        public decimal SellMargin { get; set; } = 2.00M;

        /// <summary>
        /// Sell trailing percentage (should be a positive number, set to 0 to disable trailing)
        /// </summary>
        public decimal SellTrailing { get; set; } = 0.70M;

        /// <summary>
        /// Stop trailing and place sell order immediately when margin hits the specified value
        /// </summary>
        public decimal SellTrailingStopMargin { get; set; } = 1.70M;

        /// <summary>
        /// Action to take after hitting the StopMargin. Possible values: Sell, Cancel
        /// </summary>
        public SellTrailingStopAction SellTrailingStopAction { get; set; } = SellTrailingStopAction.Sell;

        /// <summary>
        /// Enable / disable stop loss trigger
        /// </summary>
        public bool SellStopLossEnabled { get; set; } = false;

        /// <summary>
        /// Trigger stop loss only after all DCA levels has been reached
        /// </summary>
        public bool SellStopLossAfterDCA { get; set; } = true;

        /// <summary>
        /// Minimum number of days needed before triggering the stop loss
        /// </summary>
        public double SellStopLossMinAge { get; set; } = 3.0D;

        /// <summary>
        /// Trigger stop loss and immediately place sell order at the specified percentage decrease
        /// </summary>
        public decimal SellStopLossMargin { get; set; } = -20;


        #endregion

        #region Sell DCA

        /// <summary>
        /// Minimum percentage increase to start trailing
        /// </summary>
        public decimal SellDCAMargin { get; set; } = ConfigDefaults.SellDCAMargin;

        /// <summary>
        /// Sell trailing percentage (set to 0 to disable trailing)
        /// </summary>
        public decimal SellDCATrailing { get; set; } = ConfigDefaults.SellDCATrailing;

        /// <summary>
        /// Stop trailing and place sell order immediately when margin hits the specified value
        /// </summary>
        public decimal SellDCATrailingStopMargin { get; set; } = ConfigDefaults.SellDCATrailingStopMargin;

        /// <summary>
        /// Action to take after hitting the StopMargin. Possible values: Sell, Cancel
        /// </summary>
        public SellTrailingStopAction SellDCATrailingStopAction { get; set; } = ConfigDefaults.SellDCATrailingStopAction;

        /// <summary>
        /// Repeat the last DCA Level indefinitely, essentially making the DCA level number unlimited
        /// </summary>
        public bool RepeatLastDCALevel { get; set; } = false;

        /// <summary>
        /// Action to take after hitting the StopMargin. Possible values: Sell, Cancel
        /// </summary>
        public List<DCALevel> DCALevels { get; set; } = new List<DCALevel>();

        #endregion

        #region Accounts Config

        /// <summary>
        /// Tickers check frequency (in seconds)
        /// </summary>
        public double TradingCheckInterval { get; set; } = 1;

        /// <summary>
        /// Exchange account refresh interval (in seconds)
        /// </summary>
        public double AccountRefreshInterval { get; set; } = 360;

        /// <summary>
        /// Initial balance on the account, used for stats calculations
        /// </summary>
        public decimal AccountInitialBalance { get; set; }

        /// <summary>
        /// Date of the initial balance snapshot
        /// </summary>
        public DateTimeOffset AccountInitialBalanceDate { get; set; }

        /// <summary>
        /// Path to the account file
        /// </summary>
        public string AccountFilePath { get; set; } = "data/exchange-account.json";

        /// <summary>
        /// Enable / disable virtual trading
        /// </summary>
        public bool VirtualTrading { get; set; } = true;

        /// <summary>
        /// Trading fees (percentage)
        /// </summary>
        public decimal VirtualTradingFees { get; set; }

        /// <summary>
        /// Initial balance on the virtual account, used for stats calculations
        /// </summary>
        public decimal VirtualAccountInitialBalance { get; set; }

        /// <summary>
        /// Path to the virtual account file
        /// </summary>
        public string VirtualAccountFilePath { get; set; } = "data/virtual-account.json";

        #endregion

        public ITradingConfig Clone()
        {
            return new TradingConfig
            {
                Enabled = Enabled,
                Market = Market,
                Exchange = Exchange,
                MaxPairs = MaxPairs,
                MinCost = MinCost,
                ExcludedPairs = ExcludedPairs,
                TradePriceType = TradePriceType,

                BuyEnabled = BuyEnabled,
                BuyType = BuyType,
                BuyMaxCost = BuyMaxCost,
                BuyMultiplier = BuyMultiplier,
                BuyMinBalance = BuyMinBalance,
                BuySamePairTimeout = BuySamePairTimeout,
                BuyTrailing = BuyTrailing,
                BuyTrailingStopMargin = BuyTrailingStopMargin,
                BuyTrailingStopAction = BuyTrailingStopAction,

                BuyDCAEnabled = BuyDCAEnabled,
                BuyDCAMultiplier = BuyDCAMultiplier,
                BuyDCAMinBalance = BuyDCAMinBalance,
                BuyDCASamePairTimeout = BuyDCASamePairTimeout,
                BuyDCATrailing = BuyDCATrailing,
                BuyDCATrailingStopMargin = BuyDCATrailingStopMargin,
                BuyDCATrailingStopAction = BuyDCATrailingStopAction,

                SellEnabled = SellEnabled,
                SellType = SellType,
                SellMargin = SellMargin,
                SellTrailing = SellTrailing,
                SellTrailingStopMargin = SellTrailingStopMargin,
                SellTrailingStopAction = SellTrailingStopAction,
                SellStopLossEnabled = SellStopLossEnabled,
                SellStopLossAfterDCA = SellStopLossAfterDCA,
                SellStopLossMinAge = SellStopLossMinAge,
                SellStopLossMargin = SellStopLossMargin,

                SellDCAMargin = SellDCAMargin,
                SellDCATrailing = SellDCATrailing,
                SellDCATrailingStopMargin = SellDCATrailingStopMargin,
                SellDCATrailingStopAction = SellDCATrailingStopAction,

                RepeatLastDCALevel = RepeatLastDCALevel,
                DCALevels = DCALevels,

                TradingCheckInterval = TradingCheckInterval,
                AccountRefreshInterval = AccountRefreshInterval,
                AccountInitialBalance = AccountInitialBalance,
                AccountInitialBalanceDate = AccountInitialBalanceDate,
                AccountFilePath = AccountFilePath,

                VirtualTrading = VirtualTrading,
                VirtualTradingFees = VirtualTradingFees,
                VirtualAccountInitialBalance = VirtualAccountInitialBalance,
                VirtualAccountFilePath = VirtualAccountFilePath
            };
        }
    }
}