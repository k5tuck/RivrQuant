namespace RivrQuant.Domain.Enums;

/// <summary>
/// Represents the severity level of a system or trading alert.
/// </summary>
public enum AlertSeverity
{
    /// <summary>
    /// Informational alert for routine events that require no immediate action.
    /// </summary>
    Info,

    /// <summary>
    /// Warning alert indicating a condition that may require attention
    /// if it persists or worsens.
    /// </summary>
    Warning,

    /// <summary>
    /// Critical alert indicating a severe condition that requires immediate attention,
    /// such as a risk limit breach or system failure.
    /// </summary>
    Critical
}
