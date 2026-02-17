namespace RivrQuant.Domain.Exceptions;

/// <summary>
/// Exception thrown when an operation cannot be completed because the available
/// capital is insufficient to cover the required amount.
/// This applies to trade placement, margin requirements, and other capital-dependent operations.
/// </summary>
public class InsufficientFundsException : RivrQuantException
{
    private const string DefaultErrorCode = "FUNDS.INSUFFICIENT";

    /// <summary>
    /// Gets the amount of capital required to complete the operation.
    /// </summary>
    public decimal RequiredAmount { get; init; }

    /// <summary>
    /// Gets the amount of capital currently available.
    /// </summary>
    public decimal AvailableAmount { get; init; }

    /// <summary>
    /// Gets the shortfall amount (the difference between required and available capital).
    /// </summary>
    public decimal Shortfall => RequiredAmount - AvailableAmount;

    /// <summary>
    /// Initializes a new instance of the <see cref="InsufficientFundsException"/> class
    /// with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the insufficient funds condition.</param>
    public InsufficientFundsException(string message)
        : base(message, DefaultErrorCode)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InsufficientFundsException"/> class
    /// with a specified error message and a reference to the inner exception that
    /// is the cause of this exception.
    /// </summary>
    /// <param name="message">The message that describes the insufficient funds condition.</param>
    /// <param name="innerException">
    /// The exception that is the cause of the current exception, or <c>null</c> if no
    /// inner exception is specified.
    /// </param>
    public InsufficientFundsException(string message, Exception innerException)
        : base(message, DefaultErrorCode, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InsufficientFundsException"/> class
    /// with a specified error message, required amount, and available amount.
    /// </summary>
    /// <param name="message">The message that describes the insufficient funds condition.</param>
    /// <param name="requiredAmount">The amount of capital required.</param>
    /// <param name="availableAmount">The amount of capital currently available.</param>
    public InsufficientFundsException(string message, decimal requiredAmount, decimal availableAmount)
        : base(message, DefaultErrorCode)
    {
        RequiredAmount = requiredAmount;
        AvailableAmount = availableAmount;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InsufficientFundsException"/> class
    /// with a specified error message, required amount, available amount, and a reference
    /// to the inner exception.
    /// </summary>
    /// <param name="message">The message that describes the insufficient funds condition.</param>
    /// <param name="requiredAmount">The amount of capital required.</param>
    /// <param name="availableAmount">The amount of capital currently available.</param>
    /// <param name="innerException">
    /// The exception that is the cause of the current exception, or <c>null</c> if no
    /// inner exception is specified.
    /// </param>
    public InsufficientFundsException(string message, decimal requiredAmount, decimal availableAmount, Exception innerException)
        : base(message, DefaultErrorCode, innerException)
    {
        RequiredAmount = requiredAmount;
        AvailableAmount = availableAmount;
    }
}
