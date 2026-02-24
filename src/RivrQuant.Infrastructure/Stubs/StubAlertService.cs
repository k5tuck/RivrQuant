namespace RivrQuant.Infrastructure.Stubs;

using RivrQuant.Domain.Interfaces;
using RivrQuant.Domain.Models.Alerts;

/// <summary>Stub implementation of IAlertService. Returns empty data until a real implementation is wired up.</summary>
public sealed class StubAlertService : IAlertService
{
    public Task<AlertRule> CreateRuleAsync(AlertRule rule, CancellationToken ct)
        => Task.FromResult(rule);

    public Task<AlertRule> UpdateRuleAsync(AlertRule rule, CancellationToken ct)
        => Task.FromResult(rule);

    public Task DeleteRuleAsync(Guid ruleId, CancellationToken ct)
        => Task.CompletedTask;

    public Task<IReadOnlyList<AlertRule>> GetActiveRulesAsync(CancellationToken ct)
        => Task.FromResult<IReadOnlyList<AlertRule>>(Array.Empty<AlertRule>());

    public Task<IReadOnlyList<AlertEvent>> GetAlertHistoryAsync(DateTimeOffset from, DateTimeOffset to, CancellationToken ct)
        => Task.FromResult<IReadOnlyList<AlertEvent>>(Array.Empty<AlertEvent>());

    public Task EvaluateRulesAsync(CancellationToken ct)
        => Task.CompletedTask;

    public Task AcknowledgeAlertAsync(Guid alertEventId, CancellationToken ct)
        => Task.CompletedTask;
}
