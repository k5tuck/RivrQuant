namespace RivrQuant.Domain.Exceptions;

/// <summary>
/// Exception thrown when a connection to a brokerage API fails.
/// This covers authentication failures, network timeouts, rate limiting,
/// and other connectivity issues with broker services such as Alpaca or Coinbase.
/// </summary>
public class BrokerConnectionException : RivrQuantException
{
    private const string DefaultErrorCode = "BROKER.CONNECTION";

    /// <summary>
    /// Gets the type of broker that the connection failure occurred with.
    /// </summary>
    /// <remarks>
    /// This value corresponds to the broker integration type (e.g., "Alpaca", "Coinbase").
    /// Stored as a string to avoid a hard dependency on infrastructure enum types within the domain layer.
    /// </remarks>
    public string? BrokerType { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BrokerConnectionException"/> class
    /// with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the connection failure.</param>
    public BrokerConnectionException(string message)
        : base(message, DefaultErrorCode)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BrokerConnectionException"/> class
    /// with a specified error message and a reference to the inner exception that
    /// is the cause of this exception.
    /// </summary>
    /// <param name="message">The message that describes the connection failure.</param>
    /// <param name="innerException">
    /// The exception that is the cause of the current exception, or <c>null</c> if no
    /// inner exception is specified.
    /// </param>
    public BrokerConnectionException(string message, Exception innerException)
        : base(message, DefaultErrorCode, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BrokerConnectionException"/> class
    /// with a specified error message and broker type.
    /// </summary>
    /// <param name="message">The message that describes the connection failure.</param>
    /// <param name="brokerType">The type of broker that failed to connect.</param>
    public BrokerConnectionException(string message, string brokerType)
        : base(message, DefaultErrorCode)
    {
        BrokerType = brokerType;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BrokerConnectionException"/> class
    /// with a specified error message, broker type, and a reference to the inner exception.
    /// </summary>
    /// <param name="message">The message that describes the connection failure.</param>
    /// <param name="brokerType">The type of broker that failed to connect.</param>
    /// <param name="innerException">
    /// The exception that is the cause of the current exception, or <c>null</c> if no
    /// inner exception is specified.
    /// </param>
    public BrokerConnectionException(string message, string brokerType, Exception innerException)
        : base(message, DefaultErrorCode, innerException)
    {
        BrokerType = brokerType;
    }
}
