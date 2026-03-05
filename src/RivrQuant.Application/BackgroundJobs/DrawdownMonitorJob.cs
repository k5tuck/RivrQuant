namespace RivrQuant.Application.BackgroundJobs;

using Microsoft.Extensions.Logging;
using RivrQuant.Domain.Interfaces;

/// <summary>Hangfire recurring job that monitors drawdown and triggers deleveraging every 15 seconds.</summary>
public sealed class DrawdownMonitorJob
{
    private readonly IDrawdownManager _drawdownManager;
    private readonly ILogger<DrawdownMonitorJob> _logger;

    public DrawdownMonitorJob(
        IDrawdownManager drawdownManager,
        ILogger<DrawdownMonitorJob> logger)
    {
        _drawdownManager = drawdownManager;
        _logger = logger;
    }

    /// <summary>Evaluates drawdown state and triggers deleveraging actions if thresholds are breached.</summary>
    public async Task ExecuteAsync()
    {
        try
        {
            await _drawdownManager.EvaluateAndActAsync(CancellationToken.None);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // If the drawdown monitor crashes, log CRITICAL and let Hangfire retry.
            // The DrawdownManager itself should fail-safe (halt trading if it can't evaluate).
            _logger.LogCritical(ex, "DrawdownMonitorJob FAILED — drawdown protection may be compromised. Trading should be halted until this is resolved.");
        }
    }
}
