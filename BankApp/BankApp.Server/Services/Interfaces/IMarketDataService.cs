namespace BankApp.Server.Services.Interfaces
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Defines operations for polling and retrieving real-time market data.
    /// </summary>
    public interface IMarketDataService
    {
        /// <summary>
        /// Starts the price update timer for the specified tickers.
        /// </summary>
        void StartPolling(List<string> tickerSymbols);

        /// <summary>
        /// Stops the price update timer.
        /// </summary>
        void StopPolling();

        /// <summary>
        /// Gets the current simulated price for a specific ticker.
        /// </summary>
        decimal GetPrice(string tickerSymbol);

        /// <summary>
        /// Registers a callback for when prices are updated.
        /// </summary>
        void RegisterPriceUpdateHandler(Action updateHandler);
    }
}