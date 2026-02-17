// Copyright (c) RivrQuant. All rights reserved.
// Licensed under the MIT License.

using RivrQuant.Domain.Models.Market;

namespace RivrQuant.Domain.Interfaces;

/// <summary>
/// Provides access to historical and real-time market data, including OHLCV bars
/// and streaming quote subscriptions.
/// </summary>
public interface IMarketDataProvider
{
    /// <summary>
    /// Retrieves historical price bars for the specified symbol and date range.
    /// </summary>
    /// <param name="symbol">The ticker symbol to query (e.g., "AAPL", "SPY").</param>
    /// <param name="from">The inclusive start of the date range (UTC).</param>
    /// <param name="to">The inclusive end of the date range (UTC).</param>
    /// <param name="resolution">
    /// The bar resolution (e.g., "1Min", "5Min", "15Min", "1Hour", "1Day").
    /// </param>
    /// <param name="ct">A cancellation token to cancel the asynchronous operation.</param>
    /// <returns>
    /// A read-only list of <see cref="PriceBar"/> entries ordered chronologically.
    /// </returns>
    Task<IReadOnlyList<PriceBar>> GetHistoricalBarsAsync(
        string symbol,
        DateTimeOffset from,
        DateTimeOffset to,
        string resolution,
        CancellationToken ct);

    /// <summary>
    /// Retrieves the most recent completed price bar for the specified symbol.
    /// </summary>
    /// <param name="symbol">The ticker symbol to query.</param>
    /// <param name="ct">A cancellation token to cancel the asynchronous operation.</param>
    /// <returns>The latest <see cref="PriceBar"/> for the symbol.</returns>
    Task<PriceBar> GetLatestBarAsync(string symbol, CancellationToken ct);

    /// <summary>
    /// Subscribes to real-time quote updates for one or more symbols.
    /// The callback is invoked each time a new bar or quote tick is received.
    /// The subscription remains active until the <paramref name="ct"/> is cancelled.
    /// </summary>
    /// <param name="symbols">A read-only list of ticker symbols to subscribe to.</param>
    /// <param name="onQuote">
    /// A callback invoked with each <see cref="PriceBar"/> received from the data feed.
    /// </param>
    /// <param name="ct">
    /// A cancellation token that, when cancelled, terminates the subscription
    /// and releases underlying resources.
    /// </param>
    Task SubscribeToQuotesAsync(
        IReadOnlyList<string> symbols,
        Action<PriceBar> onQuote,
        CancellationToken ct);
}
