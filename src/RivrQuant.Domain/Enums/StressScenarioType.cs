namespace RivrQuant.Domain.Enums;

/// <summary>
/// Represents the type of stress test scenario applied to the portfolio.
/// Stress testing evaluates portfolio resilience under adverse market conditions.
/// </summary>
public enum StressScenarioType
{
    /// <summary>
    /// A historical replay scenario that applies actual market movements from a past
    /// crisis period (e.g., 2008 financial crisis, COVID-19 crash) to current positions.
    /// </summary>
    HistoricalReplay,

    /// <summary>
    /// A synthetic shock scenario that applies hypothetical but plausible market
    /// dislocations (e.g., a 20% equity drop combined with a volatility spike).
    /// </summary>
    SyntheticShock,

    /// <summary>
    /// A liquidity stress scenario that models the impact of reduced market liquidity
    /// on execution costs, slippage, and the ability to exit positions.
    /// <para>Tier 3 stub: full implementation is planned for a future release.</para>
    /// </summary>
    LiquidityStress
}
