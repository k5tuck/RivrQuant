namespace RivrQuant.Domain.Enums;

/// <summary>
/// Represents the severity level of a deleveraging action triggered by drawdown
/// or other risk events. Higher levels correspond to more aggressive risk reduction.
/// </summary>
public enum DeleverageLevel
{
    /// <summary>
    /// Normal operating conditions. No deleveraging required; full position sizing is permitted.
    /// </summary>
    Normal = 0,

    /// <summary>
    /// Caution level. Minor drawdown detected; position sizes may be modestly reduced
    /// and new entries may require additional confirmation.
    /// </summary>
    Caution = 1,

    /// <summary>
    /// Defensive level. Significant drawdown detected; position sizes are materially reduced
    /// and some strategies may be paused.
    /// </summary>
    Defensive = 2,

    /// <summary>
    /// Critical level. Severe drawdown detected; most new entries are halted and existing
    /// positions are actively reduced toward minimum exposure.
    /// </summary>
    Critical = 3,

    /// <summary>
    /// Emergency level. Extreme drawdown or system stress detected; all positions are
    /// liquidated and trading is suspended until manual review.
    /// </summary>
    Emergency = 4
}
