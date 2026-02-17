namespace RivrQuant.Domain.Exceptions;

/// <summary>
/// Exception thrown when AI-powered analysis of backtest results fails.
/// This covers Claude API communication errors, prompt processing failures,
/// response parsing errors, and other issues during AI-driven backtest analysis.
/// </summary>
public class AiAnalysisException : RivrQuantException
{
    private const string DefaultErrorCode = "AI.ANALYSIS";

    /// <summary>
    /// Gets the identifier of the backtest being analyzed when the failure occurred.
    /// </summary>
    public string? BacktestId { get; init; }

    /// <summary>
    /// Gets the name of the analysis step that failed.
    /// </summary>
    /// <remarks>
    /// Common analysis steps include "PromptConstruction", "ApiCall", "ResponseParsing",
    /// "SummaryGeneration", and "RecommendationExtraction".
    /// </remarks>
    public string? AnalysisStep { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AiAnalysisException"/> class
    /// with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the analysis failure.</param>
    public AiAnalysisException(string message)
        : base(message, DefaultErrorCode)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AiAnalysisException"/> class
    /// with a specified error message and a reference to the inner exception that
    /// is the cause of this exception.
    /// </summary>
    /// <param name="message">The message that describes the analysis failure.</param>
    /// <param name="innerException">
    /// The exception that is the cause of the current exception, or <c>null</c> if no
    /// inner exception is specified.
    /// </param>
    public AiAnalysisException(string message, Exception innerException)
        : base(message, DefaultErrorCode, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AiAnalysisException"/> class
    /// with a specified error message, backtest identifier, and analysis step.
    /// </summary>
    /// <param name="message">The message that describes the analysis failure.</param>
    /// <param name="backtestId">The identifier of the backtest being analyzed.</param>
    /// <param name="analysisStep">The name of the analysis step that failed.</param>
    public AiAnalysisException(string message, string backtestId, string analysisStep)
        : base(message, DefaultErrorCode)
    {
        BacktestId = backtestId;
        AnalysisStep = analysisStep;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AiAnalysisException"/> class
    /// with a specified error message, backtest identifier, analysis step, and a
    /// reference to the inner exception.
    /// </summary>
    /// <param name="message">The message that describes the analysis failure.</param>
    /// <param name="backtestId">The identifier of the backtest being analyzed.</param>
    /// <param name="analysisStep">The name of the analysis step that failed.</param>
    /// <param name="innerException">
    /// The exception that is the cause of the current exception, or <c>null</c> if no
    /// inner exception is specified.
    /// </param>
    public AiAnalysisException(string message, string backtestId, string analysisStep, Exception innerException)
        : base(message, DefaultErrorCode, innerException)
    {
        BacktestId = backtestId;
        AnalysisStep = analysisStep;
    }
}
