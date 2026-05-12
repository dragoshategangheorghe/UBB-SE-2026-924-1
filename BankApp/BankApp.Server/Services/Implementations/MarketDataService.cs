namespace BankApp.Server.Services.Implementations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using BankApp.Server.Services.Interfaces;

    public class MarketDataService : IMarketDataService
    {
        private const int DefaultPollingIntervalInMilliseconds = 5000;
        private const double MaximumPriceFluctuationPercentage = 0.04;
        private const double PriceFluctuationOffset = 0.02;
        private const decimal DefaultBtcPrice = 68000m;
        private const decimal DefaultEthPrice = 3400m;
        private const decimal DefaultAaplPrice = 185m;
        private const decimal DefaultMsftPrice = 420m;
        private const decimal DefaultGooglPrice = 155m;
        private const decimal DefaultTslaPrice = 650m;
        private const decimal DefaultSpyPrice = 520m;
        private const decimal PriceBaseMultiplier = 1m;
        private const int PriceRoundingDigits = 2;
        private const decimal MissingPrice = 0m;

        private readonly Dictionary<string, decimal> _currentPrices = new (StringComparer.OrdinalIgnoreCase)
        {
            ["BTC"] = DefaultBtcPrice,
            ["ETH"] = DefaultEthPrice,
            ["AAPL"] = DefaultAaplPrice,
            ["MSFT"] = DefaultMsftPrice,
            ["GOOGL"] = DefaultGooglPrice,
            ["TSLA"] = DefaultTslaPrice,
            ["SPY"] = DefaultSpyPrice
        };

        private readonly Random _randomNumberGenerator = new ();
        private readonly object _synchronizationRoot = new ();

        private Timer? _pollingTimer;
        private Action? _priceUpdateHandler;
        private List<string> _trackedTickerSymbols = new ();

        public void StartPolling(List<string> tickerSymbols)
        {
            lock (this._synchronizationRoot)
            {
                this._trackedTickerSymbols = tickerSymbols
                    .Where(ticker => !string.IsNullOrWhiteSpace(ticker))
                    .Select(ticker => ticker.Trim().ToUpperInvariant())
                    .Distinct()
                    .ToList();

                if (this._pollingTimer != null)
                {
                    return;
                }

                this._pollingTimer = new Timer(
                    timerState =>
                    {
                        lock (this._synchronizationRoot)
                        {
                            foreach (var ticker in this._trackedTickerSymbols)
                            {
                                if (!this._currentPrices.TryGetValue(ticker, out var currentPrice))
                                {
                                    continue;
                                }

                                var changePercentage =
                                    (decimal)((this._randomNumberGenerator.NextDouble() *
                                               MaximumPriceFluctuationPercentage) - PriceFluctuationOffset);
                                var updatedPrice = currentPrice * (PriceBaseMultiplier + changePercentage);
                                this._currentPrices[ticker] = decimal.Round(updatedPrice, PriceRoundingDigits);
                            }
                        }

                        this._priceUpdateHandler?.Invoke();
                    },
                    null,
                    DefaultPollingIntervalInMilliseconds,
                    DefaultPollingIntervalInMilliseconds);
            }
        }

        public void StopPolling()
        {
            lock (this._synchronizationRoot)
            {
                this._pollingTimer?.Dispose();
                this._pollingTimer = null;
            }
        }

        public decimal GetPrice(string tickerSymbol)
        {
            if (string.IsNullOrWhiteSpace(tickerSymbol))
            {
                return MissingPrice;
            }

            lock (this._synchronizationRoot)
            {
                return this._currentPrices.TryGetValue(tickerSymbol.Trim().ToUpperInvariant(), out var price) ? price : MissingPrice;
            }
        }

        public void RegisterPriceUpdateHandler(Action updateHandler)
        {
            this._priceUpdateHandler = updateHandler;
        }
    }
}