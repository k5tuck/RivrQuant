// Copyright (c) RivrQuant. All rights reserved.
// Licensed under the MIT License.

using RivrQuant.Domain.Models.Alerts;

namespace RivrQuant.Domain.Interfaces;

/// <summary>
/// Manages alert rules and their evaluation lifecycle, including creation,
/// modification, evaluation against live data, and historical event retrieval.
/// </summary>
public interface IAlertService
{
    /// <summary>
    /// Creates a new alert rule and persists it for future evaluation cycles.
    /// </summary>
    /// <param name="rule">
    /// The <see cref="AlertRule"/> to create. The <see cref="AlertRule.Id"/> may be
    /// empty and will be assigned by the service.
    /// </param>
    /// <param name="ct">A cancellation token to cancel the asynchronous operation.</param>
    /// <returns>The persisted <see cref="AlertRule"/> with its assigned identifier.</returns>
    Task<AlertRule> CreateRuleAsync(AlertRule rule, CancellationToken ct);

    /// <summary>
    /// Updates an existing alert rule's configuration. The rule is matched
    /// by its <see cref="AlertRule.Id"/> property.
    /// </summary>
    /// <param name="rule">The <see cref="AlertRule"/> containing the updated configuration.</param>
    /// <param name="ct">A cancellation token to cancel the asynchronous operation.</param>
    /// <returns>The updated <see cref="AlertRule"/> as persisted.</returns>
    Task<AlertRule> UpdateRuleAsync(AlertRule rule, CancellationToken ct);

    /// <summary>
    /// Permanently deletes an alert rule by its unique identifier.
    /// Any pending evaluations for this rule are also cancelled.
    /// </summary>
    /// <param name="ruleId">The unique identifier of the rule to delete.</param>
    /// <param name="ct">A cancellation token to cancel the asynchronous operation.</param>
    Task DeleteRuleAsync(Guid ruleId, CancellationToken ct);

    /// <summary>
    /// Retrieves all currently active (enabled) alert rules.
    /// </summary>
    /// <param name="ct">A cancellation token to cancel the asynchronous operation.</param>
    /// <returns>A read-only list of active <see cref="AlertRule"/> instances.</returns>
    Task<IReadOnlyList<AlertRule>> GetActiveRulesAsync(CancellationToken ct);

    /// <summary>
    /// Retrieves the history of triggered alert events within the specified date range.
    /// </summary>
    /// <param name="from">The inclusive start of the date range (UTC).</param>
    /// <param name="to">The inclusive end of the date range (UTC).</param>
    /// <param name="ct">A cancellation token to cancel the asynchronous operation.</param>
    /// <returns>
    /// A read-only list of <see cref="AlertEvent"/> entries ordered by trigger time descending.
    /// </returns>
    Task<IReadOnlyList<AlertEvent>> GetAlertHistoryAsync(DateTimeOffset from, DateTimeOffset to, CancellationToken ct);

    /// <summary>
    /// Evaluates all active alert rules against current market data and portfolio state.
    /// Any rules whose conditions are met will generate corresponding <see cref="AlertEvent"/> entries
    /// and dispatch notifications through configured channels.
    /// </summary>
    /// <param name="ct">A cancellation token to cancel the asynchronous operation.</param>
    Task EvaluateRulesAsync(CancellationToken ct);

    /// <summary>
    /// Marks an alert event as acknowledged, preventing it from appearing in
    /// unacknowledged alert queries and suppressing further notifications for this event.
    /// </summary>
    /// <param name="alertEventId">The unique identifier of the alert event to acknowledge.</param>
    /// <param name="ct">A cancellation token to cancel the asynchronous operation.</param>
    Task AcknowledgeAlertAsync(Guid alertEventId, CancellationToken ct);
}
