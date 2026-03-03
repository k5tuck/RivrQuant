using RivrQuant.Domain.Enums;

namespace RivrQuant.Domain.Models.Execution;

/// <summary>
/// Represents a computed aggregate report of execution quality over a specified time period,
/// summarizing fill analysis statistics across all trades. This is a read-only value object
/// generated on demand and is not persisted directly.
/// </summary>
public sealed record ExecutionReport
{
    /// <summary>
    /// Inclusive start of the reporting period.
    /// </summary>
    public required DateTimeOffset From { get; init; }

    /// <summary>
    /// Inclusive end of the reporting period.
    /// </summary>
    public required DateTimeOffset To { get; init; }

    /// <summary>
    /// Optional broker filter applied to this report. Null indicates all brokers are included.
    /// </summary>
    public required BrokerType? BrokerFilter { get; init; }

    /// <summary>
    /// Total number of fills analyzed in this report.
    /// </summary>
    public required int TotalFills { get; init; }

    /// <summary>
    /// Mean deviation between actual and estimated slippage in basis points across all fills.
    /// </summary>
    public required decimal MeanSlippageDeviationBps { get; init; }

    /// <summary>
    /// Standard deviation of slippage deviation in basis points, measuring model consistency.
    /// </summary>
    public required decimal StdDevSlippageDeviationBps { get; init; }

    /// <summary>
    /// Worst (highest) actual slippage observed in basis points during the period.
    /// </summary>
    public required decimal WorstSlippageBps { get; init; }

    /// <summary>
    /// Best (lowest) actual slippage observed in basis points during the period.
    /// </summary>
    public required decimal BestSlippageBps { get; init; }

    /// <summary>
    /// Total execution costs in dollar terms across all fills in the reporting period.
    /// </summary>
    public required decimal TotalCostDollars { get; init; }

    /// <summary>
    /// Average execution cost per trade in basis points.
    /// </summary>
    public required decimal AverageCostPerTradeBps { get; init; }

    /// <summary>
    /// Indicates whether the execution cost model is considered accurate,
    /// defined as mean slippage deviation less than 3 basis points.
    /// </summary>
    public required bool IsModelAccurate { get; init; }

    /// <summary>
    /// Collection of individual fill analyses included in this report.
    /// </summary>
    public required IReadOnlyList<FillAnalysis> Fills { get; init; }
}
