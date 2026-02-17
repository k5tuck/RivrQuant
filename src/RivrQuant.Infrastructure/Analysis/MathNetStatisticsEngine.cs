namespace RivrQuant.Infrastructure.Analysis;

using MathNet.Numerics.Statistics;
using RivrQuant.Domain.Interfaces;
using RivrQuant.Domain.Models.Backtests;

/// <summary>Statistics engine implementation using Math.NET Numerics library.</summary>
public sealed class MathNetStatisticsEngine : IStatisticsEngine
{
    private const int TradingDaysPerYear = 252;

    /// <inheritdoc />
    public double CalculateSharpeRatio(IReadOnlyList<double> dailyReturns, double riskFreeRate)
    {
        if (dailyReturns.Count < 2) return 0;
        var dailyRf = riskFreeRate / TradingDaysPerYear;
        var excessReturns = dailyReturns.Select(r => r - dailyRf).ToArray();
        var mean = excessReturns.Mean();
        var stdev = excessReturns.StandardDeviation();
        return stdev == 0 ? 0 : (mean / stdev) * Math.Sqrt(TradingDaysPerYear);
    }

    /// <inheritdoc />
    public double CalculateSortinoRatio(IReadOnlyList<double> dailyReturns, double riskFreeRate)
    {
        if (dailyReturns.Count < 2) return 0;
        var dailyRf = riskFreeRate / TradingDaysPerYear;
        var excessReturns = dailyReturns.Select(r => r - dailyRf).ToArray();
        var mean = excessReturns.Mean();
        var negativeReturns = excessReturns.Where(r => r < 0).ToArray();
        if (negativeReturns.Length == 0) return mean > 0 ? double.MaxValue : 0;
        var downsideDeviation = Math.Sqrt(negativeReturns.Select(r => r * r).Mean());
        return downsideDeviation == 0 ? 0 : (mean / downsideDeviation) * Math.Sqrt(TradingDaysPerYear);
    }

    /// <inheritdoc />
    public double CalculateMaxDrawdown(IReadOnlyList<double> equityCurve)
    {
        if (equityCurve.Count == 0) return 0;
        var maxDrawdown = 0.0;
        var peak = equityCurve[0];
        foreach (var value in equityCurve)
        {
            if (value > peak) peak = value;
            var drawdown = peak > 0 ? (peak - value) / peak : 0;
            if (drawdown > maxDrawdown) maxDrawdown = drawdown;
        }
        return maxDrawdown;
    }

    /// <inheritdoc />
    public double CalculateValueAtRisk(IReadOnlyList<double> dailyReturns, double confidence)
    {
        if (dailyReturns.Count == 0) return 0;
        var sorted = dailyReturns.OrderBy(r => r).ToArray();
        var index = (int)Math.Floor((1 - confidence) * sorted.Length);
        index = Math.Max(0, Math.Min(index, sorted.Length - 1));
        return sorted[index];
    }

    /// <inheritdoc />
    public double CalculateExpectedShortfall(IReadOnlyList<double> dailyReturns, double confidence)
    {
        if (dailyReturns.Count == 0) return 0;
        var var = CalculateValueAtRisk(dailyReturns, confidence);
        var tailReturns = dailyReturns.Where(r => r <= var).ToArray();
        return tailReturns.Length > 0 ? tailReturns.Mean() : var;
    }

    /// <inheritdoc />
    public IReadOnlyList<double> CalculateRollingStatistic(IReadOnlyList<double> dailyReturns, int windowSize, Func<IReadOnlyList<double>, double> statisticFn)
    {
        if (dailyReturns.Count < windowSize) return Array.Empty<double>();
        var results = new List<double>(dailyReturns.Count - windowSize + 1);
        for (var i = 0; i <= dailyReturns.Count - windowSize; i++)
        {
            var window = dailyReturns.Skip(i).Take(windowSize).ToList();
            results.Add(statisticFn(window));
        }
        return results;
    }

    /// <inheritdoc />
    public double CalculateMonteCarloDrawdownProbability(IReadOnlyList<double> dailyReturns, double threshold, int simulations, int horizon)
    {
        if (dailyReturns.Count == 0 || simulations <= 0) return 0;
        var random = new Random(42);
        var exceedanceCount = 0;
        var returnsArray = dailyReturns.ToArray();

        for (var sim = 0; sim < simulations; sim++)
        {
            var equity = 1.0;
            var peak = 1.0;
            var maxDd = 0.0;

            for (var day = 0; day < horizon; day++)
            {
                var idx = random.Next(returnsArray.Length);
                equity *= (1 + returnsArray[idx]);
                if (equity > peak) peak = equity;
                var dd = peak > 0 ? (peak - equity) / peak : 0;
                if (dd > maxDd) maxDd = dd;
            }

            if (maxDd >= threshold) exceedanceCount++;
        }

        return (double)exceedanceCount / simulations;
    }

    /// <inheritdoc />
    public double CalculateCalmarRatio(double annualizedReturn, double maxDrawdown)
    {
        return Math.Abs(maxDrawdown) < 1e-10 ? 0 : annualizedReturn / Math.Abs(maxDrawdown);
    }

    /// <inheritdoc />
    public double CalculateProfitFactor(IReadOnlyList<double> wins, IReadOnlyList<double> losses)
    {
        var totalWins = wins.Sum();
        var totalLosses = Math.Abs(losses.Sum());
        return totalLosses < 1e-10 ? (totalWins > 0 ? double.MaxValue : 0) : totalWins / totalLosses;
    }

    /// <inheritdoc />
    public double CalculateBeta(IReadOnlyList<double> strategyReturns, IReadOnlyList<double> benchmarkReturns)
    {
        var count = Math.Min(strategyReturns.Count, benchmarkReturns.Count);
        if (count < 2) return 0;
        var stratSlice = strategyReturns.Take(count).ToArray();
        var benchSlice = benchmarkReturns.Take(count).ToArray();
        var benchVariance = benchSlice.Variance();
        if (benchVariance < 1e-10) return 0;
        var covariance = Covariance(stratSlice, benchSlice);
        return covariance / benchVariance;
    }

    /// <inheritdoc />
    public BacktestMetrics CalculateFullMetrics(IReadOnlyList<DailyReturn> dailyReturns, IReadOnlyList<BacktestTrade> trades, double riskFreeRate)
    {
        var returns = dailyReturns.Select(d => (double)d.DailyReturnPercent).ToArray();
        var equityCurve = dailyReturns.Select(d => (double)d.Equity).ToArray();
        var wins = trades.Where(t => t.IsWin).Select(t => (double)t.ProfitLoss).ToArray();
        var losses = trades.Where(t => !t.IsWin).Select(t => (double)t.ProfitLoss).ToArray();

        var annualizedReturn = returns.Length > 0 ? returns.Mean() * TradingDaysPerYear : 0;
        var annualizedVol = returns.Length > 1 ? returns.StandardDeviation() * Math.Sqrt(TradingDaysPerYear) : 0;
        var maxDd = CalculateMaxDrawdown(equityCurve);

        return new BacktestMetrics
        {
            SharpeRatio = CalculateSharpeRatio(returns, riskFreeRate),
            SortinoRatio = CalculateSortinoRatio(returns, riskFreeRate),
            MaxDrawdown = maxDd,
            WinRate = trades.Count > 0 ? (double)trades.Count(t => t.IsWin) / trades.Count : 0,
            ProfitFactor = CalculateProfitFactor(wins, losses),
            CalmarRatio = CalculateCalmarRatio(annualizedReturn, maxDd),
            ValueAtRisk95 = CalculateValueAtRisk(returns, 0.95),
            ExpectedShortfall95 = CalculateExpectedShortfall(returns, 0.95),
            Beta = 0,
            AnnualizedReturn = annualizedReturn,
            AnnualizedVolatility = annualizedVol,
            TotalTrades = trades.Count,
            WinningTrades = trades.Count(t => t.IsWin),
            LosingTrades = trades.Count(t => !t.IsWin),
            AverageWin = wins.Length > 0 ? (decimal)wins.Average() : 0,
            AverageLoss = losses.Length > 0 ? (decimal)losses.Average() : 0,
            LargestWin = wins.Length > 0 ? (decimal)wins.Max() : 0,
            LargestLoss = losses.Length > 0 ? (decimal)losses.Min() : 0,
            AverageHoldingPeriod = trades.Count > 0
                ? TimeSpan.FromTicks((long)trades.Average(t => t.HoldingPeriod.Ticks))
                : TimeSpan.Zero
        };
    }

    private static double Covariance(double[] x, double[] y)
    {
        var meanX = x.Mean();
        var meanY = y.Mean();
        var sum = 0.0;
        for (var i = 0; i < x.Length; i++)
        {
            sum += (x[i] - meanX) * (y[i] - meanY);
        }
        return sum / (x.Length - 1);
    }
}
