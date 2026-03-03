namespace RivrQuant.Domain.Models.Allocation;

/// <summary>
/// Represents the result of evaluating whether a portfolio rebalance is needed,
/// including drift analysis and the proposed allocation for each active strategy.
/// This is a computed value object produced by the capital allocator.
/// </summary>
public sealed record AllocationDecision
{
    /// <summary>
    /// Indicates whether a rebalance is recommended based on the current drift levels.
    /// </summary>
    public required bool RebalanceNeeded { get; init; }

    /// <summary>
    /// Maximum drift percentage observed across all strategy allocations.
    /// </summary>
    public required decimal MaxDriftPercent { get; init; }

    /// <summary>
    /// Human-readable explanation of the primary drift driver and rebalance recommendation.
    /// </summary>
    public required string DriftReason { get; init; }

    /// <summary>
    /// Collection of per-strategy allocation details, including target, current exposure, and drift.
    /// </summary>
    public required IReadOnlyList<StrategyAllocation> Allocations { get; init; }

    /// <summary>
    /// Timestamp when this allocation decision was evaluated.
    /// </summary>
    public required DateTimeOffset EvaluatedAt { get; init; }
}
