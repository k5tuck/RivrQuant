namespace RivrQuant.Application.BackgroundJobs;

using Microsoft.Extensions.Logging;
using RivrQuant.Application.Services;

/// <summary>Hangfire recurring job that recalculates cross-asset correlations every hour.</summary>
public sealed class CorrelationUpdateJob
{
    private readonly ExposureService _exposureService;
    private readonly ILogger<CorrelationUpdateJob> _logger;

    private const int DefaultLookbackDays = 60;

    public CorrelationUpdateJob(
        ExposureService exposureService,
        ILogger<CorrelationUpdateJob> logger)
    {
        _exposureService = exposureService;
        _logger = logger;
    }

    /// <summary>Recalculates the cross-asset correlation matrix.</summary>
    public async Task ExecuteAsync()
    {
        _logger.LogDebug("Correlation update job started");

        try
        {
            var snapshot = await _exposureService.GetCorrelationMatrixAsync(DefaultLookbackDays, CancellationToken.None);

            _logger.LogInformation(
                "Correlation matrix updated: {LookbackDays}-day lookback, snapshot at {SnapshotAt}",
                snapshot.LookbackDays, snapshot.SnapshotAt);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Correlation update job failed");
        }
    }
}
