using RivrQuant.Domain.Enums;
using RivrQuant.Domain.Models.Risk;

namespace RivrQuant.Domain.Interfaces;

/// <summary>
/// Provides position size calculation using a specific sizing methodology.
/// Implementations encapsulate the logic for a single <see cref="PositionSizingMethod"/>
/// and may be composed within a composite sizer that selects the most appropriate result.
/// </summary>
public interface IPositionSizer
{
    /// <summary>
    /// Gets the position sizing method implemented by this sizer.
    /// </summary>
    PositionSizingMethod Method { get; }

    /// <summary>
    /// Calculates the recommended position size for a proposed trade based on the
    /// current portfolio state, risk parameters, signal confidence, and cost estimates.
    /// </summary>
    /// <param name="request">
    /// The <see cref="PositionSizeRequest"/> containing all inputs needed for the sizing calculation.
    /// </param>
    /// <param name="ct">A cancellation token to cancel the asynchronous operation.</param>
    /// <returns>
    /// A <see cref="PositionSizeRecommendation"/> containing the recommended quantity,
    /// notional value, and details of any adjustments applied.
    /// </returns>
    Task<PositionSizeRecommendation> CalculateSizeAsync(
        PositionSizeRequest request,
        CancellationToken ct);
}
