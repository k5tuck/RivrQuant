// Copyright (c) RivrQuant. All rights reserved.
// Licensed under the MIT License.

using RivrQuant.Domain.Models.Backtests;

namespace RivrQuant.Domain.Interfaces;

/// <summary>
/// Provides access to backtest results and project metadata from an external
/// algorithmic trading platform (e.g., QuantConnect LEAN).
/// </summary>
public interface IBacktestProvider
{
    /// <summary>
    /// Retrieves all backtest results associated with the specified project.
    /// </summary>
    /// <param name="projectId">The unique identifier of the project whose backtests are requested.</param>
    /// <param name="ct">A cancellation token to cancel the asynchronous operation.</param>
    /// <returns>
    /// A read-only list of <see cref="BacktestResult"/> summaries for the given project,
    /// ordered by creation date descending.
    /// </returns>
    Task<IReadOnlyList<BacktestResult>> GetBacktestsForProjectAsync(string projectId, CancellationToken ct);

    /// <summary>
    /// Retrieves the full detail of a single backtest, including equity curve data,
    /// trade log, and configuration parameters.
    /// </summary>
    /// <param name="projectId">The unique identifier of the project that owns the backtest.</param>
    /// <param name="backtestId">The unique identifier of the backtest to retrieve.</param>
    /// <param name="ct">A cancellation token to cancel the asynchronous operation.</param>
    /// <returns>The complete <see cref="BacktestResult"/> for the specified backtest.</returns>
    Task<BacktestResult> GetBacktestDetailAsync(string projectId, string backtestId, CancellationToken ct);

    /// <summary>
    /// Retrieves all project identifiers accessible to the authenticated user.
    /// </summary>
    /// <param name="ct">A cancellation token to cancel the asynchronous operation.</param>
    /// <returns>A read-only list of project identifier strings.</returns>
    Task<IReadOnlyList<string>> GetProjectIdsAsync(CancellationToken ct);
}
