namespace RivrQuant.Domain.Models.Alerts;

/// <summary>Represents an alert delivery channel configuration (email or SMS).</summary>
public class AlertChannel
{
    /// <summary>Unique identifier.</summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>Channel type (Email or SMS).</summary>
    public string ChannelType { get; init; } = string.Empty;

    /// <summary>Destination address (email address or phone number).</summary>
    public string Destination { get; init; } = string.Empty;

    /// <summary>Whether this channel is active.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Whether this channel has been verified.</summary>
    public bool IsVerified { get; set; }

    /// <summary>When this channel was created.</summary>
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}
