using System.Threading;
using System.Threading.Tasks;
using SiliconeTrader.Machine.Client.Models;

namespace SiliconeTrader.Machine.Client.Core.Abstractions
{
    public interface IMarketManager
    {
        Task<MarketPairsResponse> GetMarketPairs(MarketPairsRequest request, CancellationToken cancellationToken);
        Task<MarketSignalsResponse> GetMarketSignals(CancellationToken cancellationToken);
    }
}