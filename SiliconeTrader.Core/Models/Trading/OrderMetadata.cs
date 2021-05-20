using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace SiliconeTrader.Core
{
    public class OrderMetadata
    {
        public bool? IsTransitional { get; set; }
        public string OriginalPair { get; set; }
        public List<string> TradingRules { get; set; }
        public string SignalRule { get; set; }
        public List<string> Signals { get; set; }
        public double? BoughtRating { get; set; }
        public double? CurrentRating { get; set; }
        public double? BoughtGlobalRating { get; set; }
        public double? CurrentGlobalRating { get; set; }
        public decimal? LastBuyMargin { get; set; }
        public int? AdditionalDCALevels { get; set; }
        public decimal? AdditionalCosts { get; set; }
        public decimal? FeesNonDeductible { get; set; }
        public string SwapPair { get; set; }
        public string Arbitrage { get; set; }
        public decimal? ArbitragePercentage { get; set; }

        public OrderMetadata MergeWith(OrderMetadata metadata)
        {
            return new OrderMetadata
            {
                IsTransitional = metadata.IsTransitional ?? this.IsTransitional,
                OriginalPair = metadata.OriginalPair ?? this.OriginalPair,
                TradingRules = metadata.TradingRules ?? this.TradingRules,
                SignalRule = metadata.SignalRule ?? this.SignalRule,
                Signals = metadata.Signals ?? this.Signals,
                BoughtRating = metadata.BoughtRating ?? this.BoughtRating,
                CurrentRating = metadata.CurrentRating ?? this.CurrentRating,
                BoughtGlobalRating = metadata.BoughtGlobalRating ?? this.BoughtGlobalRating,
                CurrentGlobalRating = metadata.CurrentGlobalRating ?? this.CurrentGlobalRating,
                LastBuyMargin = metadata.LastBuyMargin ?? this.LastBuyMargin,
                AdditionalDCALevels = metadata.AdditionalDCALevels ?? this.AdditionalDCALevels,
                AdditionalCosts = metadata.AdditionalCosts ?? this.AdditionalCosts,
                FeesNonDeductible = metadata.FeesNonDeductible ?? this.FeesNonDeductible,
                SwapPair = metadata.SwapPair ?? this.SwapPair,
                Arbitrage = metadata.Arbitrage ?? this.Arbitrage,
                ArbitragePercentage = metadata.ArbitragePercentage ?? this.ArbitragePercentage
            };
        }
    }
}
