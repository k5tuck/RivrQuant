namespace RivrQuant.Application.BackgroundJobs;

using Microsoft.Extensions.Logging;
using RivrQuant.Application.Services;

/// <summary>Hangfire recurring job that snapshots portfolio exposure every minute.</summary>
public sealed class ExposureSnapshotJob
{
    private readonly ExposureService _exposureService;
    private readonly ILogger<ExposureSnapshotJob> _logger;

    public ExposureSnapshotJob(
        ExposureService exposureService,
        ILogger<ExposureSnapshotJob> logger)
    {
        _exposureService = exposureService;
        _logger = logger;
    }

    /// <summary>Takes an exposure snapshot and persists it for the dashboard timeline.</summary>
    public async Task ExecuteAsync()
    {
        _logger.LogDebug("Exposure snapshot job started");

        try
        {
            var exposure = await _exposureService.SnapshotAsync(CancellationToken.None);

            _logger.LogDebug(
                "Exposure snapshot: net={Net:F1}%, gross={Gross:F1}%, beta={Beta:F2}",
                exposure.NetExposurePercent, exposure.GrossExposurePercent, exposure.PortfolioBeta);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Exposure snapshot job failed");
        }
    }
}
