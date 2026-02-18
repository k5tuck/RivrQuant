namespace RivrQuant.Application.BackgroundJobs;

using Microsoft.Extensions.Logging;
using RivrQuant.Domain.Interfaces;

/// <summary>Hangfire recurring job that evaluates alert rules against current portfolio state.</summary>
public sealed class AlertEvaluationJob
{
    private readonly IAlertService _alertService;
    private readonly ILogger<AlertEvaluationJob> _logger;

    /// <summary>Initializes a new instance of <see cref="AlertEvaluationJob"/>.</summary>
    public AlertEvaluationJob(IAlertService alertService, ILogger<AlertEvaluationJob> logger)
    {
        _alertService = alertService;
        _logger = logger;
    }

    /// <summary>Evaluates all active alert rules.</summary>
    public async Task ExecuteAsync()
    {
        try
        {
            await _alertService.EvaluateRulesAsync(CancellationToken.None);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Alert evaluation job failed");
        }
    }
}
