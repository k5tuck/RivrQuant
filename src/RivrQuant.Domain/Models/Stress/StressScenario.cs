using RivrQuant.Domain.Enums;

namespace RivrQuant.Domain.Models.Stress;

/// <summary>
/// Represents a stress test scenario definition, specifying the type and magnitude of
/// market shocks to apply to the portfolio. This is a Tier 3 stub; full scenario
/// libraries and historical event databases are planned for a future release.
/// </summary>
/// <remarks>
/// Tier 3: Currently supports basic scenario definitions. Future enhancements will include
/// multi-factor shock vectors, cross-asset contagion modeling, and integration with
/// historical crisis event databases.
/// </remarks>
public sealed record StressScenario
{
    /// <summary>
    /// Human-readable name of the stress scenario (e.g., "2008 Financial Crisis", "Flash Crash").
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Type of stress scenario (HistoricalReplay, SyntheticShock, or LiquidityStress).
    /// </summary>
    public required StressScenarioType Type { get; init; }

    /// <summary>
    /// Detailed description of the scenario and the market conditions it models.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Hypothetical market drop as a percentage (e.g., -20.0 for a 20% decline).
    /// Null for scenarios that do not model a direct market decline.
    /// </summary>
    public decimal? MarketDropPercent { get; init; }

    /// <summary>
    /// Multiplier applied to current volatility levels (e.g., 2.5 for 150% increase in vol).
    /// Null for scenarios that do not model a volatility shock.
    /// </summary>
    public decimal? VolMultiplier { get; init; }

    /// <summary>
    /// Multiplier applied to current bid-ask spreads (e.g., 3.0 for spreads tripling).
    /// Null for scenarios that do not model a spread shock.
    /// </summary>
    public decimal? SpreadMultiplier { get; init; }
}
