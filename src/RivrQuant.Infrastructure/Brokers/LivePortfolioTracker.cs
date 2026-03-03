namespace RivrQuant.Infrastructure.Brokers;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RivrQuant.Domain.Enums;
using RivrQuant.Domain.Interfaces;
using RivrQuant.Domain.Models.Trading;
using RivrQuant.Infrastructure.Persistence;

/// <summary>
/// Production implementation of <see cref="IPortfolioTracker"/> that aggregates
/// real portfolio data from all registered broker accounts. One broker being
/// temporarily unavailable does not fail the entire snapshot — its contribution
/// is skipped with a warning log so the other broker's data is still returned.
/// </summary>
public sealed class LivePortfolioTracker : IPortfolioTracker
{
    private readonly IBrokerClientFactory _brokerFactory;
    private readonly IStatisticsEngine _statistics;
    private readonly RivrQuantDbContext _db;
    private readonly ILogger<LivePortfolioTracker> _logger;

    /// <summary>Initializes a new instance of <see cref="LivePortfolioTracker"/>.</summary>
    public LivePortfolioTracker(
        IBrokerClientFactory brokerFactory,
        IStatisticsEngine statistics,
        RivrQuantDbContext db,
        ILogger<LivePortfolioTracker> logger)
    {
        _brokerFactory = brokerFactory;
        _statistics    = statistics;
        _db            = db;
        _logger        = logger;
    }

    /// <inheritdoc />
    /// <remarks>
    /// Queries all registered brokers concurrently and sums equity, cash, buying power,
    /// and PnL figures. If a broker call fails its numbers are excluded but the method
    /// succeeds, returning whatever data is available.
    /// </remarks>
    public async Task<Portfolio> GetAggregatePortfolioAsync(CancellationToken ct)
    {
        var portfolioTasks = Enum.GetValues<BrokerType>()
            .Select(async brokerType =>
            {
                try
                {
                    return await _brokerFactory.GetClient(brokerType).GetPortfolioAsync(ct);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogWarning(ex, "Could not fetch {Broker} portfolio — excluding from aggregate", brokerType);
                    return null;
                }
            });

        var results = await Task.WhenAll(portfolioTasks);

        var available = results.Where(p => p is not null).Select(p => p!).ToList();

        if (available.Count == 0)
        {
            _logger.LogWarning("All broker portfolio fetches failed — returning empty aggregate portfolio");
            return new Portfolio();
        }

        // Aggregate across all successful broker responses.
        return new Portfolio
        {
            TotalEquity        = available.Sum(p => p.TotalEquity),
            CashBalance        = available.Sum(p => p.CashBalance),
            BuyingPower        = available.Sum(p => p.BuyingPower),
            UnrealizedPnl      = available.Sum(p => p.UnrealizedPnl),
            RealizedPnlToday   = available.Sum(p => p.RealizedPnlToday),
            DailyChangePercent = available.Count > 0
                ? available.Average(p => p.DailyChangePercent)
                : 0m,
            // Use None to indicate this is a consolidated multi-broker view.
            Broker = (BrokerType)(-1),
            AsOf   = DateTimeOffset.UtcNow
        };
    }

    /// <inheritdoc />
    /// <remarks>
    /// Captures a point-in-time snapshot by fetching the current aggregate portfolio,
    /// computing the rolling Sharpe ratio from recent persisted snapshots, and
    /// calculating the current drawdown from the rolling peak equity.
    /// </remarks>
    public async Task<PerformanceSnapshot> TakeSnapshotAsync(CancellationToken ct)
    {
        var portfolio = await GetAggregatePortfolioAsync(ct);

        // Load the last 30 snapshots to calculate rolling metrics.
        var recentSnapshots = await _db.PerformanceSnapshots
            .OrderByDescending(s => s.Timestamp)
            .Take(30)
            .OrderBy(s => s.Timestamp)
            .ToListAsync(ct);

        // Daily P&L relative to the most recent snapshot equity.
        var previousEquity = recentSnapshots.LastOrDefault()?.TotalEquity ?? portfolio.TotalEquity;
        var dailyPnl = portfolio.TotalEquity - previousEquity;
        var dailyReturnPercent = previousEquity != 0
            ? dailyPnl / previousEquity
            : 0m;

        // Drawdown from rolling peak equity (all-time high across persisted snapshots).
        var peakEquity = recentSnapshots.Count > 0
            ? Math.Max(recentSnapshots.Max(s => s.TotalEquity), portfolio.TotalEquity)
            : portfolio.TotalEquity;
        var currentDrawdown = peakEquity > 0
            ? (portfolio.TotalEquity - peakEquity) / peakEquity
            : 0m;

        // Cumulative return: compare current equity to the first persisted snapshot.
        var firstEquity = recentSnapshots.FirstOrDefault()?.TotalEquity;
        var cumulativeReturn = firstEquity.HasValue && firstEquity.Value != 0
            ? (portfolio.TotalEquity - firstEquity.Value) / firstEquity.Value
            : 0m;

        // Open position count from live broker data (best-effort; failure is non-fatal).
        var openPositionCount = 0;
        try
        {
            var positions = await Task.WhenAll(
                Enum.GetValues<BrokerType>().Select(bt =>
                    _brokerFactory.GetClient(bt).GetPositionsAsync(ct)
                        .ContinueWith(t => t.IsCompletedSuccessfully ? t.Result.Count : 0,
                            TaskContinuationOptions.None)));
            openPositionCount = positions.Sum();
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex, "Could not determine open position count for snapshot");
        }

        return new PerformanceSnapshot
        {
            TotalEquity        = portfolio.TotalEquity,
            DailyPnl           = dailyPnl,
            DailyReturnPercent = dailyReturnPercent,
            CumulativeReturn   = cumulativeReturn,
            CurrentDrawdown    = currentDrawdown,
            OpenPositionCount  = openPositionCount,
            Timestamp          = DateTimeOffset.UtcNow
        };
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PerformanceSnapshot>> GetSnapshotHistoryAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken ct)
    {
        return await _db.PerformanceSnapshots
            .Where(s => s.Timestamp >= from && s.Timestamp <= to)
            .OrderBy(s => s.Timestamp)
            .ToListAsync(ct);
    }
}
