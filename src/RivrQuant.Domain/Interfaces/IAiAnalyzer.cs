// Copyright (c) RivrQuant. All rights reserved.
// Licensed under the MIT License.

using RivrQuant.Domain.Models.Analysis;
using RivrQuant.Domain.Models.Backtests;

namespace RivrQuant.Domain.Interfaces;

/// <summary>
/// Provides AI-powered analysis capabilities for backtest results, combining
/// quantitative metrics with regime classifications to produce actionable insights.
/// </summary>
public interface IAiAnalyzer
{
    /// <summary>
    /// Analyzes a single backtest by correlating its result data, computed metrics,
    /// and detected market regimes to generate a comprehensive AI-driven report.
    /// </summary>
    /// <param name="backtest">The <see cref="BacktestResult"/> containing the raw backtest output.</param>
    /// <param name="metrics">
    /// The <see cref="BacktestMetrics"/> computed from the backtest's daily returns and trades.
    /// </param>
    /// <param name="regimes">
    /// A read-only list of <see cref="RegimeClassification"/> entries representing
    /// the detected market regimes during the backtest period.
    /// </param>
    /// <param name="ct">A cancellation token to cancel the asynchronous operation.</param>
    /// <returns>
    /// An <see cref="AiAnalysisReport"/> containing narrative insights, identified strengths
    /// and weaknesses, and actionable recommendations.
    /// </returns>
    Task<AiAnalysisReport> AnalyzeBacktestAsync(
        BacktestResult backtest,
        BacktestMetrics metrics,
        IReadOnlyList<RegimeClassification> regimes,
        CancellationToken ct);

    /// <summary>
    /// Performs a comparative analysis across multiple backtests, highlighting
    /// relative strengths, strategy divergence, and regime sensitivity differences.
    /// </summary>
    /// <param name="backtests">
    /// A read-only list of <see cref="BacktestResult"/> entries to compare.
    /// Must contain at least two items.
    /// </param>
    /// <param name="metrics">
    /// A read-only list of <see cref="BacktestMetrics"/> corresponding positionally
    /// to each entry in <paramref name="backtests"/>.
    /// </param>
    /// <param name="ct">A cancellation token to cancel the asynchronous operation.</param>
    /// <returns>
    /// An <see cref="AiAnalysisReport"/> summarizing the comparative findings,
    /// including ranking, trade-offs, and suggested next steps.
    /// </returns>
    Task<AiAnalysisReport> CompareBacktestsAsync(
        IReadOnlyList<BacktestResult> backtests,
        IReadOnlyList<BacktestMetrics> metrics,
        CancellationToken ct);
}
