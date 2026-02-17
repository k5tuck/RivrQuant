namespace RivrQuant.Infrastructure.QuantConnect;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RivrQuant.Domain.Models.Backtests;
using RivrQuant.Infrastructure.Persistence;

/// <summary>Background polling service that detects new backtest results on QuantConnect.</summary>
public sealed class QcBacktestPoller
{
    private readonly QcApiClient _apiClient;
    private readonly RivrQuantDbContext _dbContext;
    private readonly QcConfiguration _config;
    private readonly ILogger<QcBacktestPoller> _logger;

    /// <summary>Initializes a new instance of <see cref="QcBacktestPoller"/>.</summary>
    public QcBacktestPoller(
        QcApiClient apiClient,
        RivrQuantDbContext dbContext,
        IOptions<QcConfiguration> config,
        ILogger<QcBacktestPoller> logger)
    {
        _apiClient = apiClient;
        _dbContext = dbContext;
        _config = config.Value;
        _logger = logger;
    }

    /// <summary>
    /// Polls QuantConnect for new backtest results across all configured projects.
    /// Returns the list of newly detected and persisted backtest results.
    /// </summary>
    public async Task<IReadOnlyList<BacktestResult>> PollForNewBacktestsAsync(CancellationToken ct)
    {
        var newBacktests = new List<BacktestResult>();
        _logger.LogInformation("Starting backtest poll cycle for {ProjectCount} projects", _config.ProjectIds.Count);

        foreach (var projectId in _config.ProjectIds)
        {
            try
            {
                var backtests = await _apiClient.GetBacktestsForProjectAsync(projectId, ct);
                _logger.LogDebug("Found {BacktestCount} backtests in project {ProjectId}", backtests.Count, projectId);

                foreach (var summary in backtests)
                {
                    var exists = await _dbContext.BacktestResults
                        .AnyAsync(b => b.ExternalBacktestId == summary.ExternalBacktestId, ct);

                    if (exists)
                    {
                        continue;
                    }

                    _logger.LogInformation("New backtest detected: {BacktestId} in project {ProjectId}", summary.ExternalBacktestId, projectId);

                    try
                    {
                        var detail = await _apiClient.GetBacktestDetailAsync(projectId, summary.ExternalBacktestId, ct);
                        _dbContext.BacktestResults.Add(detail);
                        await _dbContext.SaveChangesAsync(ct);
                        newBacktests.Add(detail);
                        _logger.LogInformation(
                            "Persisted new backtest {BacktestId} ({StrategyName}) with {TradeCount} trades and {DailyReturnCount} daily returns",
                            detail.ExternalBacktestId, detail.StrategyName, detail.Trades.Count, detail.DailyReturns.Count);
                    }
                    catch (Exception ex) when (ex is not OperationCanceledException)
                    {
                        _logger.LogError(ex, "Failed to fetch/persist backtest detail {BacktestId} in project {ProjectId}. Skipping.", summary.ExternalBacktestId, projectId);
                    }
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning(ex, "Failed to poll project {ProjectId}. Skipping to next project.", projectId);
            }
        }

        _logger.LogInformation("Poll cycle complete. Detected {NewBacktestCount} new backtests.", newBacktests.Count);
        return newBacktests;
    }
}
