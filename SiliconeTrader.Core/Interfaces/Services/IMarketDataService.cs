using System.Threading.Tasks;
using SiliconeTrader.Core.Models; // Changed to Core.Models

namespace SiliconeTrader.Core.Interfaces.Services
{
    public interface IMarketDataService
    {
        Task<MarketPairsResponse> GetMarketPairsAsync(MarketPairsRequest request);
    }
}
