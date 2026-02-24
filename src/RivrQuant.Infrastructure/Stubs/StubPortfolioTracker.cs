namespace RivrQuant.Infrastructure.Stubs;

using RivrQuant.Domain.Interfaces;
using RivrQuant.Domain.Models.Trading;

/// <summary>Stub implementation of IPortfolioTracker. Returns empty data until a real implementation is wired up.</summary>
public sealed class StubPortfolioTracker : IPortfolioTracker
{
    public Task<Portfolio> GetAggregatePortfolioAsync(CancellationToken ct)
        => Task.FromResult(new Portfolio());

    public Task<PerformanceSnapshot> TakeSnapshotAsync(CancellationToken ct)
        => Task.FromResult(new PerformanceSnapshot());

    public Task<IReadOnlyList<PerformanceSnapshot>> GetSnapshotHistoryAsync(
        DateTimeOffset from, DateTimeOffset to, CancellationToken ct)
        => Task.FromResult<IReadOnlyList<PerformanceSnapshot>>(Array.Empty<PerformanceSnapshot>());
}
