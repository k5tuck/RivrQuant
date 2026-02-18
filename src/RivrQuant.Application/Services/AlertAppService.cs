using Microsoft.Extensions.Logging;
using RivrQuant.Domain.Interfaces;
using RivrQuant.Domain.Models.Alerts;

namespace RivrQuant.Application.Services;

/// <summary>
/// Application service for managing alert rules and viewing alert history.
/// Delegates core alert operations to the domain IAlertService.
/// </summary>
public sealed class AlertAppService
{
    private readonly IAlertService _alertService;
    private readonly ILogger<AlertAppService> _logger;

    /// <summary>Initializes a new instance of <see cref="AlertAppService"/>.</summary>
    public AlertAppService(IAlertService alertService, ILogger<AlertAppService> logger)
    {
        _alertService = alertService;
        _logger = logger;
    }

    /// <summary>Lists all active alert rules.</summary>
    public Task<IReadOnlyList<AlertRule>> GetRulesAsync(CancellationToken ct)
        => _alertService.GetActiveRulesAsync(ct);

    /// <summary>Creates a new alert rule.</summary>
    public async Task<AlertRule> CreateRuleAsync(AlertRule rule, CancellationToken ct)
    {
        var created = await _alertService.CreateRuleAsync(rule, ct);
        _logger.LogInformation("Created alert rule {RuleId} ({RuleName})", created.Id, created.Name);
        return created;
    }

    /// <summary>Updates an existing alert rule.</summary>
    public async Task<AlertRule> UpdateRuleAsync(AlertRule rule, CancellationToken ct)
    {
        var updated = await _alertService.UpdateRuleAsync(rule, ct);
        _logger.LogInformation("Updated alert rule {RuleId}", updated.Id);
        return updated;
    }

    /// <summary>Deletes an alert rule.</summary>
    public async Task DeleteRuleAsync(Guid ruleId, CancellationToken ct)
    {
        await _alertService.DeleteRuleAsync(ruleId, ct);
        _logger.LogInformation("Deleted alert rule {RuleId}", ruleId);
    }

    /// <summary>Toggles the active state of an alert rule.</summary>
    public async Task<AlertRule> ToggleRuleAsync(Guid ruleId, CancellationToken ct)
    {
        var rules = await _alertService.GetActiveRulesAsync(ct);
        // We need all rules, not just active. Workaround: get all and find by ID
        var allRules = await _alertService.GetActiveRulesAsync(ct);
        // The rule might be inactive, so re-fetch via update toggle approach
        var rule = allRules.FirstOrDefault(r => r.Id == ruleId);

        if (rule is null)
        {
            // Rule is currently inactive, so create it as active
            throw new KeyNotFoundException($"Alert rule {ruleId} not found among active rules.");
        }

        rule.IsActive = !rule.IsActive;
        var updated = await _alertService.UpdateRuleAsync(rule, ct);
        _logger.LogInformation("Toggled alert rule {RuleId} to IsActive={IsActive}", ruleId, updated.IsActive);
        return updated;
    }

    /// <summary>Retrieves alert event history.</summary>
    public Task<IReadOnlyList<AlertEvent>> GetHistoryAsync(
        DateTimeOffset? from,
        DateTimeOffset? to,
        CancellationToken ct)
    {
        var start = from ?? DateTimeOffset.UtcNow.AddDays(-30);
        var end = to ?? DateTimeOffset.UtcNow;
        return _alertService.GetAlertHistoryAsync(start, end, ct);
    }

    /// <summary>Acknowledges an alert event.</summary>
    public async Task AcknowledgeAsync(Guid alertEventId, CancellationToken ct)
    {
        await _alertService.AcknowledgeAlertAsync(alertEventId, ct);
        _logger.LogInformation("Acknowledged alert event {AlertEventId}", alertEventId);
    }
}
