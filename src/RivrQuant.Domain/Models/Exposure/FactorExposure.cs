namespace RivrQuant.Domain.Models.Exposure;

/// <summary>
/// Represents the portfolio's exposure to systematic risk factors such as value, momentum,
/// quality, and size. This is a Tier 3 stub; full factor model integration is planned for
/// a future release.
/// </summary>
/// <remarks>
/// TODO: Tier 3 - Implement full factor exposure decomposition once a factor model provider
/// (e.g., Barra, Axioma, or a custom PCA-based model) is integrated. This will include
/// per-factor beta exposures, factor return attribution, and factor risk contribution analysis.
/// </remarks>
public sealed record FactorExposure
{
    /// <summary>
    /// Description of this stub's purpose and planned functionality.
    /// </summary>
    public string Description { get; init; } = "Tier 3 stub";
}
