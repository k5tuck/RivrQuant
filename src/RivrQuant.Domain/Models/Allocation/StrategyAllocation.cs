namespace RivrQuant.Domain.Models.Allocation;

/// <summary>
/// Represents the capital allocation assigned to a single strategy, including the
/// allocated amount, current exposure, and any drift from the target allocation.
/// This is a computed value object produced by the capital allocator.
/// </summary>
public sealed record StrategyAllocation
{
    /// <summary>
    /// Unique identifier of the strategy receiving the allocation.
    /// </summary>
    public required Guid StrategyId { get; init; }

    /// <summary>
    /// Human-readable name of the strategy.
    /// </summary>
    public required string StrategyName { get; init; }

    /// <summary>
    /// Dollar amount of capital allocated to this strategy.
    /// </summary>
    public required decimal AllocatedCapital { get; init; }

    /// <summary>
    /// Allocated capital as a percentage of total portfolio value.
    /// </summary>
    public required decimal AllocatedPercent { get; init; }

    /// <summary>
    /// Current notional exposure of the strategy's open positions.
    /// </summary>
    public required decimal CurrentExposure { get; init; }

    /// <summary>
    /// Drift from target allocation expressed as a percentage (current minus target).
    /// Positive drift indicates overallocation; negative drift indicates underallocation.
    /// </summary>
    public required decimal DriftPercent { get; init; }

    /// <summary>
    /// Human-readable explanation of the allocation decision and any adjustments applied.
    /// </summary>
    public required string Reasoning { get; init; }
}
