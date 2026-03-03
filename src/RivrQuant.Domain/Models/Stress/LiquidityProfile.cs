namespace RivrQuant.Domain.Models.Stress;

/// <summary>
/// Represents the liquidity profile of a single asset, estimating the time and cost
/// required to liquidate a position based on average daily volume and market depth.
/// This is a Tier 3 stub; full market microstructure analysis is planned for a future release.
/// </summary>
/// <remarks>
/// Tier 3: Currently provides basic liquidity metrics. Future enhancements will include
/// intraday volume profiling, limit order book depth analysis, and dynamic market impact
/// modeling based on participation rate.
/// </remarks>
public sealed record LiquidityProfile
{
    /// <summary>
    /// Ticker symbol of the asset.
    /// </summary>
    public required string Symbol { get; init; }

    /// <summary>
    /// Average daily trading volume of the asset (in shares or units).
    /// </summary>
    public required decimal AvgDailyVolume { get; init; }

    /// <summary>
    /// Estimated time in minutes required to fully liquidate the current position
    /// at a reasonable participation rate without excessive market impact.
    /// </summary>
    public required decimal EstimatedLiquidationTimeMinutes { get; init; }

    /// <summary>
    /// Estimated market impact of liquidation in basis points, accounting for
    /// order size relative to average volume.
    /// </summary>
    public required decimal MarketImpactBps { get; init; }

    /// <summary>
    /// Timestamp as of which this liquidity profile was computed.
    /// </summary>
    public required DateTimeOffset AsOf { get; init; }
}
