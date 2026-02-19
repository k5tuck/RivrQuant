// Copyright (c) RivrQuant. All rights reserved.
// Licensed under the MIT License.

namespace RivrQuant.Application.DTOs;

/// <summary>
/// Projection of a user-configured alert rule for display and management,
/// including condition configuration and delivery channel settings.
/// </summary>
/// <param name="Id">Unique identifier for the alert rule.</param>
/// <param name="Name">Human-readable name for the rule.</param>
/// <param name="ConditionType">Type of condition to evaluate (e.g., DrawdownExceedsPercent).</param>
/// <param name="Threshold">Threshold value for the condition trigger.</param>
/// <param name="Severity">Severity level of alerts triggered by this rule.</param>
/// <param name="IsActive">Whether the rule is currently active.</param>
/// <param name="SendEmail">Whether email notifications are enabled.</param>
/// <param name="SendSms">Whether SMS notifications are enabled.</param>
public sealed record AlertRuleDto(
    Guid Id,
    string Name,
    string ConditionType,
    decimal Threshold,
    string Severity,
    bool IsActive,
    bool SendEmail,
    bool SendSms);

/// <summary>
/// Projection of a triggered alert event for display in alert history and notification views.
/// </summary>
/// <param name="Id">Unique identifier for the alert event.</param>
/// <param name="RuleName">Name of the rule that triggered this event.</param>
/// <param name="Severity">Severity level of this alert.</param>
/// <param name="Message">Descriptive message about the alert condition.</param>
/// <param name="CurrentValue">The current value that triggered the alert.</param>
/// <param name="Threshold">The threshold value from the rule.</param>
/// <param name="IsAcknowledged">Whether the alert has been acknowledged by the user.</param>
/// <param name="TriggeredAt">Timestamp when the alert was triggered.</param>
public sealed record AlertEventDto(
    Guid Id,
    string RuleName,
    string Severity,
    string Message,
    decimal CurrentValue,
    decimal Threshold,
    bool IsAcknowledged,
    DateTimeOffset TriggeredAt);

/// <summary>
/// Request payload for creating a new alert rule with condition configuration,
/// delivery channel preferences, and cooldown settings.
/// </summary>
/// <param name="Name">Human-readable name for the rule.</param>
/// <param name="ConditionType">Type of condition to evaluate (e.g., DrawdownExceedsPercent, DailyLossExceedsAmount).</param>
/// <param name="Threshold">Threshold value for the condition trigger.</param>
/// <param name="ComparisonOperator">Comparison operator (GreaterThan, LessThan, Equals).</param>
/// <param name="Severity">Severity level (Info, Warning, Critical).</param>
/// <param name="SendEmail">Whether to send email notifications.</param>
/// <param name="SendSms">Whether to send SMS notifications.</param>
/// <param name="CooldownMinutes">Minimum time in minutes between repeated triggers.</param>
public sealed record CreateAlertRuleDto(
    string Name,
    string ConditionType,
    decimal Threshold,
    string ComparisonOperator,
    string Severity,
    bool SendEmail,
    bool SendSms,
    int CooldownMinutes);
