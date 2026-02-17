using RivrQuant.Domain.Enums;

namespace RivrQuant.Domain.Models.Backtests;

/// <summary>
/// Represents a single round-trip trade executed during a backtest,
/// capturing entry and exit details along with profit/loss calculations.
/// </summary>
public class BacktestTrade
{
    /// <summary>
    /// Unique internal identifier for the trade.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Foreign key linking this trade to its parent backtest result.
    /// </summary>
    public Guid BacktestResultId { get; init; }

    /// <summary>
    /// Ticker symbol of the instrument traded.
    /// </summary>
    public string Symbol { get; init; } = string.Empty;

    /// <summary>
    /// Timestamp when the trade position was opened.
    /// </summary>
    public DateTimeOffset EntryTime { get; init; }

    /// <summary>
    /// Timestamp when the trade position was closed.
    /// </summary>
    public DateTimeOffset ExitTime { get; init; }

    /// <summary>
    /// Price at which the position was entered.
    /// </summary>
    public decimal EntryPrice { get; init; }

    /// <summary>
    /// Price at which the position was exited.
    /// </summary>
    public decimal ExitPrice { get; init; }

    /// <summary>
    /// Number of shares or contracts traded.
    /// </summary>
    public decimal Quantity { get; init; }

    /// <summary>
    /// Direction of the trade (buy/long or sell/short).
    /// </summary>
    public OrderSide Side { get; init; }

    /// <summary>
    /// Absolute profit or loss in currency for this trade.
    /// </summary>
    public decimal ProfitLoss { get; init; }

    /// <summary>
    /// Profit or loss expressed as a percentage of the entry value.
    /// </summary>
    public decimal ProfitLossPercent { get; init; }

    /// <summary>
    /// Duration the position was held, computed from entry and exit times.
    /// </summary>
    public TimeSpan HoldingPeriod => ExitTime - EntryTime;

    /// <summary>
    /// Indicates whether this trade was profitable.
    /// </summary>
    public bool IsWin => ProfitLoss > 0;
}
