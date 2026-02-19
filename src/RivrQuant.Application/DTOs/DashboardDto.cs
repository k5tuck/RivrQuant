using RivrQuant.Domain.Models.Trading;

namespace RivrQuant.Application.DTOs;

/// <summary>
/// Aggregated dashboard data combining portfolio state, performance metrics,
/// and recent activity summaries.
/// </summary>
public sealed record DashboardDto
{
    /// <summary>Aggregate portfolio across all brokers.</summary>
    public Portfolio? Portfolio { get; init; }

    /// <summary>Open positions across all brokers.</summary>
    public IReadOnlyList<Position> Positions { get; init; } = Array.Empty<Position>();

    /// <summary>Recent performance snapshots.</summary>
    public IReadOnlyList<PerformanceSnapshot> RecentSnapshots { get; init; } = Array.Empty<PerformanceSnapshot>();

    /// <summary>Total number of backtests.</summary>
    public int TotalBacktests { get; init; }

    /// <summary>Number of analyzed backtests.</summary>
    public int AnalyzedBacktests { get; init; }

    /// <summary>Number of active strategies.</summary>
    public int ActiveStrategies { get; init; }

    /// <summary>Number of unacknowledged alerts.</summary>
    public int UnacknowledgedAlerts { get; init; }

    /// <summary>Timestamp of the dashboard data.</summary>
    public DateTimeOffset AsOf { get; init; } = DateTimeOffset.UtcNow;
}
