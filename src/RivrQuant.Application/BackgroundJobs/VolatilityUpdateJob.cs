namespace RivrQuant.Application.BackgroundJobs;

using Microsoft.Extensions.Logging;
using RivrQuant.Domain.Interfaces;

/// <summary>Hangfire recurring job that recalculates realized volatility every 5 minutes.</summary>
public sealed class VolatilityUpdateJob
{
    private readonly IVolatilityTargetEngine _volEngine;
    private readonly ILogger<VolatilityUpdateJob> _logger;

    public VolatilityUpdateJob(
        IVolatilityTargetEngine volEngine,
        ILogger<VolatilityUpdateJob> logger)
    {
        _volEngine = volEngine;
        _logger = logger;
    }

    /// <summary>Recalculates realized volatility and updates the volatility target state.</summary>
    public async Task ExecuteAsync()
    {
        _logger.LogDebug("Volatility update job started");

        try
        {
            var target = await _volEngine.RecalculateAsync(CancellationToken.None);

            _logger.LogInformation(
                "Volatility recalculated: realized={Realized:P2}, target={Target:P2}, multiplier={Mult:F2}, regime={Regime}",
                target.RealizedAnnualizedVol, target.TargetAnnualizedVol,
                target.VolMultiplier, target.VolRegime);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Volatility update job failed");
        }
    }
}
