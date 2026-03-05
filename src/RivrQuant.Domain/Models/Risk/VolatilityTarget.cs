namespace RivrQuant.Domain.Models.Risk;

/// <summary>
/// Represents a volatility targeting snapshot, comparing the portfolio's realized
/// annualized volatility against the target level and computing an adjustment multiplier.
/// Persisted to the database as periodic snapshots for trend analysis and audit.
/// </summary>
public class VolatilityTarget
{
    /// <summary>
    /// Unique internal identifier for the volatility target snapshot.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Target annualized volatility for the portfolio, expressed as a decimal (e.g., 0.15 for 15%).
    /// </summary>
    public decimal TargetAnnualizedVol { get; init; }

    /// <summary>
    /// Current realized annualized volatility of the portfolio, expressed as a decimal.
    /// </summary>
    public decimal RealizedAnnualizedVol { get; init; }

    /// <summary>
    /// Multiplier applied to position sizes to bring realized volatility in line with the target.
    /// Values less than 1.0 indicate positions should be reduced; values greater than 1.0
    /// indicate capacity for larger positions.
    /// </summary>
    public decimal VolMultiplier { get; init; }

    /// <summary>
    /// Previously observed realized annualized volatility, used to detect volatility regime changes.
    /// </summary>
    public decimal PreviousRealizedVol { get; init; }

    /// <summary>
    /// Indicates whether realized volatility is expanding (increasing) relative to the previous observation.
    /// </summary>
    public bool IsVolExpanding { get; init; }

    /// <summary>
    /// Indicates whether realized volatility is contracting (decreasing) relative to the previous observation.
    /// </summary>
    public bool IsVolContracting { get; init; }

    /// <summary>
    /// Current volatility regime classification.
    /// 1 = Low, 2 = Normal, 3 = High, 4 = Extreme.
    /// </summary>
    public int VolRegime { get; init; }

    /// <summary>
    /// Timestamp when this volatility target snapshot was calculated.
    /// </summary>
    public DateTimeOffset CalculatedAt { get; init; } = DateTimeOffset.UtcNow;
}
