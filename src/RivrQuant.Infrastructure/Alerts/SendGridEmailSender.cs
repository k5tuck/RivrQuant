namespace RivrQuant.Infrastructure.Alerts;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RivrQuant.Domain.Exceptions;
using RivrQuant.Domain.Models.Alerts;
using SendGrid;
using SendGrid.Helpers.Mail;

/// <summary>Sends alert emails via SendGrid API.</summary>
public sealed class SendGridEmailSender
{
    private readonly SendGridClient _client;
    private readonly SendGridConfiguration _config;
    private readonly ILogger<SendGridEmailSender> _logger;

    /// <summary>Initializes a new instance of <see cref="SendGridEmailSender"/>.</summary>
    public SendGridEmailSender(IOptions<SendGridConfiguration> config, ILogger<SendGridEmailSender> logger)
    {
        _config = config.Value;
        _client = new SendGridClient(_config.ApiKey);
        _logger = logger;
    }

    /// <summary>Sends an alert email to all configured recipients.</summary>
    public async Task SendAlertEmailAsync(AlertEvent alertEvent, CancellationToken ct)
    {
        _logger.LogInformation("Sending alert email for rule {RuleName} to {RecipientCount} recipients",
            alertEvent.RuleName, _config.Recipients.Count);

        var severityColor = alertEvent.Severity switch
        {
            Domain.Enums.AlertSeverity.Critical => "#dc2626",
            Domain.Enums.AlertSeverity.Warning => "#f59e0b",
            _ => "#3b82f6"
        };

        var html = $"""
            <div style="font-family: -apple-system, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;">
                <div style="background: {severityColor}; color: white; padding: 16px; border-radius: 8px 8px 0 0;">
                    <h2 style="margin: 0;">RivrQuant Alert: {alertEvent.RuleName}</h2>
                    <span style="background: rgba(255,255,255,0.2); padding: 4px 8px; border-radius: 4px; font-size: 12px;">
                        {alertEvent.Severity}
                    </span>
                </div>
                <div style="border: 1px solid #e5e7eb; border-top: none; padding: 20px; border-radius: 0 0 8px 8px;">
                    <p style="color: #374151; font-size: 16px;">{alertEvent.Message}</p>
                    <table style="width: 100%; border-collapse: collapse; margin: 16px 0;">
                        <tr>
                            <td style="padding: 8px; border-bottom: 1px solid #e5e7eb; color: #6b7280;">Current Value</td>
                            <td style="padding: 8px; border-bottom: 1px solid #e5e7eb; font-weight: bold;">{alertEvent.CurrentValue:N4}</td>
                        </tr>
                        <tr>
                            <td style="padding: 8px; border-bottom: 1px solid #e5e7eb; color: #6b7280;">Threshold</td>
                            <td style="padding: 8px; border-bottom: 1px solid #e5e7eb; font-weight: bold;">{alertEvent.ThresholdValue:N4}</td>
                        </tr>
                        <tr>
                            <td style="padding: 8px; color: #6b7280;">Triggered At</td>
                            <td style="padding: 8px; font-weight: bold;">{alertEvent.TriggeredAt:yyyy-MM-dd HH:mm:ss} UTC</td>
                        </tr>
                    </table>
                    <p style="color: #6b7280; font-size: 12px; margin-top: 16px;">
                        This is an automated alert from RivrQuant. Review your dashboard for more details.
                    </p>
                </div>
            </div>
            """;

        foreach (var recipient in _config.Recipients)
        {
            var msg = new SendGridMessage
            {
                From = new EmailAddress(_config.FromEmail, _config.FromName),
                Subject = $"[RivrQuant {alertEvent.Severity}] {alertEvent.RuleName}",
                HtmlContent = html
            };
            msg.AddTo(new EmailAddress(recipient));

            var response = await _client.SendEmailAsync(msg, ct);
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Body.ReadAsStringAsync(ct);
                _logger.LogError("SendGrid delivery failed for {Recipient}: {StatusCode} {Error}", recipient, response.StatusCode, errorBody);
                throw new AlertDeliveryException($"SendGrid delivery failed: {response.StatusCode}")
                {
                    AlertRuleId = alertEvent.AlertRuleId,
                    Channel = "Email"
                };
            }
            _logger.LogInformation("Alert email sent to {Recipient}", recipient);
        }
    }

    /// <summary>Sends a test email to verify SendGrid configuration.</summary>
    public async Task SendTestEmailAsync(CancellationToken ct)
    {
        _logger.LogInformation("Sending test email via SendGrid");
        var msg = new SendGridMessage
        {
            From = new EmailAddress(_config.FromEmail, _config.FromName),
            Subject = "[RivrQuant] Test Alert Email",
            HtmlContent = "<p>This is a test alert from RivrQuant. If you received this, your email alerts are configured correctly.</p>"
        };
        foreach (var r in _config.Recipients) msg.AddTo(new EmailAddress(r));
        await _client.SendEmailAsync(msg, ct);
    }
}
