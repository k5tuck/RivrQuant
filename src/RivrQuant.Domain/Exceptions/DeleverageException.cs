using RivrQuant.Domain.Enums;

namespace RivrQuant.Domain.Exceptions;

/// <summary>
/// Exception thrown when a deleveraging action fails or encounters an error during execution.
/// This covers failures in position reduction, strategy pausing, and emergency liquidation procedures.
/// </summary>
public class DeleverageException : RivrQuantException
{
    private const string DefaultErrorCode = "RISK.DELEVERAGE";

    /// <summary>
    /// Gets the deleverage severity level at which the failure occurred.
    /// </summary>
    public DeleverageLevel Level { get; init; }

    /// <summary>
    /// Gets a description of the deleveraging action that was being attempted when the failure occurred.
    /// </summary>
    public string? ActionDescription { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleverageException"/> class
    /// with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the deleveraging failure.</param>
    public DeleverageException(string message)
        : base(message, DefaultErrorCode)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleverageException"/> class
    /// with a specified error message and a reference to the inner exception that
    /// is the cause of this exception.
    /// </summary>
    /// <param name="message">The message that describes the deleveraging failure.</param>
    /// <param name="innerException">
    /// The exception that is the cause of the current exception, or <c>null</c> if no
    /// inner exception is specified.
    /// </param>
    public DeleverageException(string message, Exception innerException)
        : base(message, DefaultErrorCode, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleverageException"/> class
    /// with a specified error message, deleverage level, and action description.
    /// </summary>
    /// <param name="message">The message that describes the deleveraging failure.</param>
    /// <param name="level">The deleverage severity level at which the failure occurred.</param>
    /// <param name="actionDescription">A description of the deleveraging action that was being attempted.</param>
    public DeleverageException(string message, DeleverageLevel level, string actionDescription)
        : base(message, DefaultErrorCode)
    {
        Level = level;
        ActionDescription = actionDescription;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleverageException"/> class
    /// with a specified error message, deleverage level, action description, and a reference
    /// to the inner exception.
    /// </summary>
    /// <param name="message">The message that describes the deleveraging failure.</param>
    /// <param name="level">The deleverage severity level at which the failure occurred.</param>
    /// <param name="actionDescription">A description of the deleveraging action that was being attempted.</param>
    /// <param name="innerException">
    /// The exception that is the cause of the current exception, or <c>null</c> if no
    /// inner exception is specified.
    /// </param>
    public DeleverageException(string message, DeleverageLevel level, string actionDescription, Exception innerException)
        : base(message, DefaultErrorCode, innerException)
    {
        Level = level;
        ActionDescription = actionDescription;
    }
}
