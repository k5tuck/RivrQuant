namespace RivrQuant.Application.BackgroundJobs;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RivrQuant.Domain.Interfaces;
using RivrQuant.Infrastructure.Persistence;

/// <summary>
/// Hangfire recurring job that checks out-of-sample decay daily.
/// Compares last 30 days of live performance against the backtest that justified deployment.
/// </summary>
public sealed class DecayTrackingJob
{
    private readonly RivrQuantDbContext _db;
    private readonly IStatisticsEngine _statistics;
    private readonly ILogger<DecayTrackingJob> _logger;

    public DecayTrackingJob(
        RivrQuantDbContext db,
        IStatisticsEngine statistics,
        ILogger<DecayTrackingJob> logger)
    {
        _db = db;
        _statistics = statistics;
        _logger = logger;
    }

    /// <summary>Evaluates out-of-sample decay for each deployed strategy.</summary>
    public async Task ExecuteAsync()
    {
        _logger.LogDebug("Decay tracking job started");

        try
        {
            var activeStrategies = await _db.Strategies
                .Where(s => s.IsActive)
                .ToListAsync(CancellationToken.None);

            if (activeStrategies.Count == 0)
            {
                _logger.LogDebug("No active strategies for decay tracking");
                return;
            }

            var snapshots = await _db.PerformanceSnapshots
                .OrderByDescending(s => s.Timestamp)
                .Take(30)
                .ToListAsync(CancellationToken.None);

            if (snapshots.Count < 10)
            {
                _logger.LogDebug("Insufficient snapshots ({Count}) for decay analysis", snapshots.Count);
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

                if (backtestResult?.Metrics is null) continue;

                var backtestSharpe = backtestResult.Metrics.SharpeRatio;
                var ratio = backtestSharpe != 0 ? liveSharpe / backtestSharpe : 0;

                var severity = ratio switch
                {
                    < 0 => "failure",
                    < 0.5 => "significant",
                    < 0.75 => "moderate",
                    _ => "none"
                };

                if (severity == "failure")
                {
                    _logger.LogCritical(
                        "STRATEGY FAILURE: {Strategy} — Live Sharpe ({LiveSharpe:F2}) is negative while backtest ({BacktestSharpe:F2}) was positive. Auto-pause recommended.",
                        strategy.Name, liveSharpe, backtestSharpe);
                }
                else if (severity == "significant")
                {
                    _logger.LogWarning(
                        "Significant decay: {Strategy} — Live Sharpe ({LiveSharpe:F2}) < 50% of backtest ({BacktestSharpe:F2}). Consider pausing.",
                        strategy.Name, liveSharpe, backtestSharpe);
                }
                else if (severity == "moderate")
                {
                    _logger.LogInformation(
                        "Moderate decay: {Strategy} — Live Sharpe ({LiveSharpe:F2}) at {Ratio:P0} of backtest ({BacktestSharpe:F2})",
                        strategy.Name, liveSharpe, ratio, backtestSharpe);
                }
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Decay tracking job failed");
        }
    }
}
