using RivrQuant.Domain.Models.Backtests;
using RivrQuant.Domain.Models.Analysis;

namespace RivrQuant.Application.DTOs;

/// <summary>
/// Full detail view of a backtest result including metrics, trades, daily returns,
/// and associated analysis reports.
/// </summary>
public sealed record BacktestDetailDto
{
    /// <summary>Internal identifier.</summary>
    public Guid Id { get; init; }

    /// <summary>External backtest identifier from the provider.</summary>
    public string ExternalBacktestId { get; init; } = string.Empty;

    /// <summary>Project identifier.</summary>
    public string ProjectId { get; init; } = string.Empty;

    /// <summary>Name of the strategy.</summary>
    public string StrategyName { get; init; } = string.Empty;

    /// <summary>Optional description.</summary>
    public string? StrategyDescription { get; init; }

    /// <summary>Start date of the simulation.</summary>
    public DateTimeOffset StartDate { get; init; }

    /// <summary>End date of the simulation.</summary>
    public DateTimeOffset EndDate { get; init; }

    /// <summary>Initial capital.</summary>
    public decimal InitialCapital { get; init; }

    /// <summary>Final equity.</summary>
    public decimal FinalEquity { get; init; }

    /// <summary>Total return.</summary>
    public decimal TotalReturn { get; init; }

    /// <summary>Whether AI analysis has been performed.</summary>
    public bool IsAnalyzed { get; init; }

    /// <summary>Calculated performance metrics.</summary>
    public BacktestMetrics? Metrics { get; init; }

    /// <summary>Individual trades.</summary>
    public IReadOnlyList<BacktestTrade> Trades { get; init; } = Array.Empty<BacktestTrade>();

    /// <summary>Daily return snapshots.</summary>
    public IReadOnlyList<DailyReturn> DailyReturns { get; init; } = Array.Empty<DailyReturn>();

    /// <summary>AI analysis report, if available.</summary>
    public AiAnalysisReport? AnalysisReport { get; init; }

    /// <summary>Timestamp when the backtest was created.</summary>
    public DateTimeOffset CreatedAt { get; init; }
}
