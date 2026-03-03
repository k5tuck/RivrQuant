namespace RivrQuant.Application.BackgroundJobs;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RivrQuant.Application.Services;
using RivrQuant.Infrastructure.QuantConnect;

/// <summary>Hangfire recurring job that polls QuantConnect for new backtests and triggers parallel analysis.</summary>
public sealed class BacktestPollingJob
{
    private readonly QcBacktestPoller _poller;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BacktestPollingJob> _logger;

    /// <summary>Initializes a new instance of <see cref="BacktestPollingJob"/>.</summary>
    public BacktestPollingJob(
        QcBacktestPoller poller,
        IServiceScopeFactory scopeFactory,
        ILogger<BacktestPollingJob> logger)
    {
        _poller = poller;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    /// <summary>
    /// Executes a single poll cycle. QC HTTP polling runs fully parallel across projects.
    /// Claude analysis is capped at 3 concurrent calls via a SemaphoreSlim to avoid
    /// overwhelming the Anthropic API with simultaneous requests.
    /// </summary>
    public async Task ExecuteAsync()
    {
        _logger.LogInformation("Backtest polling job started");
        try
        {
            var newBacktests = await _poller.PollForNewBacktestsAsync(CancellationToken.None);

            if (newBacktests.Count == 0)
            {
                _logger.LogInformation("Backtest polling job completed. No new backtests found.");
                return;
            }

            _logger.LogInformation("Starting parallel analysis for {Count} new backtests (max 3 concurrent Claude calls)",
                newBacktests.Count);

            // SemaphoreSlim caps concurrent Claude API calls. The QC HTTP polling above already
            // ran fully parallel; the semaphore guards only the analysis (metrics + Claude) phase.
            using var semaphore = new SemaphoreSlim(3, 3);

            var analysisTasks = newBacktests.Select(async bt =>
            {
                await semaphore.WaitAsync(CancellationToken.None);
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var backtestService = scope.ServiceProvider.GetRequiredService<BacktestService>();
                    await backtestService.AnalyzeAsync(bt.Id, CancellationToken.None);
                    _logger.LogInformation("Analysis complete for backtest {BacktestId} ({ProjectName} / {StrategyName})",
                        bt.Id, bt.ProjectName, bt.StrategyName);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogError(ex, "Failed to analyze backtest {BacktestId}", bt.Id);
                }
                finally
                {
                    semaphore.Release();
                }
            });

            await Task.WhenAll(analysisTasks);
            _logger.LogInformation("Backtest polling job completed. Processed {Count} new backtests.", newBacktests.Count);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Backtest polling job failed");
        }
    }
}
