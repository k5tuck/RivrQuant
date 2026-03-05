using RivrQuant.Domain.Models.Risk;

namespace RivrQuant.Domain.Interfaces;

/// <summary>
/// Manages portfolio drawdown monitoring, deleverage level transitions, and
/// position sizing multiplier calculations. Implementations track the high-water mark,
/// evaluate drawdown thresholds, and trigger deleveraging actions when risk limits are breached.
/// </summary>
public interface IDrawdownManager
{
    /// <summary>
    /// Retrieves the current drawdown state, including peak equity, drawdown metrics,
    /// the active deleverage level, and any paused strategies.
    /// </summary>
    /// <param name="ct">A cancellation token to cancel the asynchronous operation.</param>
    /// <returns>The current <see cref="DrawdownState"/>.</returns>
    Task<DrawdownState> GetCurrentStateAsync(CancellationToken ct);

    /// <summary>
    /// Retrieves the position sizing multiplier corresponding to the current deleverage level.
    /// Returns 1.0 at Normal level, decreasing toward 0.0 at Emergency level.
    /// </summary>
    /// <param name="ct">A cancellation token to cancel the asynchronous operation.</param>
    /// <returns>A decimal multiplier between 0.0 and 1.0.</returns>
    Task<decimal> GetDrawdownMultiplierAsync(CancellationToken ct);

    /// <summary>
    /// Evaluates the current portfolio equity against drawdown thresholds and, if necessary,
    /// transitions the deleverage level and executes deleveraging actions (pausing strategies,
    /// reducing positions, or liquidating entirely).
    /// </summary>
    /// <param name="ct">A cancellation token to cancel the asynchronous operation.</param>
    Task EvaluateAndActAsync(CancellationToken ct);
}
