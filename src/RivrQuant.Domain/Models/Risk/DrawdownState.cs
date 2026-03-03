using RivrQuant.Domain.Enums;

namespace RivrQuant.Domain.Models.Risk;

/// <summary>
/// Represents the current drawdown state of the portfolio, including peak-to-trough
/// metrics, the active deleverage level, and any strategies that have been paused
/// due to drawdown rules. This is a computed value object recalculated on each evaluation cycle.
/// </summary>
public sealed record DrawdownState
{
    /// <summary>
    /// Peak portfolio equity recorded since the last high-water mark reset.
    /// </summary>
    public required decimal PeakEquity { get; init; }

    /// <summary>
    /// Current portfolio equity at the time of evaluation.
    /// </summary>
    public required decimal CurrentEquity { get; init; }

    /// <summary>
    /// Current drawdown expressed as a negative percentage (e.g., -8.5 means 8.5% below peak).
    /// </summary>
    public required decimal DrawdownPercent { get; init; }

    /// <summary>
    /// Current drawdown expressed in dollar terms (negative value).
    /// </summary>
    public required decimal DrawdownDollars { get; init; }

    /// <summary>
    /// Date when the peak equity was recorded.
    /// </summary>
    public required DateTimeOffset PeakDate { get; init; }

    /// <summary>
    /// Number of calendar days the portfolio has been in drawdown from the peak.
    /// </summary>
    public required int DaysInDrawdown { get; init; }

    /// <summary>
    /// The currently active deleverage level based on drawdown severity thresholds.
    /// </summary>
    public required DeleverageLevel ActiveLevel { get; init; }

    /// <summary>
    /// The position sizing multiplier corresponding to the active deleverage level.
    /// A value of 1.0 indicates full sizing; 0.5 indicates half sizing; 0.0 indicates fully deleveraged.
    /// </summary>
    public required decimal CurrentMultiplier { get; init; }

    /// <summary>
    /// List of strategy names that have been paused due to the current drawdown level.
    /// </summary>
    public required IReadOnlyList<string> PausedStrategies { get; init; }

    /// <summary>
    /// Timestamp when this drawdown state was last calculated.
    /// </summary>
    public required DateTimeOffset LastUpdated { get; init; }
}
