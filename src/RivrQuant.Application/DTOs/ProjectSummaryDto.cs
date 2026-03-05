namespace RivrQuant.Application.DTOs;

/// <summary>
/// Summary of a QuantConnect project (algorithm/bot), aggregating stats
/// across all of its backtest runs stored in RivrQuant.
/// </summary>
public sealed record ProjectSummaryDto
{
    /// <summary>QuantConnect project ID (numeric string from the project URL).</summary>
    public string ProjectId { get; init; } = string.Empty;

    /// <summary>
    /// Human-readable algorithm/bot name from QuantConnect.
    /// Falls back to the ProjectId string if the name was never fetched.
    /// </summary>
    public string ProjectName { get; init; } = string.Empty;

    /// <summary>Total number of backtest runs imported for this project.</summary>
    public int BacktestCount { get; init; }

    /// <summary>Number of backtests that have been AI-analyzed.</summary>
    public int AnalyzedCount { get; init; }

    /// <summary>Best Sharpe ratio across all backtests for this project.</summary>
    public double? BestSharpe { get; init; }

    /// <summary>Best total return (decimal, e.g. 0.25 = 25%) across all backtests.</summary>
    public decimal BestTotalReturn { get; init; }

    /// <summary>CreatedAt timestamp of the most recent backtest for this project.</summary>
    public DateTimeOffset LatestBacktest { get; init; }
}
