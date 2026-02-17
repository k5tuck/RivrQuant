// Copyright (c) RivrQuant. All rights reserved.
// Licensed under the MIT License.

using RivrQuant.Domain.Enums;
using RivrQuant.Domain.Models.Trading;

namespace RivrQuant.Domain.Interfaces;

/// <summary>
/// Provides a unified abstraction over broker-specific APIs for order management,
/// position tracking, and real-time portfolio updates.
/// </summary>
public interface IBrokerClient
{
    /// <summary>
    /// Gets the type of broker this client communicates with.
    /// </summary>
    BrokerType BrokerType { get; }

    /// <summary>
    /// Retrieves the current portfolio state, including cash balances,
    /// total equity, and buying power.
    /// </summary>
    /// <param name="ct">A cancellation token to cancel the asynchronous operation.</param>
    /// <returns>The current <see cref="Portfolio"/> snapshot.</returns>
    Task<Portfolio> GetPortfolioAsync(CancellationToken ct);

    /// <summary>
    /// Retrieves all open positions held under the brokerage account.
    /// </summary>
    /// <param name="ct">A cancellation token to cancel the asynchronous operation.</param>
    /// <returns>A read-only list of currently held <see cref="Position"/> entries.</returns>
    Task<IReadOnlyList<Position>> GetPositionsAsync(CancellationToken ct);

    /// <summary>
    /// Submits a new order to the broker for execution.
    /// </summary>
    /// <param name="request">
    /// The <see cref="OrderRequest"/> containing symbol, quantity, side, order type,
    /// and any limit/stop prices.
    /// </param>
    /// <param name="ct">A cancellation token to cancel the asynchronous operation.</param>
    /// <returns>The resulting <see cref="Order"/> with its broker-assigned identifier and status.</returns>
    Task<Order> PlaceOrderAsync(OrderRequest request, CancellationToken ct);

    /// <summary>
    /// Requests cancellation of an outstanding order.
    /// </summary>
    /// <param name="orderId">The broker-assigned order identifier.</param>
    /// <param name="ct">A cancellation token to cancel the asynchronous operation.</param>
    /// <returns>The updated <see cref="Order"/> reflecting the cancelled state.</returns>
    Task<Order> CancelOrderAsync(string orderId, CancellationToken ct);

    /// <summary>
    /// Closes the entire position for a given symbol by submitting
    /// an offsetting market order.
    /// </summary>
    /// <param name="symbol">The ticker symbol of the position to close.</param>
    /// <param name="ct">A cancellation token to cancel the asynchronous operation.</param>
    Task ClosePositionAsync(string symbol, CancellationToken ct);

    /// <summary>
    /// Closes all open positions in the brokerage account by submitting
    /// offsetting market orders for every held position.
    /// </summary>
    /// <param name="ct">A cancellation token to cancel the asynchronous operation.</param>
    Task CloseAllPositionsAsync(CancellationToken ct);

    /// <summary>
    /// Retrieves historical order records within the specified date range.
    /// </summary>
    /// <param name="from">The inclusive start of the date range (UTC).</param>
    /// <param name="to">The inclusive end of the date range (UTC).</param>
    /// <param name="ct">A cancellation token to cancel the asynchronous operation.</param>
    /// <returns>A read-only list of historical <see cref="Order"/> records.</returns>
    Task<IReadOnlyList<Order>> GetOrderHistoryAsync(DateTimeOffset from, DateTimeOffset to, CancellationToken ct);

    /// <summary>
    /// Subscribes to real-time portfolio performance updates from the broker.
    /// The callback is invoked each time the broker pushes a new snapshot.
    /// The subscription remains active until the <paramref name="ct"/> is cancelled.
    /// </summary>
    /// <param name="onUpdate">
    /// A callback invoked with each <see cref="PerformanceSnapshot"/> received from the broker.
    /// </param>
    /// <param name="ct">
    /// A cancellation token that, when cancelled, terminates the subscription.
    /// </param>
    Task SubscribeToUpdatesAsync(Action<PerformanceSnapshot> onUpdate, CancellationToken ct);
}
