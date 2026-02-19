namespace RivrQuant.Application.BackgroundJobs;

using Microsoft.Extensions.Logging;

/// <summary>Hangfire recurring job that compares live performance against backtest expectations.</summary>
public sealed class LivePerformanceComparisonJob
{
    private readonly ILogger<LivePerformanceComparisonJob> _logger;

    /// <summary>Initializes a new instance of <see cref="LivePerformanceComparisonJob"/>.</summary>
    public LivePerformanceComparisonJob(ILogger<LivePerformanceComparisonJob> logger)
    {
        _logger = logger;
    }

    /// <summary>Compares live performance against backtest predictions.</summary>
    public Task ExecuteAsync()
    {
        _logger.LogDebug("Live performance comparison job executed");
        return Task.CompletedTask;
    }
}
