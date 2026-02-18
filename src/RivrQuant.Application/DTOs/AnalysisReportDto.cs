namespace RivrQuant.Application.DTOs;

/// <summary>
/// Data transfer object representing a completed AI analysis report.
/// </summary>
public sealed record AnalysisReportDto
{
    /// <summary>Report identifier.</summary>
    public Guid Id { get; init; }

    /// <summary>Identifier of the analyzed backtest.</summary>
    public Guid BacktestResultId { get; init; }

    /// <summary>Strategy name of the analyzed backtest.</summary>
    public string StrategyName { get; init; } = string.Empty;

    /// <summary>High-level narrative assessment.</summary>
    public string OverallAssessment { get; init; } = string.Empty;

    /// <summary>Identified strengths.</summary>
    public IReadOnlyList<string> Strengths { get; init; } = Array.Empty<string>();

    /// <summary>Identified weaknesses.</summary>
    public IReadOnlyList<string> Weaknesses { get; init; } = Array.Empty<string>();

    /// <summary>Overfitting risk level.</summary>
    public string OverfittingRisk { get; init; } = string.Empty;

    /// <summary>Overfitting explanation.</summary>
    public string OverfittingExplanation { get; init; } = string.Empty;

    /// <summary>Parameter adjustment suggestions.</summary>
    public IReadOnlyList<string> ParameterSuggestions { get; init; } = Array.Empty<string>();

    /// <summary>Regime analysis narrative.</summary>
    public string RegimeAnalysis { get; init; } = string.Empty;

    /// <summary>Deployment readiness score (1-10).</summary>
    public int DeploymentReadiness { get; init; }

    /// <summary>Plain-English summary.</summary>
    public string PlainEnglishSummary { get; init; } = string.Empty;

    /// <summary>Critical warnings.</summary>
    public IReadOnlyList<string> CriticalWarnings { get; init; } = Array.Empty<string>();

    /// <summary>Tokens consumed for this analysis.</summary>
    public int TokensUsed { get; init; }

    /// <summary>Estimated cost in USD.</summary>
    public decimal EstimatedCost { get; init; }

    /// <summary>When this report was generated.</summary>
    public DateTimeOffset CreatedAt { get; init; }
}
