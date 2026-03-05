using RivrQuant.Domain.Enums;

namespace RivrQuant.Domain.Exceptions;

/// <summary>
/// Exception thrown when position sizing calculation fails or produces an invalid result.
/// This covers scenarios such as missing market data for volatility estimation,
/// invalid Kelly criterion inputs, or sizing results that violate portfolio constraints.
/// </summary>
public class PositionSizingException : RivrQuantException
{
    private const string DefaultErrorCode = "RISK.POSITION_SIZING";

    /// <summary>
    /// Gets the symbol for which position sizing failed, if applicable.
    /// </summary>
    public string? Symbol { get; init; }

    /// <summary>
    /// Gets the position sizing method that was being used when the failure occurred, if applicable.
    /// </summary>
    public PositionSizingMethod? Method { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PositionSizingException"/> class
    /// with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the position sizing failure.</param>
    public PositionSizingException(string message)
        : base(message, DefaultErrorCode)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PositionSizingException"/> class
    /// with a specified error message and a reference to the inner exception that
    /// is the cause of this exception.
    /// </summary>
    /// <param name="message">The message that describes the position sizing failure.</param>
    /// <param name="innerException">
    /// The exception that is the cause of the current exception, or <c>null</c> if no
    /// inner exception is specified.
    /// </param>
    public PositionSizingException(string message, Exception innerException)
        : base(message, DefaultErrorCode, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PositionSizingException"/> class
    /// with a specified error message, symbol, and sizing method.
    /// </summary>
    /// <param name="message">The message that describes the position sizing failure.</param>
    /// <param name="symbol">The symbol for which position sizing failed.</param>
    /// <param name="method">The position sizing method that was being used.</param>
    public PositionSizingException(string message, string symbol, PositionSizingMethod method)
        : base(message, DefaultErrorCode)
    {
        Symbol = symbol;
        Method = method;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PositionSizingException"/> class
    /// with a specified error message, symbol, sizing method, and a reference to the inner exception.
    /// </summary>
    /// <param name="message">The message that describes the position sizing failure.</param>
    /// <param name="symbol">The symbol for which position sizing failed.</param>
    /// <param name="method">The position sizing method that was being used.</param>
    /// <param name="innerException">
    /// The exception that is the cause of the current exception, or <c>null</c> if no
    /// inner exception is specified.
    /// </param>
    public PositionSizingException(string message, string symbol, PositionSizingMethod method, Exception innerException)
        : base(message, DefaultErrorCode, innerException)
    {
        Symbol = symbol;
        Method = method;
    }
}
