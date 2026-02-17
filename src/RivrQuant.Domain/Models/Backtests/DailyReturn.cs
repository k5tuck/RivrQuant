namespace RivrQuant.Domain.Models.Backtests;

/// <summary>
/// Represents a single day's performance snapshot within a backtest,
/// used to construct equity curves and compute time-series-based risk metrics.
/// </summary>
public class DailyReturn
{
    /// <summary>
    /// Unique internal identifier for the daily return record.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Foreign key linking this record to its parent backtest result.
    /// </summary>
    public Guid BacktestResultId { get; init; }

    /// <summary>
    /// Date of the trading day this record represents.
    /// </summary>
    public DateTimeOffset Date { get; init; }

    /// <summary>
    /// Total portfolio equity at the close of this trading day.
    /// </summary>
    public decimal Equity { get; init; }

    /// <summary>
    /// Absolute profit or loss for this single trading day.
    /// </summary>
    public decimal DailyPnl { get; init; }

    /// <summary>
    /// Daily return expressed as a percentage (e.g., 0.02 for 2%).
    /// </summary>
    public decimal DailyReturnPercent { get; init; }

    /// <summary>
    /// Cumulative return from the start of the backtest to this day.
    /// </summary>
    public decimal CumulativeReturn { get; init; }

    /// <summary>
    /// Current drawdown from the peak equity, expressed as a negative decimal.
    /// </summary>
    public decimal Drawdown { get; init; }

    /// <summary>
    /// Benchmark equity value on this date for comparative analysis.
    /// </summary>
    public decimal BenchmarkEquity { get; init; }
}
