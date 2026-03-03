using RivrQuant.Domain.Models.Risk;

namespace RivrQuant.Domain.Interfaces;

/// <summary>
/// Manages portfolio volatility targeting by comparing realized annualized volatility
/// against the configured target and computing an adjustment multiplier for position sizing.
/// Implementations track volatility regime changes and persist snapshots for trend analysis.
/// </summary>
public interface IVolatilityTargetEngine
{
    /// <summary>
    /// Retrieves the most recent volatility target snapshot, including the current
    /// realized volatility, target level, multiplier, and regime classification.
    /// </summary>
    /// <param name="ct">A cancellation token to cancel the asynchronous operation.</param>
    /// <returns>The most recent <see cref="VolatilityTarget"/> snapshot.</returns>
    Task<VolatilityTarget> GetCurrentTargetAsync(CancellationToken ct);

    /// <summary>
    /// Recalculates the volatility target based on the latest return data, persists
    /// a new snapshot, and returns the updated result.
    /// </summary>
    /// <param name="ct">A cancellation token to cancel the asynchronous operation.</param>
    /// <returns>A newly calculated <see cref="VolatilityTarget"/> snapshot.</returns>
    Task<VolatilityTarget> RecalculateAsync(CancellationToken ct);
}
