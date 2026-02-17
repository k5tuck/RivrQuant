namespace RivrQuant.Infrastructure.Alerts;

using Microsoft.Extensions.Logging;
using RivrQuant.Domain.Models.Alerts;
using RivrQuant.Domain.Models.Trading;

/// <summary>Evaluates alert rule conditions against current portfolio state.</summary>
public sealed class AlertRuleEvaluator
{
    private readonly ILogger<AlertRuleEvaluator> _logger;

    /// <summary>Initializes a new instance of <see cref="AlertRuleEvaluator"/>.</summary>
    public AlertRuleEvaluator(ILogger<AlertRuleEvaluator> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Evaluates a single alert rule against current state.
    /// Returns an AlertEvent if the condition is triggered, null otherwise.
    /// </summary>
    public AlertEvent? Evaluate(AlertRule rule, Portfolio portfolio, PerformanceSnapshot? latestSnapshot)
    {
        if (!rule.IsActive) return null;

        if (rule.LastTriggeredAt.HasValue &&
            rule.LastTriggeredAt.Value + rule.CooldownPeriod > DateTimeOffset.UtcNow)
        {
            return null;
        }

        var (triggered, currentValue, message) = rule.ConditionType switch
        {
            "DrawdownExceedsPercent" => EvaluateDrawdown(rule, latestSnapshot),
            "DailyLossExceedsAmount" => EvaluateDailyLoss(rule, portfolio),
            "PositionSizeExceedsPercent" => EvaluatePositionSize(rule, portfolio),
            "BrokerDisconnected" => (false, 0m, string.Empty),
            "NewBacktestComplete" => (false, 0m, string.Empty),
            "AiAnalysisComplete" => (false, 0m, string.Empty),
            "AiCriticalWarning" => (false, 0m, string.Empty),
            _ => (false, 0m, $"Unknown condition type: {rule.ConditionType}")
        };

        if (!triggered) return null;

        _logger.LogWarning("Alert rule {RuleName} triggered: {Message}. Current={CurrentValue}, Threshold={Threshold}",
            rule.Name, message, currentValue, rule.Threshold);

        return new AlertEvent
        {
            AlertRuleId = rule.Id,
            RuleName = rule.Name,
            Severity = rule.Severity,
            Message = message,
            CurrentValue = currentValue,
            ThresholdValue = rule.Threshold
        };
    }

    /// <summary>Creates an alert event for event-based triggers (new backtest, analysis complete, etc.).</summary>
    public AlertEvent CreateEventAlert(AlertRule rule, string message, decimal currentValue = 0)
    {
        _logger.LogInformation("Event alert created for rule {RuleName}: {Message}", rule.Name, message);
        return new AlertEvent
        {
            AlertRuleId = rule.Id,
            RuleName = rule.Name,
            Severity = rule.Severity,
            Message = message,
            CurrentValue = currentValue,
            ThresholdValue = rule.Threshold
        };
    }

    private static (bool Triggered, decimal CurrentValue, string Message) EvaluateDrawdown(AlertRule rule, PerformanceSnapshot? snapshot)
    {
        if (snapshot is null) return (false, 0, string.Empty);
        var currentDd = Math.Abs(snapshot.CurrentDrawdown);
        var threshold = Math.Abs(rule.Threshold);
        var triggered = currentDd >= threshold;
        return (triggered, currentDd, $"Current drawdown {currentDd:P2} exceeds limit of {threshold:P2}");
    }

    private static (bool Triggered, decimal CurrentValue, string Message) EvaluateDailyLoss(AlertRule rule, Portfolio portfolio)
    {
        var dailyPnl = portfolio.RealizedPnlToday + portfolio.UnrealizedPnl;
        var threshold = rule.Threshold;
        var triggered = dailyPnl < threshold;
        return (triggered, dailyPnl, $"Daily P&L ${dailyPnl:N2} below threshold ${threshold:N2}");
    }

    private static (bool Triggered, decimal CurrentValue, string Message) EvaluatePositionSize(AlertRule rule, Portfolio portfolio)
    {
        return (false, 0, string.Empty);
    }
}
