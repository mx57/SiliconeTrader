using System.Threading;
using System.Threading.Tasks;
using SiliconeTrader.Core.Models; // For MarketPairsRequest, MarketPairsResponse
using SiliconeTrader.Machine.Client.Models; // For MarketSignalsResponse

namespace SiliconeTrader.Machine.Client.Core.Abstractions
{
    public interface IMarketManager
    {
        Task<MarketPairsResponse> GetMarketPairs(MarketPairsRequest request, CancellationToken cancellationToken);
        Task<MarketSignalsResponse> GetMarketSignals(CancellationToken cancellationToken);
    }
}