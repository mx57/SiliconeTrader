using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SiliconeTrader.Core.Interfaces;
using SiliconeTrader.Core.Interfaces.Services;
using SiliconeTrader.Core.Models;

namespace SiliconeTrader.Core.Services
{
    public class MarketDataService : IMarketDataService
    {
        // private readonly ICoreService _coreService; // Not used in the copied logic
        private readonly ITradingService _tradingService;
        private readonly ISignalsService _signalsService;

        public MarketDataService(
            ICoreService coreService, // Still injected for now, though not used in this specific method
            ITradingService tradingService,
            ISignalsService signalsService)
        {
            // _coreService = coreService ?? throw new ArgumentNullException(nameof(coreService));
            _tradingService = tradingService ?? throw new ArgumentNullException(nameof(tradingService));
            _signalsService = signalsService ?? throw new ArgumentNullException(nameof(signalsService));
        }

        public async Task<MarketPairsResponse> GetMarketPairsAsync(MarketPairsRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            // Logic from original OrcaApiController.Markets.cs
            IEnumerable<ISignal> allSignals = await Task.Run(() => _signalsService.GetAllSignals());

            if (allSignals == null)
            {
                // Return an empty response instead of null to match controller's expectation for Ok(response)
                return new MarketPairsResponse { MarketPairs = new List<MarketPairApiModel>() };
            }

            if (request.SignalsFilter != null && request.SignalsFilter.Count > 0)
            {
                allSignals = allSignals.Where(s => request.SignalsFilter.Contains(s.Name));
            }

            var groupedSignals = allSignals.GroupBy(s => s.Pair).ToDictionary(g => g.Key, g => g.AsEnumerable());
            List<MarketPairApiModel> marketPairsList = new List<MarketPairApiModel>();

            foreach (var signalGroup in groupedSignals) // signalGroup is KeyValuePair<string, IEnumerable<ISignal>> as per .ToDictionary
            {
                string pair = signalGroup.Key;
                // The following calls are wrapped in Task.Run as a temporary measure.
                IPairConfig pairConfig = await Task.Run(() => _tradingService.GetPairConfig(pair));
                string priceStr = await Task.Run(() => _tradingService.GetPrice(pair).ToString("0.00000000"));
                string spreadStr = await Task.Run(() => _tradingService.Exchange.GetPriceSpread(pair).ToString("0.00"));
                bool hasTradingPair = await Task.Run(() => _tradingService.Account.HasTradingPair(pair));

                var arbitrageList = new List<ArbitrageInfo>();
                var arbitrageMarkets = Enum.GetNames(typeof(ArbitrageMarket))
                                           .Where(m => m != _tradingService.Config.Market.ToString()) // Ensure Market is string for comparison
                                           .Select(m => Enum.Parse<ArbitrageMarket>(m))
                                           .ToList();

                foreach (var marketEnum in arbitrageMarkets) // Changed variable name to avoid conflict
                {
                    Arbitrage arbitrage = await Task.Run(() => _tradingService.Exchange.GetArbitrage(pair, _tradingService.Config.Market.ToString(), new List<ArbitrageMarket> { marketEnum }));
                    arbitrageList.Add(new ArbitrageInfo
                    {
                        Name = $"{arbitrage.Market}-{arbitrage.Type.ToString()[0]}",
                        Arbitrage = arbitrage.IsAssigned ? arbitrage.Percentage.ToString("0.00") : "N/A"
                    });
                }

                IEnumerable<string> signalRules = await Task.Run(() => _signalsService.GetTrailingInfo(pair)?.Select(ti => ti.Rule.Name) ?? Array.Empty<string>());

                marketPairsList.Add(new MarketPairApiModel
                {
                    Name = pair,
                    TradingViewName = $"{_tradingService.Config.Exchange.ToUpperInvariant()}:{pair}",
                    VolumeList = signalGroup.Value.Select(s => new NameValue<long?>(s.Name, s.Volume)).ToList(),
                    VolumeChangeList = signalGroup.Value.Select(s => new NameValue<double?>(s.Name, s.VolumeChange)).ToList(),
                    Price = priceStr,
                    PriceChangeList = signalGroup.Value.Select(s => new NameValue<decimal?>(s.Name, s.PriceChange)).ToList(),
                    RatingList = signalGroup.Value.Select(s => new NameValue<double?>(s.Name, s.Rating)).ToList(),
                    RatingChangeList = signalGroup.Value.Select(s => new NameValue<double?>(s.Name, s.RatingChange)).ToList(),
                    VolatilityList = signalGroup.Value.Select(s => new NameValue<double?>(s.Name, s.Volatility)).ToList(),
                    Spread = spreadStr,
                    ArbitrageList = arbitrageList,
                    SignalRules = signalRules,
                    HasTradingPair = hasTradingPair,
                    Config = pairConfig
                });
            }

            return new MarketPairsResponse // No Task.FromResult needed as the method is async and uses await
            {
                MarketPairs = marketPairsList
            };
        }
    }
}
