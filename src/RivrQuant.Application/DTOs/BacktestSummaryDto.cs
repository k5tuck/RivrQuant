namespace RivrQuant.Application.DTOs;

/// <summary>
/// Lightweight summary of a backtest result for list views.
/// </summary>
public sealed record BacktestSummaryDto
{
    /// <summary>Internal identifier.</summary>
    public Guid Id { get; init; }

    /// <summary>External backtest identifier from the provider.</summary>
    public string ExternalBacktestId { get; init; } = string.Empty;

    /// <summary>Name of the strategy that was backtested.</summary>
    public string StrategyName { get; init; } = string.Empty;

    /// <summary>Start date of the backtest simulation period.</summary>
    public DateTimeOffset StartDate { get; init; }

    /// <summary>End date of the backtest simulation period.</summary>
    public DateTimeOffset EndDate { get; init; }

    /// <summary>Total return as a decimal.</summary>
    public decimal TotalReturn { get; init; }

    /// <summary>Final portfolio equity.</summary>
    public decimal FinalEquity { get; init; }

    /// <summary>Whether AI analysis has been performed.</summary>
    public bool IsAnalyzed { get; init; }

    /// <summary>Sharpe ratio, if metrics have been calculated.</summary>
    public double? SharpeRatio { get; init; }

    /// <summary>Maximum drawdown, if metrics have been calculated.</summary>
    public double? MaxDrawdown { get; init; }

    /// <summary>Total number of trades.</summary>
    public int TradeCount { get; init; }

    /// <summary>Timestamp when the backtest was created.</summary>
    public DateTimeOffset CreatedAt { get; init; }
}
