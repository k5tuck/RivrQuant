namespace RivrQuant.Infrastructure.Alerts;

/// <summary>Configuration for Twilio SMS delivery.</summary>
public sealed class TwilioConfiguration
{
    /// <summary>Configuration section name.</summary>
    public const string SectionName = "Twilio";

    /// <summary>Twilio Account SID.</summary>
    public string AccountSid { get; set; } = string.Empty;

    /// <summary>Twilio Auth Token.</summary>
    public string AuthToken { get; set; } = string.Empty;

    /// <summary>Twilio phone number to send from.</summary>
    public string FromNumber { get; set; } = string.Empty;

    /// <summary>SMS recipients.</summary>
    public IReadOnlyList<string> Recipients { get; set; } = Array.Empty<string>();

    /// <summary>Validates that required configuration values are present.</summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(AccountSid))
            throw new InvalidOperationException("TWILIO_ACCOUNT_SID is required.");
        if (string.IsNullOrWhiteSpace(AuthToken))
            throw new InvalidOperationException("TWILIO_AUTH_TOKEN is required.");
        if (string.IsNullOrWhiteSpace(FromNumber))
            throw new InvalidOperationException("TWILIO_FROM_NUMBER is required.");
    }
}
