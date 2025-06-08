using ExchangeSharp;
using SiliconeTrader.Core;
using SiliconeTrader.Exchange.Base;
using System;
using System.Linq;
using System.Collections.Generic;

namespace SiliconeTrader.Exchange.Binance
{
    internal class BinanceExchangeService : ExchangeService
    {
        public BinanceExchangeService(ILoggingService loggingService, IHealthCheckService healthCheckService, ITasksService tasksService) :
            base(loggingService, healthCheckService, tasksService)
        {

        }

        protected override ExchangeAPI InitializeApi()
        {
            var binanceApi = new ExchangeBinanceAPI
            {
                RateLimit = new RateGate(this.Config.RateLimitOccurences, TimeSpan.FromSeconds(this.Config.RateLimitTimeframe))
            };
            return binanceApi;
        }

        public override IOrderDetails PlaceOrder(IOrder order)
        {
            ExchangeOrderResult result = this.Api.PlaceOrderAsync(new ExchangeOrderRequest
            {
                OrderType = (ExchangeSharp.OrderType)(int)order.Type,
                IsBuy = order.Side == OrderSide.Buy,
                Amount = order.Amount,
                Price = order.Price,
                MarketSymbol = order.Pair // Changed Symbol to MarketSymbol
            }).Result;

            return new OrderDetails
            {
                Side = result.IsBuy ? OrderSide.Buy : OrderSide.Sell,
                Result = (OrderResult)(int)result.Result,
                Date = result.OrderDate,
                OrderId = result.OrderId,
                Pair = result.MarketSymbol, // Changed Symbol to MarketSymbol
                Message = result.Message,
                Amount = result.Amount, // Direct assignment (decimal to decimal)
                AmountFilled = result.AmountFilled ?? 0m, // Null-coalescing for decimal? to decimal
                Price = result.Price ?? 0m, // Null-coalescing for decimal? to decimal
                AveragePrice = result.AveragePrice ?? 0m, // Null-coalescing for decimal? to decimal
                Fees = result.Fees ?? 0m, // Null-coalescing for decimal? to decimal
                FeesCurrency = result.FeesCurrency
            };
        }

        public override IEnumerable<IOrderDetails> GetTrades(string pair)
        {
            // Replaced this.Config.TradesSyncMaxDays with a default value (e.g., 30)
            DateTime updatedSince = DateTime.UtcNow.Subtract(TimeSpan.FromDays(30));
            var myTrades = new List<OrderDetails>();
            // Changed GetMyTrades to GetCompletedOrderDetailsAsync
            IEnumerable<ExchangeOrderResult> results = this.Api.GetCompletedOrderDetailsAsync(pair, updatedSince).Result;

            if (results != null)
            {
                foreach (ExchangeOrderResult result in results)
                {
                    myTrades.Add(new OrderDetails
                    {
                        Side = result.IsBuy ? OrderSide.Buy : OrderSide.Sell,
                        Result = (OrderResult)(int)result.Result,
                        Date = result.OrderDate,
                        OrderId = result.OrderId,
                        Pair = result.MarketSymbol, // Changed Symbol to MarketSymbol
                        Message = result.Message,
                        Amount = result.Amount, // Direct assignment (decimal to decimal)
                        AmountFilled = result.AmountFilled ?? 0m, // Null-coalescing for decimal? to decimal
                        Price = result.Price ?? 0m, // Null-coalescing for decimal? to decimal
                        AveragePrice = result.AveragePrice ?? 0m, // Null-coalescing for decimal? to decimal
                        Fees = result.Fees ?? 0m, // Null-coalescing for decimal? to decimal
                        FeesCurrency = result.FeesCurrency
                    });
                }
            }

            return myTrades;
        }

        public override Arbitrage GetArbitrage(string pair, string tradingMarket, List<ArbitrageMarket> arbitrageMarkets = null, ArbitrageType? arbitrageType = null)
        {
            if (arbitrageMarkets == null || !arbitrageMarkets.Any())
            {
                arbitrageMarkets = new List<ArbitrageMarket> { ArbitrageMarket.ETH, ArbitrageMarket.BNB, ArbitrageMarket.USDT };
            }
            var arbitrage = new Arbitrage
            {
                Market = arbitrageMarkets.First(),
                Type = arbitrageType ?? ArbitrageType.Direct
            };

            try
            {
                if (tradingMarket == Constants.Markets.BTC)
                {
                    foreach (ArbitrageMarket market in arbitrageMarkets)
                    {
                        string marketPair = this.ChangeMarket(pair, market.ToString());
                        string arbitragePair = this.GetArbitrageMarketPair(market);

                        if (marketPair != pair &&
                            this.Tickers.TryGetValue(pair, out Ticker pairTicker) &&
                            this.Tickers.TryGetValue(marketPair, out Ticker marketTicker) &&
                            this.Tickers.TryGetValue(arbitragePair, out Ticker arbitrageTicker))
                        {
                            decimal directArbitragePercentage = 0;
                            decimal reverseArbitragePercentage = 0;

                            if (market == ArbitrageMarket.ETH)
                            {
                                directArbitragePercentage = (1 / pairTicker.AskPrice * marketTicker.BidPrice * arbitrageTicker.BidPrice - 1) * 100;
                                reverseArbitragePercentage = (1 / arbitrageTicker.AskPrice / marketTicker.AskPrice * pairTicker.BidPrice - 1) * 100;
                            }
                            else if (market == ArbitrageMarket.BNB)
                            {
                                directArbitragePercentage = (1 / pairTicker.AskPrice * marketTicker.BidPrice * arbitrageTicker.BidPrice - 1) * 100;
                                reverseArbitragePercentage = (1 / arbitrageTicker.AskPrice / marketTicker.AskPrice * pairTicker.BidPrice - 1) * 100;
                            }
                            else if (market == ArbitrageMarket.USDT)
                            {
                                directArbitragePercentage = (1 / pairTicker.AskPrice * marketTicker.BidPrice / arbitrageTicker.AskPrice - 1) * 100;
                                reverseArbitragePercentage = (arbitrageTicker.BidPrice / marketTicker.AskPrice * pairTicker.BidPrice - 1) * 100;
                            }

                            if ((directArbitragePercentage > arbitrage.Percentage || !arbitrage.IsAssigned) && (arbitrageType == null || arbitrageType == ArbitrageType.Direct))
                            {
                                arbitrage.IsAssigned = true;
                                arbitrage.Market = market;
                                arbitrage.Type = ArbitrageType.Direct;
                                arbitrage.Percentage = directArbitragePercentage;
                            }

                            if ((reverseArbitragePercentage > arbitrage.Percentage || !arbitrage.IsAssigned) && (arbitrageType == null || arbitrageType == ArbitrageType.Reverse))
                            {
                                arbitrage.IsAssigned = true;
                                arbitrage.Market = market;
                                arbitrage.Type = ArbitrageType.Reverse;
                                arbitrage.Percentage = reverseArbitragePercentage;
                            }
                        }
                    }
                }
            }
            catch { }
            return arbitrage;
        }

        public override string GetArbitrageMarketPair(ArbitrageMarket arbitrageMarket)
        {
            if (arbitrageMarket == ArbitrageMarket.ETH)
            {
                return Constants.Markets.ETH + Constants.Markets.BTC;
            }
            else if (arbitrageMarket == ArbitrageMarket.BNB)
            {
                return Constants.Markets.BNB + Constants.Markets.BTC;
            }
            else if (arbitrageMarket == ArbitrageMarket.USDT)
            {
                return Constants.Markets.BTC + Constants.Markets.USDT;
            }
            else
            {
                throw new NotSupportedException($"Unsupported arbitrage market: {arbitrageMarket}");
            }
        }
    }
}
