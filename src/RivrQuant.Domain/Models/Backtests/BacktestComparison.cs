namespace RivrQuant.Domain.Models.Backtests;

/// <summary>
/// Represents a side-by-side comparison of multiple backtest results,
/// including their metrics and an optional AI-generated comparison summary.
/// </summary>
public class BacktestComparison
{
    /// <summary>
    /// Unique internal identifier for the comparison.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Collection of backtest results being compared.
    /// </summary>
    public IReadOnlyList<BacktestResult> Backtests { get; init; } = Array.Empty<BacktestResult>();

    /// <summary>
    /// Metrics for each backtest, aligned by index with <see cref="Backtests"/> for comparison.
    /// </summary>
    public IReadOnlyList<BacktestMetrics> MetricsComparison { get; init; } = Array.Empty<BacktestMetrics>();

    /// <summary>
    /// AI-generated narrative summary comparing the backtests, highlighting relative strengths
    /// and weaknesses. Null until AI analysis has been performed.
    /// </summary>
    public string? AiComparisonSummary { get; set; }

    /// <summary>
    /// Timestamp when this comparison was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}
