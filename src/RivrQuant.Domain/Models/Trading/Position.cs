using RivrQuant.Domain.Enums;

namespace RivrQuant.Domain.Models.Trading;

/// <summary>
/// Represents an open trading position, tracking quantity, cost basis, and
/// real-time unrealized profit/loss calculations.
/// </summary>
public class Position
{
    /// <summary>
    /// Unique internal identifier for the position.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Ticker symbol of the instrument held.
    /// </summary>
    public string Symbol { get; init; } = string.Empty;

    /// <summary>
    /// Direction of the position (long via Buy or short via Sell).
    /// </summary>
    public OrderSide Side { get; init; }

    /// <summary>
    /// Current quantity of the position. May change due to partial fills or scaling.
    /// </summary>
    public decimal Quantity { get; set; }

    /// <summary>
    /// Volume-weighted average price at which the position was entered.
    /// </summary>
    public decimal AverageEntryPrice { get; init; }

    /// <summary>
    /// Most recent market price of the instrument, updated in real time.
    /// </summary>
    public decimal CurrentPrice { get; set; }

    /// <summary>
    /// Unrealized profit or loss in currency, calculated from entry price, current price,
    /// quantity, and position direction.
    /// </summary>
    public decimal UnrealizedPnl => (CurrentPrice - AverageEntryPrice) * Quantity * (Side == OrderSide.Buy ? 1 : -1);

    /// <summary>
    /// Unrealized profit or loss as a percentage of the position's cost basis.
    /// Returns zero if the average entry price is zero to avoid division errors.
    /// </summary>
    public decimal UnrealizedPnlPercent => AverageEntryPrice != 0 ? UnrealizedPnl / (AverageEntryPrice * Quantity) * 100 : 0;

    /// <summary>
    /// Current market value of the position (current price multiplied by quantity).
    /// </summary>
    public decimal MarketValue => CurrentPrice * Quantity;

    /// <summary>
    /// Broker through which the position is held.
    /// </summary>
    public BrokerType Broker { get; init; }

    /// <summary>
    /// Asset class of the instrument (e.g., Stock, Crypto).
    /// </summary>
    public AssetClass AssetClass { get; init; }

    /// <summary>
    /// Timestamp when the position was originally opened.
    /// </summary>
    public DateTimeOffset OpenedAt { get; init; } = DateTimeOffset.UtcNow;
}
