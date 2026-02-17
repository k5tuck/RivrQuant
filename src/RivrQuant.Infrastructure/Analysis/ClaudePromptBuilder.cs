namespace RivrQuant.Infrastructure.Analysis;

using System.Text;
using RivrQuant.Domain.Models.Analysis;
using RivrQuant.Domain.Models.Backtests;

/// <summary>Builds structured prompts for Claude AI backtest analysis.</summary>
public sealed class ClaudePromptBuilder
{
    /// <summary>Builds the system prompt establishing Claude as a quantitative analyst.</summary>
    public string BuildSystemPrompt()
    {
        return """
            You are a senior quantitative analyst with 15+ years of experience in systematic trading strategy evaluation.
            Your role is to analyze backtest results and provide actionable, honest assessments.

            You must respond with ONLY valid JSON (no markdown, no code fences) using this exact structure:
            {
                "overallAssessment": "A 2-3 sentence overall assessment of the strategy",
                "strengths": ["strength 1", "strength 2", ...],
                "weaknesses": ["weakness 1", "weakness 2", ...],
                "overfittingRisk": "low|medium|high",
                "overfittingExplanation": "Detailed explanation of overfitting risk assessment",
                "parameterSuggestions": ["suggestion 1", "suggestion 2", ...],
                "regimeAnalysis": "Analysis of how the strategy performs across different market regimes",
                "deploymentReadiness": 7,
                "plainEnglishSummary": "A plain-English summary suitable for a non-technical stakeholder",
                "criticalWarnings": ["warning 1", ...]
            }

            Be rigorous and skeptical. Flag potential issues clearly. A deployment readiness score of 7+ means
            the strategy shows genuine edge with manageable risks. Below 5 means significant concerns exist.
            """;
    }

    /// <summary>Builds the user prompt containing backtest data for analysis.</summary>
    public string BuildAnalysisPrompt(BacktestResult backtest, BacktestMetrics metrics, IReadOnlyList<RegimeClassification> regimes)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"## Strategy: {backtest.StrategyName}");
        if (!string.IsNullOrWhiteSpace(backtest.StrategyDescription))
            sb.AppendLine($"Description: {backtest.StrategyDescription}");
        sb.AppendLine($"Period: {backtest.StartDate:yyyy-MM-dd} to {backtest.EndDate:yyyy-MM-dd}");
        sb.AppendLine($"Initial Capital: ${backtest.InitialCapital:N2}");
        sb.AppendLine($"Final Equity: ${backtest.FinalEquity:N2}");
        sb.AppendLine();

        sb.AppendLine("## Performance Metrics");
        sb.AppendLine($"| Metric | Value |");
        sb.AppendLine($"|--------|-------|");
        sb.AppendLine($"| Total Return | {metrics.AnnualizedReturn * 100:F2}% (annualized) |");
        sb.AppendLine($"| Sharpe Ratio | {metrics.SharpeRatio:F3} |");
        sb.AppendLine($"| Sortino Ratio | {metrics.SortinoRatio:F3} |");
        sb.AppendLine($"| Max Drawdown | {metrics.MaxDrawdown * 100:F2}% |");
        sb.AppendLine($"| Calmar Ratio | {metrics.CalmarRatio:F3} |");
        sb.AppendLine($"| Profit Factor | {metrics.ProfitFactor:F3} |");
        sb.AppendLine($"| Win Rate | {metrics.WinRate * 100:F1}% |");
        sb.AppendLine($"| Value at Risk (95%) | {metrics.ValueAtRisk95 * 100:F2}% |");
        sb.AppendLine($"| Expected Shortfall (95%) | {metrics.ExpectedShortfall95 * 100:F2}% |");
        sb.AppendLine($"| Beta | {metrics.Beta:F3} |");
        sb.AppendLine($"| Annualized Volatility | {metrics.AnnualizedVolatility * 100:F2}% |");
        sb.AppendLine();

        sb.AppendLine("## Trade Statistics");
        sb.AppendLine($"| Metric | Value |");
        sb.AppendLine($"|--------|-------|");
        sb.AppendLine($"| Total Trades | {metrics.TotalTrades} |");
        sb.AppendLine($"| Winning Trades | {metrics.WinningTrades} |");
        sb.AppendLine($"| Losing Trades | {metrics.LosingTrades} |");
        sb.AppendLine($"| Average Win | ${metrics.AverageWin:N2} |");
        sb.AppendLine($"| Average Loss | ${metrics.AverageLoss:N2} |");
        sb.AppendLine($"| Largest Win | ${metrics.LargestWin:N2} |");
        sb.AppendLine($"| Largest Loss | ${metrics.LargestLoss:N2} |");
        sb.AppendLine($"| Avg Holding Period | {metrics.AverageHoldingPeriod.TotalHours:F1} hours |");
        sb.AppendLine();

        if (regimes.Count > 0)
        {
            sb.AppendLine("## Per-Regime Performance");
            sb.AppendLine("| Regime | Period | Return | Sharpe | Max DD | Trades | Win Rate |");
            sb.AppendLine("|--------|--------|--------|--------|--------|--------|----------|");
            foreach (var regime in regimes)
            {
                sb.AppendLine($"| {regime.Regime} | {regime.StartDate:yyyy-MM-dd} to {regime.EndDate:yyyy-MM-dd} | {regime.ReturnInRegime * 100:F2}% | {regime.SharpeInRegime:F3} | {regime.MaxDrawdownInRegime * 100:F2}% | {regime.TradeCountInRegime} | {regime.WinRateInRegime * 100:F1}% |");
            }
            sb.AppendLine();
        }

        sb.AppendLine("Please analyze this backtest result and provide your assessment in the JSON format specified.");
        return sb.ToString();
    }

    /// <summary>Builds a comparative analysis prompt for multiple backtests.</summary>
    public string BuildComparisonPrompt(IReadOnlyList<BacktestResult> backtests, IReadOnlyList<BacktestMetrics> metrics)
    {
        var sb = new StringBuilder();
        sb.AppendLine("## Backtest Comparison");
        sb.AppendLine("Please compare the following backtests and provide a comparative analysis.");
        sb.AppendLine();

        for (var i = 0; i < backtests.Count && i < metrics.Count; i++)
        {
            var bt = backtests[i];
            var m = metrics[i];
            sb.AppendLine($"### Backtest {i + 1}: {bt.StrategyName}");
            sb.AppendLine($"Period: {bt.StartDate:yyyy-MM-dd} to {bt.EndDate:yyyy-MM-dd}");
            sb.AppendLine($"Return: {bt.TotalReturn * 100:F2}% | Sharpe: {m.SharpeRatio:F3} | Max DD: {m.MaxDrawdown * 100:F2}% | Win Rate: {m.WinRate * 100:F1}% | Trades: {m.TotalTrades}");
            sb.AppendLine();
        }

        sb.AppendLine("Provide the comparison in JSON format with the same structure, focusing on relative strengths and which strategy is most suitable for deployment.");
        return sb.ToString();
    }
}
