namespace RivrQuant.Infrastructure.Alerts;

/// <summary>Configuration for SendGrid email delivery.</summary>
public sealed class SendGridConfiguration
{
    /// <summary>Configuration section name.</summary>
    public const string SectionName = "SendGrid";

    /// <summary>SendGrid API key.</summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>From email address.</summary>
    public string FromEmail { get; set; } = string.Empty;

    /// <summary>From display name.</summary>
    public string FromName { get; set; } = "RivrQuant Alerts";

    /// <summary>Email recipients.</summary>
    public IReadOnlyList<string> Recipients { get; set; } = Array.Empty<string>();

    /// <summary>Validates that required configuration values are present.</summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(ApiKey))
            throw new InvalidOperationException("SENDGRID_API_KEY is required.");
        if (string.IsNullOrWhiteSpace(FromEmail))
            throw new InvalidOperationException("SENDGRID_FROM_EMAIL is required.");
    }
}
