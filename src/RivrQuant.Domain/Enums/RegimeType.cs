namespace RivrQuant.Domain.Enums;

/// <summary>
/// Represents the detected market regime, used to adapt trading strategies
/// to prevailing market conditions.
/// </summary>
public enum RegimeType
{
    /// <summary>
    /// A trending regime where prices exhibit sustained directional movement,
    /// favoring momentum and trend-following strategies.
    /// </summary>
    Trending,

    /// <summary>
    /// A mean-reverting regime where prices oscillate around a central value,
    /// favoring contrarian and reversion strategies.
    /// </summary>
    MeanReverting,

    /// <summary>
    /// A high-volatility regime characterized by large price swings and uncertainty,
    /// requiring tighter risk controls and reduced position sizing.
    /// </summary>
    HighVolatility,

    /// <summary>
    /// A low-volatility regime characterized by small price movements and stability,
    /// allowing for larger position sizes and wider stop levels.
    /// </summary>
    LowVolatility,

    /// <summary>
    /// A crisis regime indicating extreme market stress, sharp drawdowns,
    /// and heightened correlation across assets, requiring defensive positioning.
    /// </summary>
    Crisis
}
