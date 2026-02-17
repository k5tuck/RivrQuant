namespace RivrQuant.Domain.Models.Alerts;

/// <summary>Defines a user-configured alert condition with delivery channels.</summary>
public class AlertRule
{
    /// <summary>Unique identifier for this alert rule.</summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>Human-readable name for the rule.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Type of condition to evaluate (e.g., DrawdownExceedsPercent, DailyLossExceedsAmount).</summary>
    public string ConditionType { get; init; } = string.Empty;

    /// <summary>Threshold value for the condition.</summary>
    public decimal Threshold { get; init; }

    /// <summary>Comparison operator (GreaterThan, LessThan, Equals).</summary>
    public string ComparisonOperator { get; init; } = "GreaterThan";

    /// <summary>Severity level of alerts triggered by this rule.</summary>
    public Enums.AlertSeverity Severity { get; init; }

    /// <summary>Whether this rule is currently active.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Whether to send email notifications.</summary>
    public bool SendEmail { get; init; }

    /// <summary>Whether to send SMS notifications.</summary>
    public bool SendSms { get; init; }

    /// <summary>Minimum time between repeated triggers of this rule.</summary>
    public TimeSpan CooldownPeriod { get; init; } = TimeSpan.FromHours(1);

    /// <summary>When this rule was last triggered.</summary>
    public DateTimeOffset? LastTriggeredAt { get; set; }

    /// <summary>When this rule was created.</summary>
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}
