using RivrQuant.Domain.Models.Exposure;

namespace RivrQuant.Domain.Interfaces;

/// <summary>
/// Tracks and reports portfolio exposure across multiple dimensions including net/gross exposure,
/// beta, asset class breakdown, sector concentration, and cross-asset correlations.
/// Implementations aggregate position data and persist periodic snapshots for trend analysis.
/// </summary>
public interface IExposureTracker
{
    /// <summary>
    /// Retrieves the most recently computed portfolio exposure snapshot.
    /// </summary>
    /// <param name="ct">A cancellation token to cancel the asynchronous operation.</param>
    /// <returns>The current <see cref="PortfolioExposure"/> snapshot.</returns>
    Task<PortfolioExposure> GetCurrentExposureAsync(CancellationToken ct);

    /// <summary>
    /// Computes a pairwise correlation matrix for all portfolio symbols using the
    /// specified lookback period of historical returns.
    /// </summary>
    /// <param name="lookbackDays">Number of calendar days of historical data to use for correlation calculation.</param>
    /// <param name="ct">A cancellation token to cancel the asynchronous operation.</param>
    /// <returns>A <see cref="CorrelationSnapshot"/> containing the symbols and correlation matrix.</returns>
    Task<CorrelationSnapshot> GetCorrelationMatrixAsync(int lookbackDays, CancellationToken ct);

    /// <summary>
    /// Computes a new portfolio exposure snapshot based on current positions and market data,
    /// persists it to the database, and returns the result.
    /// </summary>
    /// <param name="ct">A cancellation token to cancel the asynchronous operation.</param>
    /// <returns>A newly computed and persisted <see cref="PortfolioExposure"/> snapshot.</returns>
    Task<PortfolioExposure> SnapshotAsync(CancellationToken ct);
}
