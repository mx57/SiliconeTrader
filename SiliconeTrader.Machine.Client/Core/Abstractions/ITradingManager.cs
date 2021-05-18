using System;
using System.Threading;
using System.Threading.Tasks;
using SiliconeTrader.Machine.Client.Models;

namespace SiliconeTrader.Machine.Client.Core.Abstractions
{
    public interface ITradingManager
    {
        Task<TradesViewModel> GetTrades(DateTimeOffset id, CancellationToken cancellationToken);

        Task<TradingPairResponse> GetTradingPairs(CancellationToken cancellationToken);
    }
}