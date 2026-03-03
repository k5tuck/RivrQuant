namespace RivrQuant.Domain.Enums;

/// <summary>
/// Represents the method used to determine position size for a trade.
/// Different methods offer varying trade-offs between capital efficiency,
/// risk control, and theoretical optimality.
/// </summary>
public enum PositionSizingMethod
{
    /// <summary>
    /// Kelly criterion sizing, which maximizes the expected logarithmic growth rate
    /// of capital based on historical win rate and payoff ratio.
    /// Typically applied as a fractional Kelly (e.g., half-Kelly) to reduce variance.
    /// </summary>
    Kelly,

    /// <summary>
    /// Volatility-target sizing, which scales position size inversely with the asset's
    /// realized or forecast volatility to maintain a constant portfolio risk level.
    /// </summary>
    VolatilityTarget,

    /// <summary>
    /// Fixed-fractional sizing, which risks a constant percentage of portfolio equity
    /// on each trade, providing consistent risk exposure regardless of market conditions.
    /// </summary>
    FixedFractional,

    /// <summary>
    /// Risk-parity sizing, which allocates capital so that each position contributes
    /// equally to overall portfolio risk, measured by volatility or variance.
    /// </summary>
    RiskParity,

    /// <summary>
    /// Composite sizing, which blends multiple position sizing methods and selects
    /// the final size based on a weighted combination or the most conservative result.
    /// </summary>
    Composite
}
