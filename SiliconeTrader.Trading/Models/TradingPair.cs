using SiliconeTrader.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SiliconeTrader.Trading
{
    public class TradingPair : ITradingPair
    {
        public string Pair { get; set; }
        [JsonIgnore]
        public string FormattedName => this.DCALevel > 0 ? $"{this.Pair}({this.DCALevel})" : this.Pair;
        public int DCALevel => (this.OrderDates.Count > 0 ? (this.OrderDates.Count - 1) : 0) + (this.Metadata.AdditionalDCALevels ?? 0);
        public List<string> OrderIds { get; set; }
        public List<DateTimeOffset> OrderDates { get; set; }
        [JsonConverter(typeof(DecimalFormatJsonConverter), 8)]
        public decimal Amount { get; set; }
        [JsonConverter(typeof(DecimalFormatJsonConverter), 8)]
        public decimal AveragePrice { get; set; }
        [JsonConverter(typeof(DecimalFormatJsonConverter), 8)]
        public decimal Fees { get; set; }
        [JsonConverter(typeof(DecimalFormatJsonConverter), 8)]
        public decimal Cost => this.GetPartialCost(this.Amount);
        [JsonIgnore]
        public decimal? CostOverride { get; set; }
        [JsonIgnore]
        public decimal CurrentCost => this.CurrentPrice * this.Amount;
        [JsonIgnore]
        public decimal CurrentPrice { get; set; }
        [JsonIgnore]
        public decimal CurrentSpread { get; set; }
        [JsonIgnore]
        public decimal CurrentMargin => Utils.CalculatePercentage(this.Cost + this.Fees + (this.Metadata.AdditionalCosts ?? 0), this.CurrentCost);
        [JsonIgnore]
        public double CurrentAge => this.OrderDates != null && this.OrderDates.Count > 0 ? (DateTimeOffset.Now - this.OrderDates.Min()).TotalDays : 0;
        [JsonIgnore]
        public double LastBuyAge => this.OrderDates != null && this.OrderDates.Count > 0 ? (DateTimeOffset.Now - this.OrderDates.Max()).TotalDays : 0;
        public OrderMetadata Metadata { get; set; } = new OrderMetadata();

        public decimal GetPartialCost(decimal partialAmount)
        {
            if (this.CostOverride != null)
            {
                return this.CostOverride.Value;
            }
            else
            {
                return this.AveragePrice * partialAmount;
            }
        }

        public void OverrideCost(decimal? costOverride)
        {
            this.CostOverride = costOverride;
        }

        public void SetCurrentValues(decimal currentPrice, decimal currentSpread)
        {
            this.CurrentPrice = currentPrice;
            this.CurrentSpread = currentSpread;
        }

        public void SetMetadata(OrderMetadata metadata)
        {
            this.Metadata = metadata;
        }
    }
}
