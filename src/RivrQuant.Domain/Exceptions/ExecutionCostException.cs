using RivrQuant.Domain.Enums;

namespace RivrQuant.Domain.Exceptions;

/// <summary>
/// Exception thrown when execution cost estimation or analysis fails.
/// This covers scenarios such as missing market data for spread estimation,
/// broker-specific fee lookup failures, and slippage model calculation errors.
/// </summary>
public class ExecutionCostException : RivrQuantException
{
    private const string DefaultErrorCode = "EXECUTION.COST";

    /// <summary>
    /// Gets the symbol for which execution cost estimation failed, if applicable.
    /// </summary>
    public string? Symbol { get; init; }

    /// <summary>
    /// Gets the broker type associated with the execution cost failure, if applicable.
    /// </summary>
    public BrokerType? Broker { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExecutionCostException"/> class
    /// with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the execution cost failure.</param>
    public ExecutionCostException(string message)
        : base(message, DefaultErrorCode)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExecutionCostException"/> class
    /// with a specified error message and a reference to the inner exception that
    /// is the cause of this exception.
    /// </summary>
    /// <param name="message">The message that describes the execution cost failure.</param>
    /// <param name="innerException">
    /// The exception that is the cause of the current exception, or <c>null</c> if no
    /// inner exception is specified.
    /// </param>
    public ExecutionCostException(string message, Exception innerException)
        : base(message, DefaultErrorCode, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExecutionCostException"/> class
    /// with a specified error message, symbol, and broker type.
    /// </summary>
    /// <param name="message">The message that describes the execution cost failure.</param>
    /// <param name="symbol">The symbol for which execution cost estimation failed.</param>
    /// <param name="broker">The broker type associated with the failure.</param>
    public ExecutionCostException(string message, string symbol, BrokerType broker)
        : base(message, DefaultErrorCode)
    {
        Symbol = symbol;
        Broker = broker;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExecutionCostException"/> class
    /// with a specified error message, symbol, broker type, and a reference to the inner exception.
    /// </summary>
    /// <param name="message">The message that describes the execution cost failure.</param>
    /// <param name="symbol">The symbol for which execution cost estimation failed.</param>
    /// <param name="broker">The broker type associated with the failure.</param>
    /// <param name="innerException">
    /// The exception that is the cause of the current exception, or <c>null</c> if no
    /// inner exception is specified.
    /// </param>
    public ExecutionCostException(string message, string symbol, BrokerType broker, Exception innerException)
        : base(message, DefaultErrorCode, innerException)
    {
        Symbol = symbol;
        Broker = broker;
    }
}
