using RivrQuant.Domain.Enums;

namespace RivrQuant.Domain.Models.Exposure;

/// <summary>
/// Represents the exposure contribution of a single asset within the portfolio,
/// including notional value, beta, and sector classification. This is a computed
/// value object used within portfolio exposure calculations.
/// </summary>
public sealed record AssetExposure
{
    /// <summary>
    /// Ticker symbol of the asset.
    /// </summary>
    public required string Symbol { get; init; }

    /// <summary>
    /// Asset class of the instrument (e.g., Stock, Crypto).
    /// </summary>
    public required AssetClass AssetClass { get; init; }

    /// <summary>
    /// Notional (dollar) value of the position in this asset.
    /// </summary>
    public required decimal NotionalValue { get; init; }

    /// <summary>
    /// Position size as a percentage of total portfolio value.
    /// </summary>
    public required decimal PercentOfPortfolio { get; init; }

    /// <summary>
    /// Beta coefficient of the asset relative to the market benchmark.
    /// </summary>
    public required decimal Beta { get; init; }

    /// <summary>
    /// Industry sector classification of the asset, if applicable. Null for assets
    /// without sector classification (e.g., cryptocurrency).
    /// </summary>
    public string? Sector { get; init; }

    /// <summary>
    /// Indicates whether the position is long (true) or short (false).
    /// </summary>
    public required bool IsLong { get; init; }
}
