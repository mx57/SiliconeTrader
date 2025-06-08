using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SiliconeTrader.Core
{
    public interface IExchangeService : IConfigurableService
    {
        Task Start(bool virtualTrading);
        void Stop();
        IOrderDetails PlaceOrder(IOrder order);
        Task<decimal> ClampOrderAmount(string pair, decimal amount);
        Task<decimal> ClampOrderPrice(string pair, decimal price);
        Task ConnectTickersWebsocket();
        void DisconnectTickersWebsocket();
        IEnumerable<ITicker> GetTickers();
        IEnumerable<string> GetMarkets();
        IEnumerable<string> GetMarketPairs(string market);
        Dictionary<string, decimal> GetAvailableAmounts();
        Task<IEnumerable<IOrderDetails>> GetTrades(string pair);
        decimal GetPrice(string pair, TradePriceType priceType);
        decimal GetPriceSpread(string pair);
        Arbitrage GetArbitrage(string pair, string tradingMarket, List<ArbitrageMarket> arbitrageMarkets = null, ArbitrageType? arbitrageType = null);
        string GetArbitrageMarketPair(ArbitrageMarket arbitrageMarket);
        string GetPairMarket(string pair);
        string ChangeMarket(string pair, string market);
        decimal ConvertPrice(string pair, decimal price, string market, TradePriceType priceType);
        TimeSpan GetTimeElapsedSinceLastTickersUpdate();
    }
}
