namespace RivrQuant.Infrastructure.Alerts;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RivrQuant.Domain.Exceptions;
using RivrQuant.Domain.Models.Alerts;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

/// <summary>Sends alert SMS messages via Twilio API.</summary>
public sealed class TwilioSmsSender
{
    private readonly TwilioConfiguration _config;
    private readonly ILogger<TwilioSmsSender> _logger;

    /// <summary>Initializes a new instance of <see cref="TwilioSmsSender"/>.</summary>
    public TwilioSmsSender(IOptions<TwilioConfiguration> config, ILogger<TwilioSmsSender> logger)
    {
        _config = config.Value;
        _logger = logger;
        TwilioClient.Init(_config.AccountSid, _config.AuthToken);
    }

    /// <summary>Sends an alert SMS to all configured recipients.</summary>
    public async Task SendAlertSmsAsync(AlertEvent alertEvent, CancellationToken ct)
    {
        _logger.LogInformation("Sending alert SMS for rule {RuleName} to {RecipientCount} recipients",
            alertEvent.RuleName, _config.Recipients.Count);

        var body = $"[RivrQuant {alertEvent.Severity}] {alertEvent.RuleName}: Current {alertEvent.CurrentValue:N2} exceeds threshold {alertEvent.ThresholdValue:N2}. {alertEvent.TriggeredAt:HH:mm} UTC";

        if (body.Length > 160)
            body = body[..157] + "...";

        foreach (var recipient in _config.Recipients)
        {
            try
            {
                var message = await MessageResource.CreateAsync(
                    to: new PhoneNumber(recipient),
                    from: new PhoneNumber(_config.FromNumber),
                    body: body);

                _logger.LogInformation("Alert SMS sent to {Recipient}. SID: {MessageSid}", recipient, message.Sid);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Twilio SMS delivery failed for {Recipient}", recipient);
                throw new AlertDeliveryException($"Twilio SMS delivery failed for {recipient}: {ex.Message}", ex)
                {
                    AlertRuleId = alertEvent.AlertRuleId,
                    Channel = "SMS"
                };
            }
        }
    }

    /// <summary>Sends a test SMS to verify Twilio configuration.</summary>
    public async Task SendTestSmsAsync(CancellationToken ct)
    {
        _logger.LogInformation("Sending test SMS via Twilio");
        foreach (var recipient in _config.Recipients)
        {
            await MessageResource.CreateAsync(
                to: new PhoneNumber(recipient),
                from: new PhoneNumber(_config.FromNumber),
                body: "[RivrQuant] Test SMS. Your SMS alerts are configured correctly.");
        }
    }
}
