namespace RivrQuant.Application.BackgroundJobs;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RivrQuant.Domain.Interfaces;
using RivrQuant.Infrastructure.Persistence;

/// <summary>Hangfire recurring job that compares live performance against backtest expectations.</summary>
public sealed class LivePerformanceComparisonJob
{
    private readonly RivrQuantDbContext _db;
    private readonly IPortfolioTracker _portfolioTracker;
    private readonly IStatisticsEngine _statistics;
    private readonly ILogger<LivePerformanceComparisonJob> _logger;

    public LivePerformanceComparisonJob(
        RivrQuantDbContext db,
        IPortfolioTracker portfolioTracker,
        IStatisticsEngine statistics,
        ILogger<LivePerformanceComparisonJob> logger)
    {
        _db = db;
        _portfolioTracker = portfolioTracker;
        _statistics = statistics;
        _logger = logger;
    }

    /// <summary>Compares live performance against backtest predictions for deployed strategies.</summary>
    public async Task ExecuteAsync()
    {
        _logger.LogDebug("Live performance comparison job started");

        try
        {
            var activeStrategies = await _db.Strategies
                .Where(s => s.IsActive)
                .ToListAsync(CancellationToken.None);

            if (activeStrategies.Count == 0)
            {
                _logger.LogDebug("No active strategies to compare");
                return;
            }

            var snapshots = await _db.PerformanceSnapshots
                .OrderByDescending(s => s.Timestamp)
                .Take(30)
                .ToListAsync(CancellationToken.None);

            if (snapshots.Count < 5)
            {
                _logger.LogDebug("Insufficient snapshots ({Count}) for meaningful comparison", snapshots.Count);
                return;
            }

            var liveReturns = snapshots
                .OrderBy(s => s.Timestamp)
                .Select(s => (double)s.DailyReturnPercent)
                .ToList();

            var liveSharpe = _statistics.CalculateSharpeRatio(liveReturns, 0.05);

            foreach (var strategy in activeStrategies)
            {
                var backtestResult = await _db.BacktestResults
                    .Include(b => b.Metrics)
                    .Where(b => b.StrategyName == strategy.Name && b.Metrics != null)
                    .OrderByDescending(b => b.CreatedAt)
                    .FirstOrDefaultAsync(CancellationToken.None);

                if (backtestResult?.Metrics is null)
                {
                    _logger.LogDebug("No backtest metrics found for strategy {Strategy}", strategy.Name);
                    continue;
                }

                var backtestSharpe = backtestResult.Metrics.SharpeRatio;
                var sharpeRatio = backtestSharpe != 0 ? liveSharpe / backtestSharpe : 0;

                if (sharpeRatio < 0)
                {
                    _logger.LogWarning(
                        "Strategy {Strategy}: FAILURE — Live Sharpe ({LiveSharpe:F2}) is negative while backtest Sharpe ({BacktestSharpe:F2}) was positive",
                        strategy.Name, liveSharpe, backtestSharpe);
                }
                else if (sharpeRatio < 0.5)
                {
                    _logger.LogWarning(
                        "Strategy {Strategy}: SIGNIFICANT DECAY — Live Sharpe ({LiveSharpe:F2}) is less than half of backtest Sharpe ({BacktestSharpe:F2}). Ratio: {Ratio:F2}",
                        strategy.Name, liveSharpe, backtestSharpe, sharpeRatio);
                }
                else
                {
                    _logger.LogInformation(
                        "Strategy {Strategy}: OK — Live Sharpe ({LiveSharpe:F2}) vs Backtest Sharpe ({BacktestSharpe:F2}). Ratio: {Ratio:F2}",
                        strategy.Name, liveSharpe, backtestSharpe, sharpeRatio);
                }
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Live performance comparison job failed");
        }
    }
}
