namespace RivrQuant.Domain.Exceptions;

/// <summary>
/// Exception thrown when backtest results cannot be retrieved from the QuantConnect API.
/// This covers API authentication failures, invalid backtest/project identifiers,
/// network errors, and unexpected response formats from the QuantConnect cloud.
/// </summary>
public class BacktestRetrievalException : RivrQuantException
{
    private const string DefaultErrorCode = "BACKTEST.RETRIEVAL";

    /// <summary>
    /// Gets the identifier of the backtest that failed to be retrieved.
    /// </summary>
    public string? BacktestId { get; init; }

    /// <summary>
    /// Gets the identifier of the QuantConnect project associated with the backtest.
    /// </summary>
    public string? ProjectId { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BacktestRetrievalException"/> class
    /// with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the retrieval failure.</param>
    public BacktestRetrievalException(string message)
        : base(message, DefaultErrorCode)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BacktestRetrievalException"/> class
    /// with a specified error message and a reference to the inner exception that
    /// is the cause of this exception.
    /// </summary>
    /// <param name="message">The message that describes the retrieval failure.</param>
    /// <param name="innerException">
    /// The exception that is the cause of the current exception, or <c>null</c> if no
    /// inner exception is specified.
    /// </param>
    public BacktestRetrievalException(string message, Exception innerException)
        : base(message, DefaultErrorCode, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BacktestRetrievalException"/> class
    /// with a specified error message, backtest identifier, and project identifier.
    /// </summary>
    /// <param name="message">The message that describes the retrieval failure.</param>
    /// <param name="backtestId">The identifier of the backtest that failed to be retrieved.</param>
    /// <param name="projectId">The identifier of the QuantConnect project.</param>
    public BacktestRetrievalException(string message, string backtestId, string projectId)
        : base(message, DefaultErrorCode)
    {
        BacktestId = backtestId;
        ProjectId = projectId;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BacktestRetrievalException"/> class
    /// with a specified error message, backtest identifier, project identifier, and a
    /// reference to the inner exception.
    /// </summary>
    /// <param name="message">The message that describes the retrieval failure.</param>
    /// <param name="backtestId">The identifier of the backtest that failed to be retrieved.</param>
    /// <param name="projectId">The identifier of the QuantConnect project.</param>
    /// <param name="innerException">
    /// The exception that is the cause of the current exception, or <c>null</c> if no
    /// inner exception is specified.
    /// </param>
    public BacktestRetrievalException(string message, string backtestId, string projectId, Exception innerException)
        : base(message, DefaultErrorCode, innerException)
    {
        BacktestId = backtestId;
        ProjectId = projectId;
    }
}
