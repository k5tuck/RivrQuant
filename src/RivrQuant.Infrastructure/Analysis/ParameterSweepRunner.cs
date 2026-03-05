namespace RivrQuant.Infrastructure.Analysis;

using Microsoft.Extensions.Logging;
using MathNet.Numerics.Statistics;
using RivrQuant.Domain.Models.Analysis;
using RivrQuant.Domain.Models.Backtests;

/// <summary>Runs parameter sensitivity analysis across multiple backtest results.</summary>
public sealed class ParameterSweepRunner
{
    private readonly ILogger<ParameterSweepRunner> _logger;

    /// <summary>Initializes a new instance of <see cref="ParameterSweepRunner"/>.</summary>
    public ParameterSweepRunner(ILogger<ParameterSweepRunner> logger)
    {
        _logger = logger;
    }

    /// <summary>Analyzes how a specific parameter correlates with key performance metrics across backtest runs.</summary>
    public Task<ParameterSensitivity> RunParameterSensitivityAsync(
        IReadOnlyList<BacktestResult> backtestResults,
        string parameterName,
        CancellationToken ct)
    {
        _logger.LogInformation("Running parameter sensitivity for '{Parameter}' across {Count} backtests", parameterName, backtestResults.Count);

        var points = new List<ParameterPoint>();
        var paramValues = new List<double>();
        var sharpeValues = new List<double>();
        var returnValues = new List<double>();

        foreach (var bt in backtestResults)
        {
            ct.ThrowIfCancellationRequested();
            if (bt.Metrics is null) continue;

            var paramValue = ExtractParameterValue(bt, parameterName);
            if (paramValue is null) continue;

            var point = new ParameterPoint
            {
                ParameterValue = paramValue.Value,
                SharpeRatio = bt.Metrics.SharpeRatio,
                TotalReturn = (double)bt.TotalReturn,
                MaxDrawdown = bt.Metrics.MaxDrawdown
            };

            points.Add(point);
            paramValues.Add((double)paramValue.Value);
            sharpeValues.Add(bt.Metrics.SharpeRatio);
            returnValues.Add((double)bt.TotalReturn);
        }

        double corrSharpe = 0, corrReturn = 0, optimal = 0;
        if (paramValues.Count >= 3)
        {
            corrSharpe = Correlation.Pearson(paramValues, sharpeValues);
            corrReturn = Correlation.Pearson(paramValues, returnValues);
            if (double.IsNaN(corrSharpe)) corrSharpe = 0;
            if (double.IsNaN(corrReturn)) corrReturn = 0;

            var bestPoint = points.OrderByDescending(p => p.SharpeRatio).First();
            optimal = (double)bestPoint.ParameterValue;
        }

        var result = new ParameterSensitivity
        {
            ParameterName = parameterName,
            Points = points,
            CorrelationWithSharpe = corrSharpe,
            CorrelationWithReturn = corrReturn,
            OptimalValue = optimal
        };

        _logger.LogInformation(
            "Parameter sensitivity complete for '{Parameter}': Corr(Sharpe)={CorrSharpe:F3}, Corr(Return)={CorrReturn:F3}, Optimal={Optimal}",
            parameterName, corrSharpe, corrReturn, optimal);

        return Task.FromResult(result);
    }

    private static decimal? ExtractParameterValue(BacktestResult backtest, string parameterName)
    {
        // Try to extract numeric value after the parameter name in the strategy name
        // Supports patterns: ParamName_123, ParamName=123, ParamName123, ParamName-123
        var pattern = $@"(?i){System.Text.RegularExpressions.Regex.Escape(parameterName)}[\s_=\-]?(\d+\.?\d*)";
        var match = System.Text.RegularExpressions.Regex.Match(
            backtest.StrategyName ?? string.Empty, pattern);

        if (match.Success && decimal.TryParse(match.Groups[1].Value,
            System.Globalization.NumberStyles.Number,
            System.Globalization.CultureInfo.InvariantCulture, out var value))
        {
            return value;
        }

        return null;
    }
}
