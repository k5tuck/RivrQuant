namespace RivrQuant.Infrastructure.Analysis;

using Microsoft.Extensions.Logging;
using RivrQuant.Domain.Interfaces;
using RivrQuant.Domain.Models.Analysis;
using RivrQuant.Domain.Models.Backtests;

/// <summary>Performs walk-forward validation to assess strategy robustness.</summary>
public sealed class WalkForwardAnalyzer
{
    private readonly IStatisticsEngine _stats;
    private readonly ILogger<WalkForwardAnalyzer> _logger;

    /// <summary>Initializes a new instance of <see cref="WalkForwardAnalyzer"/>.</summary>
    public WalkForwardAnalyzer(IStatisticsEngine stats, ILogger<WalkForwardAnalyzer> logger)
    {
        _stats = stats;
        _logger = logger;
    }

    /// <summary>
    /// Runs walk-forward analysis by splitting daily returns into windows with in-sample and out-of-sample periods.
    /// </summary>
    /// <param name="dailyReturns">The full set of daily returns from the backtest.</param>
    /// <param name="numberOfWindows">Number of walk-forward windows to create.</param>
    /// <param name="inSampleRatio">Fraction of each window used for in-sample (e.g., 0.7 = 70%).</param>
    /// <param name="backtestResultId">The backtest result this analysis belongs to.</param>
    /// <returns>Walk-forward results for each window.</returns>
    public IReadOnlyList<WalkForwardResult> RunWalkForwardAnalysis(
        IReadOnlyList<DailyReturn> dailyReturns,
        int numberOfWindows,
        double inSampleRatio,
        Guid backtestResultId)
    {
        if (dailyReturns.Count < numberOfWindows * 20)
        {
            _logger.LogWarning(
                "Insufficient data ({Count} days) for {Windows} walk-forward windows. Need at least {Required}.",
                dailyReturns.Count, numberOfWindows, numberOfWindows * 20);
            return Array.Empty<WalkForwardResult>();
        }

        _logger.LogInformation(
            "Running walk-forward analysis: {Windows} windows, {InSample}% in-sample, {DataPoints} data points",
            numberOfWindows, inSampleRatio * 100, dailyReturns.Count);

        var windowSize = dailyReturns.Count / numberOfWindows;
        var results = new List<WalkForwardResult>();

        for (var w = 0; w < numberOfWindows; w++)
        {
            var windowStart = w * windowSize;
            var windowEnd = (w == numberOfWindows - 1) ? dailyReturns.Count : (w + 1) * windowSize;
            var windowData = dailyReturns.Skip(windowStart).Take(windowEnd - windowStart).ToList();

            var inSampleCount = (int)(windowData.Count * inSampleRatio);
            var inSample = windowData.Take(inSampleCount).ToList();
            var outOfSample = windowData.Skip(inSampleCount).ToList();

            if (inSample.Count < 10 || outOfSample.Count < 5)
            {
                _logger.LogDebug("Skipping window {Window} due to insufficient data", w);
                continue;
            }

            var inSampleReturns = inSample.Select(d => (double)d.DailyReturnPercent).ToArray();
            var outOfSampleReturns = outOfSample.Select(d => (double)d.DailyReturnPercent).ToArray();

            var result = new WalkForwardResult
            {
                BacktestResultId = backtestResultId,
                WindowIndex = w,
                InSampleStart = inSample.First().Date,
                InSampleEnd = inSample.Last().Date,
                OutOfSampleStart = outOfSample.First().Date,
                OutOfSampleEnd = outOfSample.Last().Date,
                InSampleSharpe = _stats.CalculateSharpeRatio(inSampleReturns, 0),
                OutOfSampleSharpe = _stats.CalculateSharpeRatio(outOfSampleReturns, 0),
                InSampleReturn = inSampleReturns.Sum(),
                OutOfSampleReturn = outOfSampleReturns.Sum()
            };

            results.Add(result);
            _logger.LogDebug(
                "Window {Window}: IS Sharpe={InSampleSharpe:F3}, OOS Sharpe={OutSampleSharpe:F3}, Efficiency={Efficiency:F1}%",
                w + 1, result.InSampleSharpe, result.OutOfSampleSharpe, result.Efficiency * 100);
        }

        _logger.LogInformation("Walk-forward analysis complete. {WindowCount} windows analyzed.", results.Count);
        return results;
    }
}
