namespace RivrQuant.Domain.Models.Stress;

/// <summary>
/// Represents the result of running a stress test scenario against the current portfolio,
/// including the estimated impact on portfolio value and the worst-hit position.
/// This is a Tier 3 stub; advanced result decomposition is planned for a future release.
/// </summary>
/// <remarks>
/// Tier 3: Currently provides top-level impact metrics. Future enhancements will include
/// per-position impact breakdown, P&amp;L attribution by factor, and time-series impact
/// trajectories for multi-day stress scenarios.
/// </remarks>
public sealed record StressTestResult
{
    /// <summary>
    /// Name of the scenario that produced this result.
    /// </summary>
    public required string ScenarioName { get; init; }

    /// <summary>
    /// Estimated dollar impact on total portfolio value under the stress scenario (negative for losses).
    /// </summary>
    public required decimal PortfolioImpactDollars { get; init; }

    /// <summary>
    /// Estimated percentage impact on total portfolio value under the stress scenario (negative for losses).
    /// </summary>
    public required decimal PortfolioImpactPercent { get; init; }

    /// <summary>
    /// Percentage impact on the worst-affected individual position (negative for losses).
    /// </summary>
    public required decimal WorstPositionImpactPercent { get; init; }

    /// <summary>
    /// Ticker symbol of the position with the worst estimated impact under the stress scenario.
    /// </summary>
    public required string WorstPositionSymbol { get; init; }

    /// <summary>
    /// Timestamp when this stress test was executed.
    /// </summary>
    public required DateTimeOffset RunAt { get; init; }
}
