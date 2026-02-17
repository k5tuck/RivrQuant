using RivrQuant.Domain.Enums;

namespace RivrQuant.Domain.Models.Trading;

/// <summary>
/// Represents a point-in-time snapshot of a brokerage account's portfolio,
/// including equity, cash balance, and daily performance metrics.
/// </summary>
public class Portfolio
{
    /// <summary>
    /// Total portfolio equity including all positions and cash.
    /// </summary>
    public decimal TotalEquity { get; init; }

    /// <summary>
    /// Available cash balance not currently invested in positions.
    /// </summary>
    public decimal CashBalance { get; init; }

    /// <summary>
    /// Total buying power available, accounting for margin if applicable.
    /// </summary>
    public decimal BuyingPower { get; init; }

    /// <summary>
    /// Total unrealized profit or loss across all open positions.
    /// </summary>
    public decimal UnrealizedPnl { get; init; }

    /// <summary>
    /// Realized profit or loss from trades closed during the current trading day.
    /// </summary>
    public decimal RealizedPnlToday { get; init; }

    /// <summary>
    /// Percentage change in portfolio value for the current trading day.
    /// </summary>
    public decimal DailyChangePercent { get; init; }

    /// <summary>
    /// Broker from which this portfolio snapshot was retrieved.
    /// </summary>
    public BrokerType Broker { get; init; }

    /// <summary>
    /// Timestamp indicating when this portfolio snapshot was taken.
    /// </summary>
    public DateTimeOffset AsOf { get; init; } = DateTimeOffset.UtcNow;
}
