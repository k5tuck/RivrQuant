namespace RivrQuant.Domain.Enums;

/// <summary>
/// Represents the reason that triggered a deleveraging event.
/// Used for audit logging and to determine appropriate recovery actions.
/// </summary>
public enum DeleverageReason
{
    /// <summary>
    /// Deleveraging triggered because portfolio drawdown exceeded a configured threshold.
    /// </summary>
    MaxDrawdown,

    /// <summary>
    /// Deleveraging triggered because realized or implied volatility spiked beyond
    /// acceptable levels, indicating heightened market stress.
    /// </summary>
    VolatilitySpike,

    /// <summary>
    /// Deleveraging triggered because cross-asset correlations surged, reducing
    /// diversification benefits and increasing portfolio tail risk.
    /// </summary>
    CorrelationSpike,

    /// <summary>
    /// Deleveraging triggered because strategy performance decay was detected,
    /// indicating potential regime change or model degradation.
    /// </summary>
    DecayDetected,

    /// <summary>
    /// Deleveraging triggered manually by an operator or administrator override.
    /// </summary>
    Manual
}
