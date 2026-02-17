namespace RivrQuant.Domain.Exceptions;

/// <summary>
/// Exception thrown when an alert notification fails to be delivered.
/// This covers failures in email delivery via SendGrid, SMS delivery via Twilio,
/// and other notification channel errors.
/// </summary>
public class AlertDeliveryException : RivrQuantException
{
    private const string DefaultErrorCode = "ALERT.DELIVERY";

    /// <summary>
    /// Gets the identifier of the alert rule that triggered the failed delivery.
    /// </summary>
    public Guid? AlertRuleId { get; init; }

    /// <summary>
    /// Gets the name of the delivery channel that failed.
    /// </summary>
    /// <remarks>
    /// Common channels include "Email", "Sms", "Push", and "Webhook".
    /// </remarks>
    public string? Channel { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AlertDeliveryException"/> class
    /// with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the delivery failure.</param>
    public AlertDeliveryException(string message)
        : base(message, DefaultErrorCode)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AlertDeliveryException"/> class
    /// with a specified error message and a reference to the inner exception that
    /// is the cause of this exception.
    /// </summary>
    /// <param name="message">The message that describes the delivery failure.</param>
    /// <param name="innerException">
    /// The exception that is the cause of the current exception, or <c>null</c> if no
    /// inner exception is specified.
    /// </param>
    public AlertDeliveryException(string message, Exception innerException)
        : base(message, DefaultErrorCode, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AlertDeliveryException"/> class
    /// with a specified error message, alert rule identifier, and delivery channel.
    /// </summary>
    /// <param name="message">The message that describes the delivery failure.</param>
    /// <param name="alertRuleId">The identifier of the alert rule that triggered the delivery.</param>
    /// <param name="channel">The name of the delivery channel that failed.</param>
    public AlertDeliveryException(string message, Guid alertRuleId, string channel)
        : base(message, DefaultErrorCode)
    {
        AlertRuleId = alertRuleId;
        Channel = channel;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AlertDeliveryException"/> class
    /// with a specified error message, alert rule identifier, delivery channel, and a
    /// reference to the inner exception.
    /// </summary>
    /// <param name="message">The message that describes the delivery failure.</param>
    /// <param name="alertRuleId">The identifier of the alert rule that triggered the delivery.</param>
    /// <param name="channel">The name of the delivery channel that failed.</param>
    /// <param name="innerException">
    /// The exception that is the cause of the current exception, or <c>null</c> if no
    /// inner exception is specified.
    /// </param>
    public AlertDeliveryException(string message, Guid alertRuleId, string channel, Exception innerException)
        : base(message, DefaultErrorCode, innerException)
    {
        AlertRuleId = alertRuleId;
        Channel = channel;
    }
}
