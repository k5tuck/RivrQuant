namespace RivrQuant.Domain.Models.Backtests;

/// <summary>
/// Represents the complete result of a backtest execution, including equity curves,
/// trade history, and computed performance metrics.
/// </summary>
public class BacktestResult
{
    /// <summary>
    /// Unique internal identifier for the backtest result.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// External identifier assigned by the backtest provider (e.g., QuantConnect backtest ID).
    /// </summary>
    public string ExternalBacktestId { get; init; } = string.Empty;

    /// <summary>
    /// Identifier of the project or workspace that owns this backtest.
    /// </summary>
    public string ProjectId { get; init; } = string.Empty;

    /// <summary>
    /// Human-readable name of the QuantConnect project (algorithm / bot).
    /// Populated from the QC projects API during polling; null for records
    /// imported before this field was added.
    /// </summary>
    public string? ProjectName { get; set; }

    /// <summary>
    /// Name of the strategy that was backtested.
    /// </summary>
    public string StrategyName { get; init; } = string.Empty;

    /// <summary>
    /// Optional description of the strategy or backtest configuration.
    /// </summary>
    public string? StrategyDescription { get; init; }

    /// <summary>
    /// Start date of the backtest simulation period.
    /// </summary>
    public DateTimeOffset StartDate { get; init; }

    /// <summary>
    /// End date of the backtest simulation period.
    /// </summary>
    public DateTimeOffset EndDate { get; init; }

    /// <summary>
    /// Timestamp when this backtest result was created and persisted.
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Initial capital allocated to the strategy at the start of the backtest.
    /// </summary>
    public decimal InitialCapital { get; init; }

    /// <summary>
    /// Final portfolio equity at the end of the backtest simulation.
    /// </summary>
    public decimal FinalEquity { get; init; }

    /// <summary>
    /// Total return as a decimal (e.g., 0.15 for 15% return).
    /// </summary>
    public decimal TotalReturn { get; init; }

    /// <summary>
    /// Indicates whether AI analysis has been performed on this backtest result.
    /// </summary>
    public bool IsAnalyzed { get; set; }

    /// <summary>
    /// Collection of individual trades executed during the backtest.
    /// </summary>
    public ICollection<BacktestTrade> Trades { get; init; } = new List<BacktestTrade>();

    /// <summary>
    /// Collection of daily return snapshots capturing the equity curve over time.
    /// </summary>
    public ICollection<DailyReturn> DailyReturns { get; init; } = new List<DailyReturn>();

    /// <summary>
    /// Aggregated performance metrics computed from trades and daily returns.
    /// Null until metrics have been calculated.
    /// </summary>
    public BacktestMetrics? Metrics { get; set; }
}
