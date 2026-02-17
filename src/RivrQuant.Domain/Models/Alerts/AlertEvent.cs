namespace RivrQuant.Domain.Models.Alerts;

/// <summary>Records a triggered alert event with delivery status.</summary>
public class AlertEvent
{
    /// <summary>Unique identifier for this alert event.</summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>The alert rule that triggered this event.</summary>
    public Guid AlertRuleId { get; init; }

    /// <summary>Name of the rule that triggered this event.</summary>
    public string RuleName { get; init; } = string.Empty;

    /// <summary>Severity of this alert.</summary>
    public Enums.AlertSeverity Severity { get; init; }

    /// <summary>Descriptive message about the alert condition.</summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>The current value that triggered the alert.</summary>
    public decimal CurrentValue { get; init; }

    /// <summary>The threshold value from the rule.</summary>
    public decimal ThresholdValue { get; init; }

    /// <summary>Whether the email notification was sent successfully.</summary>
    public bool EmailSent { get; set; }

    /// <summary>Whether the SMS notification was sent successfully.</summary>
    public bool SmsSent { get; set; }

    /// <summary>Error message if delivery failed.</summary>
    public string? DeliveryError { get; set; }

    /// <summary>Whether this alert has been acknowledged by the user.</summary>
    public bool IsAcknowledged { get; set; }

    /// <summary>When this alert was triggered.</summary>
    public DateTimeOffset TriggeredAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>When this alert was acknowledged.</summary>
    public DateTimeOffset? AcknowledgedAt { get; set; }
}
