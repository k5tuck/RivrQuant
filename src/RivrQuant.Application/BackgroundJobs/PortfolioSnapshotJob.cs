namespace RivrQuant.Application.BackgroundJobs;

using Microsoft.Extensions.Logging;
using RivrQuant.Application.Services;

/// <summary>Hangfire recurring job that takes periodic portfolio snapshots.</summary>
public sealed class PortfolioSnapshotJob
{
    private readonly DashboardService _dashboardService;
    private readonly ILogger<PortfolioSnapshotJob> _logger;

    /// <summary>Initializes a new instance of <see cref="PortfolioSnapshotJob"/>.</summary>
    public PortfolioSnapshotJob(DashboardService dashboardService, ILogger<PortfolioSnapshotJob> logger)
    {
        _dashboardService = dashboardService;
        _logger = logger;
    }

    /// <summary>Takes a portfolio snapshot and persists it.</summary>
    public async Task ExecuteAsync()
    {
        _logger.LogDebug("Portfolio snapshot job started");
        try
        {
            await _dashboardService.TakeSnapshotAsync(CancellationToken.None);
            _logger.LogDebug("Portfolio snapshot taken");
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Portfolio snapshot job failed");
        }
    }
}
