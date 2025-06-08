using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks; // Added for Task<>
using Microsoft.AspNetCore.Mvc;
using SiliconeTrader.Core;
using SiliconeTrader.Machine.Client.Models;

namespace SiliconeTrader.Machine.Controllers
{
    public partial class OrcaApiController
    {
        [HttpGet("trades/{id}")]
        public async Task<ActionResult<TradesViewModel>> Trades(DateTimeOffset id) // Changed return type
        {
            var tradesData = await GetTradesAsync(id);
            var model = new TradesViewModel()
            {
                InstanceName = _coreService.Config.InstanceName,
                Version = _coreService.Version,
                ReadOnlyMode = _coreService.Config.ReadOnlyMode,
                TimezoneOffset = _coreService.Config.TimezoneOffset,
                Date = id,
                Trades = tradesData.Values.FirstOrDefault() ?? new List<TradeResult>()
            };

            return Ok(model); // Return Ok(model)
        }

        [HttpGet("trading-pairs")]
        public async Task<ActionResult<TradingPairResponse>> TradingPairs() // Made async, changed return type
        {
            // The LINQ query and its dependencies might be I/O bound or CPU intensive.
            // Wrap the entire logic in Task.Run for now.
            var tradingPairsList = await Task.Run(() =>
            {
                return (from tradingPair in _tradingService.Account.GetTradingPairs()
                        let pairConfig = _tradingService.GetPairConfig(tradingPair.Pair)
                        select new TradingPairApiModel
                        {
                            Name = tradingPair.Pair,
                            DCA = tradingPair.DCALevel,
                            TradingViewName = $"{_tradingService.Config.Exchange.ToUpperInvariant()}:{tradingPair.Pair}",
                            Margin = tradingPair.CurrentMargin.ToString("0.00"),
                            Target = pairConfig.SellMargin.ToString("0.00"),
                            CurrentPrice = tradingPair.CurrentPrice.ToString("0.00000000"),
                            CurrentSpread = tradingPair.CurrentSpread.ToString("0.00"),
                            BoughtPrice = tradingPair.AveragePrice.ToString("0.00000000"),
                            Cost = tradingPair.Cost.ToString("0.00000000"),
                            CurrentCost = tradingPair.CurrentCost.ToString("0.00000000"),
                            Amount = tradingPair.Amount.ToString("0.########"),
                            OrderDates = tradingPair.OrderDates.Select(d => d.ToOffset(TimeSpan.FromHours(_coreService.Config.TimezoneOffset)).ToString("yyyy-MM-dd HH:mm:ss")),
                            OrderIds = tradingPair.OrderIds,
                            Age = tradingPair.CurrentAge.ToString("0.00"),
                            CurrentRating = tradingPair.Metadata.CurrentRating?.ToString("0.000") ?? "N/A",
                            BoughtRating = tradingPair.Metadata.BoughtRating?.ToString("0.000") ?? "N/A",
                            SignalRule = tradingPair.Metadata.SignalRule ?? "N/A",
                            SwapPair = tradingPair.Metadata.SwapPair,
                            TradingRules = pairConfig.Rules,
                            IsTrailingSell = _tradingService.GetTrailingSells().Contains(tradingPair.Pair),
                            IsTrailingBuy = _tradingService.GetTrailingBuys().Contains(tradingPair.Pair),
                            LastBuyMargin = tradingPair.Metadata.LastBuyMargin?.ToString("0.00") ?? "N/A",
                            Config = pairConfig
                        }).ToList(); // Materialize the list inside Task.Run
            });

            return Ok(new TradingPairResponse // Return Ok(response)
            {
                TradingPairs = tradingPairsList
            });
        }
    }
}