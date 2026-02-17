using RivrQuant.Domain.Enums;

namespace RivrQuant.Domain.Models.Trading;

/// <summary>
/// Represents a trading order submitted to a broker, tracking its full lifecycle
/// from creation through execution, cancellation, or rejection.
/// </summary>
public class Order
{
    /// <summary>
    /// Unique internal identifier for the order.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Identifier assigned by the broker or exchange for this order.
    /// </summary>
    public string ExternalOrderId { get; init; } = string.Empty;

    /// <summary>
    /// Client-generated identifier for order tracking and deduplication.
    /// </summary>
    public string ClientOrderId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Ticker symbol of the instrument being ordered.
    /// </summary>
    public string Symbol { get; init; } = string.Empty;

    /// <summary>
    /// Direction of the order (Buy or Sell).
    /// </summary>
    public OrderSide Side { get; init; }

    /// <summary>
    /// Type of the order, determining execution behavior (Market, Limit, StopLoss, etc.).
    /// </summary>
    public OrderType Type { get; init; }

    /// <summary>
    /// Current lifecycle status of the order.
    /// </summary>
    public OrderStatus Status { get; set; }

    /// <summary>
    /// Requested quantity to buy or sell.
    /// </summary>
    public decimal Quantity { get; init; }

    /// <summary>
    /// Limit price for Limit and StopLimit orders. Null for Market orders.
    /// </summary>
    public decimal? LimitPrice { get; init; }

    /// <summary>
    /// Stop/trigger price for StopLoss, StopLimit, and TrailingStop orders. Null for Market/Limit orders.
    /// </summary>
    public decimal? StopPrice { get; init; }

    /// <summary>
    /// Quantity that has been filled so far. Null if no fills have occurred.
    /// </summary>
    public decimal? FilledQuantity { get; set; }

    /// <summary>
    /// Volume-weighted average price across all fills. Null if no fills have occurred.
    /// </summary>
    public decimal? FilledAveragePrice { get; set; }

    /// <summary>
    /// Broker through which the order was submitted.
    /// </summary>
    public BrokerType Broker { get; init; }

    /// <summary>
    /// Asset class of the instrument being ordered.
    /// </summary>
    public AssetClass AssetClass { get; init; }

    /// <summary>
    /// Timestamp when the order was created and submitted.
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Timestamp when the order was completely filled. Null if not yet filled.
    /// </summary>
    public DateTimeOffset? FilledAt { get; set; }

    /// <summary>
    /// Timestamp when the order was cancelled. Null if not cancelled.
    /// </summary>
    public DateTimeOffset? CancelledAt { get; set; }

    /// <summary>
    /// Reason the order was rejected by the broker, if applicable.
    /// </summary>
    public string? RejectReason { get; set; }

    /// <summary>
    /// Collection of individual fill executions that make up this order's execution.
    /// </summary>
    public ICollection<Fill> Fills { get; init; } = new List<Fill>();
}
