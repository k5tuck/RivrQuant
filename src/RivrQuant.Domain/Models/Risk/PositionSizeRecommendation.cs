using RivrQuant.Domain.Enums;

namespace RivrQuant.Domain.Models.Risk;

/// <summary>
/// Represents the output of a position sizing calculation, including the recommended
/// quantity, the method used, and details of any drawdown or volatility adjustments applied.
/// This is a computed value object returned by <see cref="Interfaces.IPositionSizer"/>.
/// </summary>
public sealed record PositionSizeRecommendation
{
    /// <summary>
    /// Ticker symbol of the instrument for which the position size was calculated.
    /// </summary>
    public required string Symbol { get; init; }

    /// <summary>
    /// Recommended quantity of shares or units to trade after all adjustments.
    /// </summary>
    public required decimal RecommendedQuantity { get; init; }

    /// <summary>
    /// Recommended notional value of the trade (RecommendedQuantity * current price).
    /// </summary>
    public required decimal RecommendedNotional { get; init; }

    /// <summary>
    /// Recommended position size as a percentage of total portfolio equity.
    /// </summary>
    public required decimal PercentOfPortfolio { get; init; }

    /// <summary>
    /// Dollar amount of risk allocated to this trade (e.g., distance to stop * quantity).
    /// </summary>
    public required decimal RiskPerTrade { get; init; }

    /// <summary>
    /// The position sizing method that produced this recommendation.
    /// </summary>
    public required PositionSizingMethod MethodUsed { get; init; }

    /// <summary>
    /// Human-readable explanation of the sizing logic and any constraints that were applied.
    /// </summary>
    public required string Reasoning { get; init; }

    /// <summary>
    /// Indicates whether the final quantity was reduced due to the current drawdown level.
    /// </summary>
    public required bool WasReducedByDrawdown { get; init; }

    /// <summary>
    /// Indicates whether the final quantity was reduced due to the volatility target adjustment.
    /// </summary>
    public required bool WasReducedByVolTarget { get; init; }

    /// <summary>
    /// Quantity calculated before drawdown and volatility adjustments were applied.
    /// </summary>
    public required decimal PreAdjustmentQuantity { get; init; }

    /// <summary>
    /// Multiplier applied from the drawdown manager (1.0 = no reduction, 0.5 = halved).
    /// </summary>
    public required decimal DrawdownMultiplier { get; init; }

    /// <summary>
    /// Multiplier applied from the volatility target engine (1.0 = no reduction).
    /// </summary>
    public required decimal VolatilityMultiplier { get; init; }

    /// <summary>
    /// Timestamp when this recommendation was calculated.
    /// </summary>
    public required DateTimeOffset CalculatedAt { get; init; }
}
