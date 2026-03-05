namespace RivrQuant.Domain.Exceptions;

/// <summary>
/// Exception thrown when a portfolio exposure limit is breached.
/// This covers net exposure, gross exposure, sector concentration, beta limits,
/// and other exposure constraints configured for the portfolio.
/// </summary>
public class ExposureLimitException : RivrQuantException
{
    private const string DefaultErrorCode = "RISK.EXPOSURE_LIMIT";

    /// <summary>
    /// Gets the type of exposure limit that was breached (e.g., "NetExposure", "GrossExposure",
    /// "SectorConcentration", "BetaExposure").
    /// </summary>
    public string LimitType { get; init; } = string.Empty;

    /// <summary>
    /// Gets the current exposure value that breached the limit.
    /// </summary>
    public decimal CurrentValue { get; init; }

    /// <summary>
    /// Gets the configured threshold value that was exceeded.
    /// </summary>
    public decimal ThresholdValue { get; init; }

    /// <summary>
    /// Gets the amount by which the current value exceeds the threshold.
    /// </summary>
    public decimal ExcessAmount => CurrentValue - ThresholdValue;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExposureLimitException"/> class
    /// with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the exposure limit breach.</param>
    public ExposureLimitException(string message)
        : base(message, DefaultErrorCode)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExposureLimitException"/> class
    /// with a specified error message and a reference to the inner exception that
    /// is the cause of this exception.
    /// </summary>
    /// <param name="message">The message that describes the exposure limit breach.</param>
    /// <param name="innerException">
    /// The exception that is the cause of the current exception, or <c>null</c> if no
    /// inner exception is specified.
    /// </param>
    public ExposureLimitException(string message, Exception innerException)
        : base(message, DefaultErrorCode, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExposureLimitException"/> class
    /// with a specified error message, limit type, current value, and threshold value.
    /// </summary>
    /// <param name="message">The message that describes the exposure limit breach.</param>
    /// <param name="limitType">The type of exposure limit that was breached.</param>
    /// <param name="currentValue">The current exposure value that breached the limit.</param>
    /// <param name="thresholdValue">The configured threshold value that was exceeded.</param>
    public ExposureLimitException(string message, string limitType, decimal currentValue, decimal thresholdValue)
        : base(message, DefaultErrorCode)
    {
        LimitType = limitType;
        CurrentValue = currentValue;
        ThresholdValue = thresholdValue;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExposureLimitException"/> class
    /// with a specified error message, limit type, current value, threshold value, and a
    /// reference to the inner exception.
    /// </summary>
    /// <param name="message">The message that describes the exposure limit breach.</param>
    /// <param name="limitType">The type of exposure limit that was breached.</param>
    /// <param name="currentValue">The current exposure value that breached the limit.</param>
    /// <param name="thresholdValue">The configured threshold value that was exceeded.</param>
    /// <param name="innerException">
    /// The exception that is the cause of the current exception, or <c>null</c> if no
    /// inner exception is specified.
    /// </param>
    public ExposureLimitException(string message, string limitType, decimal currentValue, decimal thresholdValue, Exception innerException)
        : base(message, DefaultErrorCode, innerException)
    {
        LimitType = limitType;
        CurrentValue = currentValue;
        ThresholdValue = thresholdValue;
    }
}
