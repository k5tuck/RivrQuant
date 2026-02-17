// Copyright (c) RivrQuant. All rights reserved.
// Licensed under the MIT License.

using RivrQuant.Domain.Models.Trading;

namespace RivrQuant.Domain.Interfaces;

/// <summary>
/// Tracks aggregate portfolio state across one or more broker accounts and
/// provides point-in-time performance snapshots for historical analysis.
/// </summary>
public interface IPortfolioTracker
{
    /// <summary>
    /// Retrieves the current aggregate portfolio by combining positions and balances
    /// from all connected broker accounts.
    /// </summary>
    /// <param name="ct">A cancellation token to cancel the asynchronous operation.</param>
    /// <returns>
    /// A <see cref="Portfolio"/> representing the consolidated view of all accounts,
    /// including total equity, cash, and merged positions.
    /// </returns>
    Task<Portfolio> GetAggregatePortfolioAsync(CancellationToken ct);

    /// <summary>
    /// Captures and persists a point-in-time performance snapshot of the aggregate
    /// portfolio, recording equity, cash balance, daily return, and drawdown metrics.
    /// </summary>
    /// <param name="ct">A cancellation token to cancel the asynchronous operation.</param>
    /// <returns>The newly created <see cref="PerformanceSnapshot"/>.</returns>
    Task<PerformanceSnapshot> TakeSnapshotAsync(CancellationToken ct);

    /// <summary>
    /// Retrieves previously recorded performance snapshots within the specified date range,
    /// enabling time-series analysis of portfolio performance.
    /// </summary>
    /// <param name="from">The inclusive start of the date range (UTC).</param>
    /// <param name="to">The inclusive end of the date range (UTC).</param>
    /// <param name="ct">A cancellation token to cancel the asynchronous operation.</param>
    /// <returns>
    /// A read-only list of <see cref="PerformanceSnapshot"/> entries ordered chronologically.
    /// </returns>
    Task<IReadOnlyList<PerformanceSnapshot>> GetSnapshotHistoryAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken ct);
}
