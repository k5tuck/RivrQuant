namespace RivrQuant.Infrastructure.Alerts;

using Microsoft.Extensions.Logging;
using RivrQuant.Domain.Models.Alerts;

/// <summary>Routes alerts to configured delivery channels (email, SMS).</summary>
public sealed class AlertDispatcher
{
    private readonly SendGridEmailSender _emailSender;
    private readonly TwilioSmsSender _smsSender;
    private readonly ILogger<AlertDispatcher> _logger;

    /// <summary>Initializes a new instance of <see cref="AlertDispatcher"/>.</summary>
    public AlertDispatcher(SendGridEmailSender emailSender, TwilioSmsSender smsSender, ILogger<AlertDispatcher> logger)
    {
        _emailSender = emailSender;
        _smsSender = smsSender;
        _logger = logger;
    }

    /// <summary>Dispatches an alert event to the appropriate channels based on the rule configuration.</summary>
    public async Task DispatchAsync(AlertEvent alertEvent, AlertRule rule, CancellationToken ct)
    {
        _logger.LogInformation("Dispatching alert {AlertId} for rule {RuleName}. Email={SendEmail}, SMS={SendSms}",
            alertEvent.Id, rule.Name, rule.SendEmail, rule.SendSms);

        if (rule.SendEmail)
        {
            try
            {
                await _emailSender.SendAlertEmailAsync(alertEvent, ct);
                alertEvent.EmailSent = true;
                _logger.LogInformation("Email alert dispatched for {RuleName}", rule.Name);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Email alert dispatch failed for {RuleName}", rule.Name);
                alertEvent.DeliveryError = $"Email: {ex.Message}";
            }
        }

        if (rule.SendSms)
        {
            try
            {
                await _smsSender.SendAlertSmsAsync(alertEvent, ct);
                alertEvent.SmsSent = true;
                _logger.LogInformation("SMS alert dispatched for {RuleName}", rule.Name);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "SMS alert dispatch failed for {RuleName}", rule.Name);
                var existing = alertEvent.DeliveryError;
                alertEvent.DeliveryError = string.IsNullOrEmpty(existing)
                    ? $"SMS: {ex.Message}"
                    : $"{existing}; SMS: {ex.Message}";
            }
        }
    }
}
