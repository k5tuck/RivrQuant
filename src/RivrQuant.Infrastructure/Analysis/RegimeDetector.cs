namespace RivrQuant.Infrastructure.Analysis;

using Microsoft.Extensions.Logging;
using RivrQuant.Domain.Enums;
using RivrQuant.Domain.Interfaces;
using RivrQuant.Domain.Models.Analysis;
using RivrQuant.Domain.Models.Backtests;

/// <summary>Classifies backtest periods into market regimes based on volatility and trend characteristics.</summary>
public sealed class RegimeDetector
{
    private const int ShortWindow = 20;
    private const int LongWindow = 50;
    private const int MinRegimeLength = 10;
    private readonly IStatisticsEngine _stats;
    private readonly ILogger<RegimeDetector> _logger;

    /// <summary>Initializes a new instance of <see cref="RegimeDetector"/>.</summary>
    public RegimeDetector(IStatisticsEngine stats, ILogger<RegimeDetector> logger)
    {
        _stats = stats;
        _logger = logger;
    }

    /// <summary>Detects market regimes across the backtest period and calculates per-regime statistics.</summary>
    public IReadOnlyList<RegimeClassification> DetectRegimes(IReadOnlyList<DailyReturn> dailyReturns, Guid backtestResultId)
    {
        if (dailyReturns.Count < LongWindow)
        {
            _logger.LogWarning("Insufficient data ({Count} days) for regime detection. Minimum {Required} required.", dailyReturns.Count, LongWindow);
            return Array.Empty<RegimeClassification>();
        }

        _logger.LogInformation("Detecting regimes for backtest {BacktestId} across {Days} trading days", backtestResultId, dailyReturns.Count);

        var returns = dailyReturns.Select(d => (double)d.DailyReturnPercent).ToArray();
        var equities = dailyReturns.Select(d => (double)d.Equity).ToArray();

        var rollingVol = CalculateRollingVolatility(returns, ShortWindow);
        var avgVol = rollingVol.Count > 0 ? rollingVol.Average() : 0;
        var shortSma = CalculateRollingSma(equities, ShortWindow);
        var longSma = CalculateRollingSma(equities, LongWindow);

        var dayRegimes = new RegimeType[dailyReturns.Count];
        for (var i = 0; i < dailyReturns.Count; i++)
        {
            dayRegimes[i] = ClassifyDay(i, rollingVol, avgVol, shortSma, longSma, returns);
        }

        var regimes = ConsolidateRegimes(dayRegimes, dailyReturns, backtestResultId);
        _logger.LogInformation("Detected {RegimeCount} regimes for backtest {BacktestId}", regimes.Count, backtestResultId);
        return regimes;
    }

    private RegimeType ClassifyDay(int index, IReadOnlyList<double> rollingVol, double avgVol, IReadOnlyList<double> shortSma, IReadOnlyList<double> longSma, double[] returns)
    {
        var vol = index < rollingVol.Count ? rollingVol[index] : avgVol;
        var highVolThreshold = avgVol * 2.0;
        var lowVolThreshold = avgVol * 0.5;

        if (vol > highVolThreshold)
        {
            var recentReturns = returns.Skip(Math.Max(0, index - 30)).Take(Math.Min(30, index + 1)).ToArray();
            var cumReturn = recentReturns.Length > 0 ? recentReturns.Sum() : 0;
            return cumReturn < -0.15 ? RegimeType.Crisis : RegimeType.HighVolatility;
        }

        if (vol < lowVolThreshold)
            return RegimeType.LowVolatility;

        if (index < longSma.Count && index < shortSma.Count && shortSma.Count > 0 && longSma.Count > 0)
        {
            var smaIdx = Math.Min(index, Math.Min(shortSma.Count - 1, longSma.Count - 1));
            var trend = shortSma[smaIdx] - longSma[smaIdx];
            var trendPct = longSma[smaIdx] != 0 ? Math.Abs(trend / longSma[smaIdx]) : 0;
            if (trendPct > 0.02)
                return RegimeType.Trending;
        }

        return RegimeType.MeanReverting;
    }

    private IReadOnlyList<RegimeClassification> ConsolidateRegimes(RegimeType[] dayRegimes, IReadOnlyList<DailyReturn> dailyReturns, Guid backtestResultId)
    {
        var classifications = new List<RegimeClassification>();
        if (dayRegimes.Length == 0) return classifications;

        var currentRegime = dayRegimes[0];
        var startIdx = 0;

        for (var i = 1; i <= dayRegimes.Length; i++)
        {
            if (i == dayRegimes.Length || (dayRegimes[i] != currentRegime && i - startIdx >= MinRegimeLength))
            {
                var endIdx = i - 1;
                var regimeReturns = dailyReturns.Skip(startIdx).Take(endIdx - startIdx + 1).ToList();
                var returns = regimeReturns.Select(d => (double)d.DailyReturnPercent).ToArray();
                var equities = regimeReturns.Select(d => (double)d.Equity).ToArray();

                classifications.Add(new RegimeClassification
                {
                    BacktestResultId = backtestResultId,
                    Regime = currentRegime,
                    StartDate = dailyReturns[startIdx].Date,
                    EndDate = dailyReturns[endIdx].Date,
                    DurationDays = endIdx - startIdx + 1,
                    AnnualizedReturn = returns.Length > 0 ? returns.Sum() : 0,
                    SharpeRatio = returns.Length > 1 ? _stats.CalculateSharpeRatio(returns, 0) : 0,
                    MaxDrawdown = equities.Length > 0 ? _stats.CalculateMaxDrawdown(equities) : 0,
                    Volatility = 0,
                    TradeCount = 0,
                    WinRate = 0
                });

                if (i < dayRegimes.Length)
                {
                    currentRegime = dayRegimes[i];
                    startIdx = i;
                }
            }
        }

        return classifications;
    }

    private static IReadOnlyList<double> CalculateRollingVolatility(double[] returns, int window)
    {
        if (returns.Length < window) return Array.Empty<double>();
        var results = new double[returns.Length];
        for (var i = 0; i < returns.Length; i++)
        {
            if (i < window - 1) { results[i] = 0; continue; }
            var slice = returns.AsSpan(i - window + 1, window);
            var mean = 0.0;
            foreach (var v in slice) mean += v;
            mean /= window;
            var sumSq = 0.0;
            foreach (var v in slice) sumSq += (v - mean) * (v - mean);
            results[i] = Math.Sqrt(sumSq / (window - 1));
        }
        return results;
    }

    private static IReadOnlyList<double> CalculateRollingSma(double[] values, int window)
    {
        if (values.Length < window) return Array.Empty<double>();
        var results = new double[values.Length];
        var sum = 0.0;
        for (var i = 0; i < values.Length; i++)
        {
            sum += values[i];
            if (i >= window) sum -= values[i - window];
            results[i] = i >= window - 1 ? sum / window : values[i];
        }
        return results;
    }
}
