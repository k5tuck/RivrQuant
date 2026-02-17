namespace RivrQuant.Domain.Models.Analysis;

/// <summary>
/// Represents a comprehensive AI-generated analysis report for a backtest result,
/// providing insights on strategy quality, overfitting risk, regime behavior,
/// and deployment readiness.
/// </summary>
public class AiAnalysisReport
{
    /// <summary>
    /// Unique internal identifier for the analysis report.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Foreign key linking this report to the analyzed backtest result.
    /// </summary>
    public Guid BacktestResultId { get; init; }

    /// <summary>
    /// High-level narrative assessment of the strategy's overall performance and viability.
    /// </summary>
    public string OverallAssessment { get; init; } = string.Empty;

    /// <summary>
    /// List of identified strengths in the strategy's performance and design.
    /// </summary>
    public IReadOnlyList<string> Strengths { get; init; } = Array.Empty<string>();

    /// <summary>
    /// List of identified weaknesses or areas of concern in the strategy.
    /// </summary>
    public IReadOnlyList<string> Weaknesses { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Assessed level of overfitting risk: "low", "medium", or "high".
    /// </summary>
    public string OverfittingRisk { get; init; } = string.Empty;

    /// <summary>
    /// Detailed explanation of why the overfitting risk level was assigned.
    /// </summary>
    public string OverfittingExplanation { get; init; } = string.Empty;

    /// <summary>
    /// List of suggested parameter adjustments to improve robustness or performance.
    /// </summary>
    public IReadOnlyList<string> ParameterSuggestions { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Analysis of how the strategy performs across different market regimes
    /// (trending, mean-reverting, high-volatility, etc.).
    /// </summary>
    public string RegimeAnalysis { get; init; } = string.Empty;

    /// <summary>
    /// Deployment readiness score on a scale of 1 to 10, where 10 indicates
    /// highest confidence in live deployment suitability.
    /// </summary>
    public int DeploymentReadiness { get; init; }

    /// <summary>
    /// Plain-English summary of the analysis, suitable for non-technical stakeholders.
    /// </summary>
    public string PlainEnglishSummary { get; init; } = string.Empty;

    /// <summary>
    /// List of critical warnings that should be addressed before live deployment.
    /// </summary>
    public IReadOnlyList<string> CriticalWarnings { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Raw response text from the AI model, preserved for debugging and audit purposes.
    /// </summary>
    public string RawResponse { get; init; } = string.Empty;

    /// <summary>
    /// Timestamp when this analysis report was generated.
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Number of tokens consumed by the AI model to generate this analysis.
    /// </summary>
    public int TokensUsed { get; init; }

    /// <summary>
    /// Estimated cost in USD for generating this analysis.
    /// </summary>
    public decimal EstimatedCost { get; init; }
}
