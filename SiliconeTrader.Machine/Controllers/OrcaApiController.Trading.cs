using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using SiliconeTrader.Core;
using SiliconeTrader.Machine.Client.Models;

namespace SiliconeTrader.Machine.Controllers
{
    public partial class OrcaApiController
    {
        [HttpGet("trades/{id}")]
        public TradesViewModel Trades(DateTimeOffset id)
        {
            ICoreService coreService = Application.Resolve<ICoreService>();

            var model = new TradesViewModel()
            {
                InstanceName = coreService.Config.InstanceName,
                Version = coreService.Version,
                ReadOnlyMode = coreService.Config.ReadOnlyMode,
                TimezoneOffset = coreService.Config.TimezoneOffset,
                Date = id,
                Trades = GetTrades(id).Values.FirstOrDefault() ?? new List<TradeResult>()
            };

            return model;
        }

        [HttpGet("trading-pairs")]
        public TradingPairResponse TradingPairs()
        {
            ICoreService coreService = Application.Resolve<ICoreService>();
            ITradingService tradingService = Application.Resolve<ITradingService>();

            IEnumerable<TradingPairApiModel> tradingPairs = from tradingPair in tradingService.Account.GetTradingPairs()
                                                            let pairConfig = tradingService.GetPairConfig(tradingPair.Pair)
                                                            select new TradingPairApiModel
                                                            {
                                                                Name = tradingPair.Pair,
                                                                DCA = tradingPair.DCALevel,
                                                                TradingViewName = $"{tradingService.Config.Exchange.ToUpperInvariant()}:{tradingPair.Pair}",
                                                                Margin = tradingPair.CurrentMargin.ToString("0.00"),
                                                                Target = pairConfig.SellMargin.ToString("0.00"),
                                                                CurrentPrice = tradingPair.CurrentPrice.ToString("0.00000000"),
                                                                CurrentSpread = tradingPair.CurrentSpread.ToString("0.00"),
                                                                BoughtPrice = tradingPair.AveragePrice.ToString("0.00000000"),
                                                                Cost = tradingPair.Cost.ToString("0.00000000"),
                                                                CurrentCost = tradingPair.CurrentCost.ToString("0.00000000"),
                                                                Amount = tradingPair.Amount.ToString("0.########"),
                                                                OrderDates = tradingPair.OrderDates.Select(d => d.ToOffset(TimeSpan.FromHours(coreService.Config.TimezoneOffset)).ToString("yyyy-MM-dd HH:mm:ss")),
                                                                OrderIds = tradingPair.OrderIds,
                                                                Age = tradingPair.CurrentAge.ToString("0.00"),
                                                                CurrentRating = tradingPair.Metadata.CurrentRating?.ToString("0.000") ?? "N/A",
                                                                BoughtRating = tradingPair.Metadata.BoughtRating?.ToString("0.000") ?? "N/A",
                                                                SignalRule = tradingPair.Metadata.SignalRule ?? "N/A",
                                                                SwapPair = tradingPair.Metadata.SwapPair,
                                                                TradingRules = pairConfig.Rules,
                                                                IsTrailingSell = tradingService.GetTrailingSells().Contains(tradingPair.Pair),
                                                                IsTrailingBuy = tradingService.GetTrailingBuys().Contains(tradingPair.Pair),
                                                                LastBuyMargin = tradingPair.Metadata.LastBuyMargin?.ToString("0.00") ?? "N/A",
                                                                Config = pairConfig
                                                            };

            return new TradingPairResponse
            {
                TradingPairs = tradingPairs.ToList()
            };
        }
    }
}