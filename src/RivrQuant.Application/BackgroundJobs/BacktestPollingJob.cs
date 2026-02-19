namespace RivrQuant.Application.BackgroundJobs;

using Microsoft.Extensions.Logging;
using RivrQuant.Application.Services;
using RivrQuant.Infrastructure.QuantConnect;

/// <summary>Hangfire recurring job that polls QuantConnect for new backtests and triggers analysis.</summary>
public sealed class BacktestPollingJob
{
    private readonly QcBacktestPoller _poller;
    private readonly BacktestService _backtestService;
    private readonly ILogger<BacktestPollingJob> _logger;

    /// <summary>Initializes a new instance of <see cref="BacktestPollingJob"/>.</summary>
    public BacktestPollingJob(QcBacktestPoller poller, BacktestService backtestService, ILogger<BacktestPollingJob> logger)
    {
        _poller = poller;
        _backtestService = backtestService;
        _logger = logger;
    }

    /// <summary>Executes a single poll cycle.</summary>
    public async Task ExecuteAsync()
    {
        _logger.LogInformation("Backtest polling job started");
        try
        {
            var newBacktests = await _poller.PollForNewBacktestsAsync(CancellationToken.None);
            foreach (var bt in newBacktests)
            {
                try
                {
                    await _backtestService.AnalyzeBacktestAsync(bt.Id, CancellationToken.None);
                    _logger.LogInformation("Analysis complete for backtest {BacktestId}", bt.Id);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogError(ex, "Failed to analyze backtest {BacktestId}", bt.Id);
                }
            }
            _logger.LogInformation("Backtest polling job completed. Processed {Count} new backtests.", newBacktests.Count);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Backtest polling job failed");
        }
    }
}
