namespace RivrQuant.Domain.Exceptions;

/// <summary>
/// Exception thrown when a risk management limit is violated.
/// This covers position size limits, portfolio concentration limits, maximum drawdown thresholds,
/// daily loss limits, and other risk constraints configured for the trading system.
/// </summary>
public class RiskLimitExceededException : RivrQuantException
{
    private const string DefaultErrorCode = "RISK.LIMIT_EXCEEDED";

    /// <summary>
    /// Gets the type of risk limit that was exceeded.
    /// </summary>
    /// <remarks>
    /// Common limit types include "MaxPositionSize", "MaxDrawdown", "DailyLossLimit",
    /// "PortfolioConcentration", and "MaxOpenPositions".
    /// </remarks>
    public string? LimitType { get; init; }

    /// <summary>
    /// Gets the current value that exceeded the threshold.
    /// </summary>
    public decimal CurrentValue { get; init; }

    /// <summary>
    /// Gets the threshold value that was exceeded.
    /// </summary>
    public decimal ThresholdValue { get; init; }

    /// <summary>
    /// Gets the amount by which the current value exceeds the threshold.
    /// </summary>
    public decimal ExcessAmount => CurrentValue - ThresholdValue;

    /// <summary>
    /// Initializes a new instance of the <see cref="RiskLimitExceededException"/> class
    /// with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the risk limit violation.</param>
    public RiskLimitExceededException(string message)
        : base(message, DefaultErrorCode)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RiskLimitExceededException"/> class
    /// with a specified error message and a reference to the inner exception that
    /// is the cause of this exception.
    /// </summary>
    /// <param name="message">The message that describes the risk limit violation.</param>
    /// <param name="innerException">
    /// The exception that is the cause of the current exception, or <c>null</c> if no
    /// inner exception is specified.
    /// </param>
    public RiskLimitExceededException(string message, Exception innerException)
        : base(message, DefaultErrorCode, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RiskLimitExceededException"/> class
    /// with a specified error message, limit type, current value, and threshold value.
    /// </summary>
    /// <param name="message">The message that describes the risk limit violation.</param>
    /// <param name="limitType">The type of risk limit that was exceeded.</param>
    /// <param name="currentValue">The current value that exceeded the threshold.</param>
    /// <param name="thresholdValue">The threshold value that was exceeded.</param>
    public RiskLimitExceededException(string message, string limitType, decimal currentValue, decimal thresholdValue)
        : base(message, DefaultErrorCode)
    {
        LimitType = limitType;
        CurrentValue = currentValue;
        ThresholdValue = thresholdValue;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RiskLimitExceededException"/> class
    /// with a specified error message, limit type, current value, threshold value, and a
    /// reference to the inner exception.
    /// </summary>
    /// <param name="message">The message that describes the risk limit violation.</param>
    /// <param name="limitType">The type of risk limit that was exceeded.</param>
    /// <param name="currentValue">The current value that exceeded the threshold.</param>
    /// <param name="thresholdValue">The threshold value that was exceeded.</param>
    /// <param name="innerException">
    /// The exception that is the cause of the current exception, or <c>null</c> if no
    /// inner exception is specified.
    /// </param>
    public RiskLimitExceededException(string message, string limitType, decimal currentValue, decimal thresholdValue, Exception innerException)
        : base(message, DefaultErrorCode, innerException)
    {
        LimitType = limitType;
        CurrentValue = currentValue;
        ThresholdValue = thresholdValue;
    }
}
