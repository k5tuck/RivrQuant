namespace RivrQuant.Infrastructure.QuantConnect;

using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RivrQuant.Domain.Models.Backtests;
using RivrQuant.Infrastructure.Persistence;

/// <summary>Background polling service that detects new backtest results on QuantConnect.</summary>
public sealed class QcBacktestPoller
{
    private readonly QcApiClient _apiClient;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly QcConfiguration _config;
    private readonly ILogger<QcBacktestPoller> _logger;

    /// <summary>Initializes a new instance of <see cref="QcBacktestPoller"/>.</summary>
    public QcBacktestPoller(
        QcApiClient apiClient,
        IServiceScopeFactory scopeFactory,
        IOptions<QcConfiguration> config,
        ILogger<QcBacktestPoller> logger)
    {
        _apiClient = apiClient;
        _scopeFactory = scopeFactory;
        _config = config.Value;
        _logger = logger;
    }

    /// <summary>
    /// Polls QuantConnect for new backtest results across all configured projects in parallel.
    /// Each project runs in its own DI scope so DbContext instances are never shared across threads.
    /// Returns the list of newly detected and persisted backtest results.
    /// </summary>
    public async Task<IReadOnlyList<BacktestResult>> PollForNewBacktestsAsync(CancellationToken ct)
    {
        _logger.LogInformation("Starting backtest poll cycle for {ProjectCount} projects", _config.ProjectIds.Count);

        // Fetch project names once (single API call, shared across all project tasks).
        // QcApiClient wraps HttpClient which is thread-safe for concurrent calls.
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

        // EF Core DbContext is NOT thread-safe. Each parallel project task creates its own
        // DI scope so it gets a dedicated DbContext instance. QcApiClient/HttpClient is
        // thread-safe and is safely shared across all tasks.
        var allNewBacktests = new ConcurrentBag<BacktestResult>();

        var projectTasks = _config.ProjectIds.Select(async projectId =>
        {
            var projectName = projectNames.TryGetValue(projectId, out var name) ? name : projectId;
            _logger.LogInformation("Poll start — project {ProjectId} ({ProjectName})", projectId, projectName);

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<RivrQuantDbContext>();

                var backtests = await _apiClient.GetBacktestsForProjectAsync(projectId, ct);
                _logger.LogDebug("Found {BacktestCount} backtests in project {ProjectId} ({ProjectName})",
                    backtests.Count, projectId, projectName);

                foreach (var summary in backtests)
                {
                    var exists = await dbContext.BacktestResults
                        .AnyAsync(b => b.ExternalBacktestId == summary.ExternalBacktestId, ct);

                    if (exists)
                    {
                        // Backfill ProjectName on existing records imported before this field existed.
                        var existing = await dbContext.BacktestResults
                            .FirstOrDefaultAsync(
                                b => b.ExternalBacktestId == summary.ExternalBacktestId && b.ProjectName == null, ct);
                        if (existing is not null)
                        {
                            existing.ProjectName = projectName;
                            await dbContext.SaveChangesAsync(ct);
                        }
                        continue;
                    }

                    _logger.LogInformation(
                        "New backtest detected: {BacktestId} in project {ProjectId} ({ProjectName})",
                        summary.ExternalBacktestId, projectId, projectName);

                    try
                    {
                        var detail = await _apiClient.GetBacktestDetailAsync(projectId, summary.ExternalBacktestId, ct);
                        detail.ProjectName = projectName;
                        dbContext.BacktestResults.Add(detail);
                        await dbContext.SaveChangesAsync(ct);
                        allNewBacktests.Add(detail);
                        _logger.LogInformation(
                            "Persisted new backtest {BacktestId} ({ProjectName} / {StrategyName}) with {TradeCount} trades",
                            detail.ExternalBacktestId, projectName, detail.StrategyName, detail.Trades.Count);
                    }
                    catch (Exception ex) when (ex is not OperationCanceledException)
                    {
                        _logger.LogError(ex,
                            "Failed to fetch/persist backtest detail {BacktestId} in project {ProjectId}. Skipping.",
                            summary.ExternalBacktestId, projectId);
                    }
                }

                _logger.LogInformation("Poll complete — project {ProjectId} ({ProjectName})", projectId, projectName);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning(ex,
                    "Failed to poll project {ProjectId} ({ProjectName}). Skipping to next project.",
                    projectId, projectName);
            }
        });

        await Task.WhenAll(projectTasks);

        var result = allNewBacktests.ToList();
        _logger.LogInformation("Poll cycle complete. Detected {NewBacktestCount} new backtests.", result.Count);
        return result;
    }
}
