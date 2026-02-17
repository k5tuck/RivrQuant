namespace RivrQuant.Domain.Models.Trading;

/// <summary>
/// Represents a time-stamped performance snapshot for live trading monitoring,
/// capturing equity, daily returns, drawdown, and position count.
/// </summary>
public class PerformanceSnapshot
{
    /// <summary>
    /// Unique internal identifier for the snapshot.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Total portfolio equity at the time of this snapshot.
    /// </summary>
    public decimal TotalEquity { get; init; }

    /// <summary>
    /// Absolute profit or loss for the current day at the time of this snapshot.
    /// </summary>
    public decimal DailyPnl { get; init; }

    /// <summary>
    /// Daily return expressed as a percentage at the time of this snapshot.
    /// </summary>
    public decimal DailyReturnPercent { get; init; }

    /// <summary>
    /// Cumulative return since the start of live trading.
    /// </summary>
    public decimal CumulativeReturn { get; init; }

    /// <summary>
    /// Current drawdown from the peak equity value, expressed as a decimal.
    /// </summary>
    public decimal CurrentDrawdown { get; init; }

    /// <summary>
    /// Number of open positions at the time of this snapshot.
    /// </summary>
    public int OpenPositionCount { get; init; }

    /// <summary>
    /// Timestamp when this performance snapshot was recorded.
    /// </summary>
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;
}
