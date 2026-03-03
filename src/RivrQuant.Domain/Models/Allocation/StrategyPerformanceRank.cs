namespace RivrQuant.Domain.Models.Allocation;

/// <summary>
/// Represents the performance ranking of a single strategy based on rolling Sharpe ratios,
/// used by the capital allocator to adjust allocations toward better-performing strategies.
/// This is a computed value object.
/// </summary>
public sealed record StrategyPerformanceRank
{
    /// <summary>
    /// Unique identifier of the ranked strategy.
    /// </summary>
    public required Guid StrategyId { get; init; }

    /// <summary>
    /// Human-readable name of the strategy.
    /// </summary>
    public required string StrategyName { get; init; }

    /// <summary>
    /// Rolling 30-day Sharpe ratio of the strategy.
    /// </summary>
    public required double RollingSharpe30d { get; init; }

    /// <summary>
    /// Rolling 60-day Sharpe ratio of the strategy.
    /// </summary>
    public required double RollingSharpe60d { get; init; }

    /// <summary>
    /// Ordinal rank of the strategy (1 = best performing).
    /// </summary>
    public required int Rank { get; init; }

    /// <summary>
    /// Indicates whether the strategy is currently active and eligible for capital allocation.
    /// </summary>
    public required bool IsActive { get; init; }

    /// <summary>
    /// Timestamp when this ranking was computed.
    /// </summary>
    public required DateTimeOffset RankedAt { get; init; }
}
