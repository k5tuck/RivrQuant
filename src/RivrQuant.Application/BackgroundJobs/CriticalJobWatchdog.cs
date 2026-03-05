namespace RivrQuant.Application.BackgroundJobs;

using Microsoft.Extensions.Logging;
using RivrQuant.Domain.Interfaces;

/// <summary>
/// Watchdog that ensures critical background jobs (DrawdownMonitor, VolatilityUpdate)
/// are running. If a critical job hasn't executed within its expected interval,
/// the watchdog triggers a safety halt.
/// </summary>
public sealed class CriticalJobWatchdog
{
    private readonly IDrawdownManager _drawdownManager;
    private readonly ILogger<CriticalJobWatchdog> _logger;

    private static DateTimeOffset _lastDrawdownCheck = DateTimeOffset.UtcNow;
    private static DateTimeOffset _lastVolCheck = DateTimeOffset.UtcNow;

    // If drawdown monitor hasn't run in 2 minutes (8x the 15-second interval), something is wrong
    private static readonly TimeSpan DrawdownTimeout = TimeSpan.FromMinutes(2);
    // If vol update hasn't run in 15 minutes (3x the 5-minute interval), something is wrong
    private static readonly TimeSpan VolTimeout = TimeSpan.FromMinutes(15);

    public CriticalJobWatchdog(
        IDrawdownManager drawdownManager,
        ILogger<CriticalJobWatchdog> logger)
    {
        _drawdownManager = drawdownManager;
        _logger = logger;
    }

    /// <summary>Records that the drawdown monitor has executed.</summary>
    public static void RecordDrawdownCheck() => _lastDrawdownCheck = DateTimeOffset.UtcNow;

    /// <summary>Records that the volatility update has executed.</summary>
    public static void RecordVolCheck() => _lastVolCheck = DateTimeOffset.UtcNow;

    /// <summary>Checks that critical jobs are running on schedule.</summary>
    public async Task ExecuteAsync()
    {
        var now = DateTimeOffset.UtcNow;

        if (now - _lastDrawdownCheck > DrawdownTimeout)
        {
            _logger.LogCritical(
                "WATCHDOG: DrawdownMonitorJob has not executed in {Minutes:F1} minutes. Triggering safety evaluation.",
                (now - _lastDrawdownCheck).TotalMinutes);

            try
            {
                await _drawdownManager.EvaluateAndActAsync(CancellationToken.None);
                _lastDrawdownCheck = now;
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "WATCHDOG: Failed to run drawdown evaluation. Trading safety may be compromised.");
            }
        }

        if (now - _lastVolCheck > VolTimeout)
        {
            _logger.LogWarning(
                "WATCHDOG: VolatilityUpdateJob has not executed in {Minutes:F1} minutes. Vol targeting may be stale.",
                (now - _lastVolCheck).TotalMinutes);
        }
    }
}
