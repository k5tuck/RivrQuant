namespace RivrQuant.Domain.Exceptions;

/// <summary>
/// Base exception for all RivrQuant domain errors.
/// All custom exceptions within the RivrQuant system should inherit from this class
/// to enable consistent error handling and categorization via <see cref="ErrorCode"/>.
/// </summary>
public class RivrQuantException : Exception
{
    /// <summary>
    /// Gets the application-specific error code that categorizes this exception.
    /// </summary>
    /// <remarks>
    /// Error codes follow a dot-notation convention (e.g., "BROKER.CONNECTION", "RISK.LIMIT")
    /// to allow structured error handling and logging.
    /// </remarks>
    public string? ErrorCode { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RivrQuantException"/> class.
    /// </summary>
    public RivrQuantException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RivrQuantException"/> class
    /// with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public RivrQuantException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RivrQuantException"/> class
    /// with a specified error message and a reference to the inner exception that
    /// is the cause of this exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">
    /// The exception that is the cause of the current exception, or <c>null</c> if no
    /// inner exception is specified.
    /// </param>
    public RivrQuantException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RivrQuantException"/> class
    /// with a specified error message and error code.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="errorCode">The application-specific error code.</param>
    public RivrQuantException(string message, string errorCode)
        : base(message)
    {
        ErrorCode = errorCode;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RivrQuantException"/> class
    /// with a specified error message, error code, and a reference to the inner exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="errorCode">The application-specific error code.</param>
    /// <param name="innerException">
    /// The exception that is the cause of the current exception, or <c>null</c> if no
    /// inner exception is specified.
    /// </param>
    public RivrQuantException(string message, string errorCode, Exception innerException)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }
}
