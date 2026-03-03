namespace RivrQuant.Domain.Models.Exposure;

/// <summary>
/// Represents the aggregate exposure to a single industry sector within the portfolio,
/// summarizing the total notional value and number of positions in that sector.
/// This is a computed value object used within portfolio exposure calculations.
/// </summary>
public sealed record SectorExposure
{
    /// <summary>
    /// Name of the industry sector (e.g., "Technology", "Healthcare", "Energy").
    /// </summary>
    public required string Sector { get; init; }

    /// <summary>
    /// Total notional (dollar) value of all positions within this sector.
    /// </summary>
    public required decimal NotionalValue { get; init; }

    /// <summary>
    /// Sector exposure as a percentage of total portfolio value.
    /// </summary>
    public required decimal PercentOfPortfolio { get; init; }

    /// <summary>
    /// Number of individual positions held within this sector.
    /// </summary>
    public required int PositionCount { get; init; }
}
