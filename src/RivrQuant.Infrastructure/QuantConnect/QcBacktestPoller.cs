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

        // Fetch project names once so every imported backtest is labelled with its algorithm name.
        Dictionary<string, string> projectNames;
        try
        {
            var projects = await _apiClient.GetProjectsAsync(ct);
            projectNames = projects.ToDictionary(p => p.Id, p => p.Name);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex, "Could not fetch project names — backtests will use project ID as name.");
            projectNames = new Dictionary<string, string>();
        }

        foreach (var projectId in _config.ProjectIds)
        {
            var projectName = projectNames.TryGetValue(projectId, out var name) ? name : projectId;
            try
            {
                var backtests = await _apiClient.GetBacktestsForProjectAsync(projectId, ct);
                _logger.LogDebug("Found {BacktestCount} backtests in project {ProjectId} ({ProjectName})", backtests.Count, projectId, projectName);

                foreach (var summary in backtests)
                {
                    var exists = await _dbContext.BacktestResults
                        .AnyAsync(b => b.ExternalBacktestId == summary.ExternalBacktestId, ct);

                    if (exists)
                    {
                        // Backfill ProjectName on existing records that were imported before this field existed.
                        var existing = await _dbContext.BacktestResults
                            .FirstOrDefaultAsync(b => b.ExternalBacktestId == summary.ExternalBacktestId && b.ProjectName == null, ct);
                        if (existing is not null)
                        {
                            existing.ProjectName = projectName;
                            await _dbContext.SaveChangesAsync(ct);
                        }
                        continue;
                    }

                    _logger.LogInformation("New backtest detected: {BacktestId} in project {ProjectId} ({ProjectName})", summary.ExternalBacktestId, projectId, projectName);

                    try
                    {
                        var detail = await _apiClient.GetBacktestDetailAsync(projectId, summary.ExternalBacktestId, ct);
                        detail.ProjectName = projectName;
                        _dbContext.BacktestResults.Add(detail);
                        await _dbContext.SaveChangesAsync(ct);
                        newBacktests.Add(detail);
                        _logger.LogInformation(
                            "Persisted new backtest {BacktestId} ({ProjectName} / {StrategyName}) with {TradeCount} trades",
                            detail.ExternalBacktestId, projectName, detail.StrategyName, detail.Trades.Count);
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
