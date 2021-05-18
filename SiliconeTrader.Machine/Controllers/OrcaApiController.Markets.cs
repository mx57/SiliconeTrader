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
        [HttpPost("market-pairs")]
        [ProducesResponseType(200, Type = typeof(MarketPairsResponse))]
        public MarketPairsResponse MarketPairs(MarketPairsRequest request)
        {
            if (request?.SignalsFilter == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            ICoreService coreService = Application.Resolve<ICoreService>();
            ITradingService tradingService = Application.Resolve<ITradingService>();
            ISignalsService signalsService = Application.Resolve<ISignalsService>();

            IEnumerable<ISignal> allSignals = signalsService.GetAllSignals();

            if (allSignals != null)
            {
                if (request.SignalsFilter.Count > 0)
                {
                    allSignals = allSignals.Where(s => request.SignalsFilter.Contains(s.Name));
                }

                var groupedSignals = allSignals.GroupBy(s => s.Pair).ToDictionary(g => g.Key, g => g.AsEnumerable());

                IEnumerable<MarketPairApiModel> marketPairs = from signalGroup in groupedSignals
                                                              let pair = signalGroup.Key
                                                              let pairConfig = tradingService.GetPairConfig(pair)
                                                              select new MarketPairApiModel
                                                              {
                                                                  Name = pair,
                                                                  TradingViewName = $"{tradingService.Config.Exchange.ToUpperInvariant()}:{pair}",
                                                                  VolumeList = signalGroup.Value.Select(s => new NameValue<long?>(s.Name, s.Volume)),
                                                                  VolumeChangeList = signalGroup.Value.Select(s => new NameValue<double?>(s.Name, s.VolumeChange)),
                                                                  Price = tradingService.GetPrice(pair).ToString("0.00000000"),
                                                                  PriceChangeList = signalGroup.Value.Select(s => new NameValue<decimal?>(s.Name, s.PriceChange)),
                                                                  RatingList = signalGroup.Value.Select(s => new NameValue<double?>(s.Name, s.Rating)),
                                                                  RatingChangeList = signalGroup.Value.Select(s => new NameValue<double?>(s.Name, s.RatingChange)),
                                                                  VolatilityList = signalGroup.Value.Select(s => new NameValue<double?>(s.Name, s.Volatility)),
                                                                  Spread = tradingService.Exchange.GetPriceSpread(pair).ToString("0.00"),
                                                                  ArbitrageList = from market in Enum.GetNames(typeof(ArbitrageMarket)).Where(m => m != tradingService.Config.Market)
                                                                                  let arbitrage = tradingService.Exchange.GetArbitrage(pair, tradingService.Config.Market, new List<ArbitrageMarket> { Enum.Parse<ArbitrageMarket>(market) })
                                                                                  select new ArbitrageInfo
                                                                                  {
                                                                                      Name = $"{arbitrage.Market}-{arbitrage.Type.ToString()[0]}",
                                                                                      Arbitrage = arbitrage.IsAssigned ? arbitrage.Percentage.ToString("0.00") : "N/A"
                                                                                  },
                                                                  SignalRules = signalsService.GetTrailingInfo(pair)?.Select(ti => ti.Rule.Name) ?? Array.Empty<string>(),
                                                                  HasTradingPair = tradingService.Account.HasTradingPair(pair),
                                                                  Config = pairConfig
                                                              };

                return new MarketPairsResponse
                {
                    MarketPairs = marketPairs.ToList()
                };
            }
            else
            {
                return null;
            }
        }

        [HttpGet("market-signals")]
        [ProducesResponseType(200, Type = typeof(MarketSignalsResponse))]
        public MarketSignalsResponse MarketSignals()
        {
            ISignalsService signalsService = Application.Resolve<ISignalsService>();

            return new MarketSignalsResponse
            {
                Signals = signalsService.GetSignalNames()
            };
        }
    }
}