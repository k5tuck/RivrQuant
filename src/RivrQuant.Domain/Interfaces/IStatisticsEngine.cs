// Copyright (c) RivrQuant. All rights reserved.
// Licensed under the MIT License.

using RivrQuant.Domain.Models.Analysis;
using RivrQuant.Domain.Models.Backtests;

namespace RivrQuant.Domain.Interfaces;

/// <summary>
/// Provides quantitative risk and performance calculations for strategy evaluation.
/// All ratio-based methods follow standard financial definitions and assume
/// daily-frequency input unless otherwise noted.
/// </summary>
public interface IStatisticsEngine
{
    /// <summary>
    /// Calculates the annualized Sharpe ratio, measuring risk-adjusted excess return
    /// per unit of total volatility.
    /// </summary>
    /// <param name="dailyReturns">A sequence of daily simple returns (e.g., 0.01 = 1%).</param>
    /// <param name="riskFreeRate">The annualized risk-free rate (e.g., 0.05 = 5%).</param>
    /// <returns>The annualized Sharpe ratio.</returns>
    double CalculateSharpeRatio(IReadOnlyList<double> dailyReturns, double riskFreeRate);

    /// <summary>
    /// Calculates the annualized Sortino ratio, measuring risk-adjusted excess return
    /// per unit of downside deviation only.
    /// </summary>
    /// <param name="dailyReturns">A sequence of daily simple returns.</param>
    /// <param name="riskFreeRate">The annualized risk-free rate.</param>
    /// <returns>The annualized Sortino ratio.</returns>
    double CalculateSortinoRatio(IReadOnlyList<double> dailyReturns, double riskFreeRate);

    /// <summary>
    /// Calculates the maximum peak-to-trough drawdown from an equity curve.
    /// </summary>
    /// <param name="equityCurve">
    /// A sequence of portfolio equity values ordered chronologically.
    /// </param>
    /// <returns>
    /// The maximum drawdown expressed as a positive decimal (e.g., 0.15 = 15% drawdown).
    /// </returns>
    double CalculateMaxDrawdown(IReadOnlyList<double> equityCurve);

    /// <summary>
    /// Calculates the parametric Value at Risk (VaR) at the specified confidence level
    /// using the variance-covariance method.
    /// </summary>
    /// <param name="dailyReturns">A sequence of daily simple returns.</param>
    /// <param name="confidence">
    /// The confidence level as a decimal (e.g., 0.95 for 95% confidence).
    /// </param>
    /// <returns>
    /// The VaR expressed as a positive loss amount relative to the portfolio value.
    /// </returns>
    double CalculateValueAtRisk(IReadOnlyList<double> dailyReturns, double confidence);

    /// <summary>
    /// Calculates the Expected Shortfall (Conditional VaR), representing the
    /// average loss in the tail beyond the VaR threshold.
    /// </summary>
    /// <param name="dailyReturns">A sequence of daily simple returns.</param>
    /// <param name="confidence">
    /// The confidence level as a decimal (e.g., 0.95 for 95% confidence).
    /// </param>
    /// <returns>
    /// The expected shortfall expressed as a positive loss amount.
    /// </returns>
    double CalculateExpectedShortfall(IReadOnlyList<double> dailyReturns, double confidence);

    /// <summary>
    /// Computes a rolling statistic over the provided daily returns using a sliding window.
    /// </summary>
    /// <param name="dailyReturns">A sequence of daily simple returns.</param>
    /// <param name="windowSize">The number of observations in each rolling window.</param>
    /// <param name="statisticFn">
    /// A function that computes a scalar statistic from a window of return values.
    /// </param>
    /// <returns>
    /// A read-only list of computed statistics, one per valid window position.
    /// The length is <c>dailyReturns.Count - windowSize + 1</c>.
    /// </returns>
    IReadOnlyList<double> CalculateRollingStatistic(
        IReadOnlyList<double> dailyReturns,
        int windowSize,
        Func<IReadOnlyList<double>, double> statisticFn);

    /// <summary>
    /// Estimates the probability of experiencing a drawdown exceeding the given
    /// threshold using Monte Carlo simulation with bootstrapped daily returns.
    /// </summary>
    /// <param name="dailyReturns">A sequence of historical daily simple returns to bootstrap from.</param>
    /// <param name="threshold">
    /// The drawdown threshold as a positive decimal (e.g., 0.20 = 20% drawdown).
    /// </param>
    /// <param name="simulations">The number of Monte Carlo simulation paths to generate.</param>
    /// <param name="horizon">The number of trading days in each simulated path.</param>
    /// <returns>
    /// The estimated probability (0.0 to 1.0) that at least one path breaches the threshold.
    /// </returns>
    double CalculateMonteCarloDrawdownProbability(
        IReadOnlyList<double> dailyReturns,
        double threshold,
        int simulations,
        int horizon);

    /// <summary>
    /// Calculates the Calmar ratio, defined as the annualized return divided by
    /// the maximum drawdown.
    /// </summary>
    /// <param name="annualizedReturn">The annualized return as a decimal.</param>
    /// <param name="maxDrawdown">
    /// The maximum drawdown as a positive decimal (e.g., 0.15 = 15%).
    /// </param>
    /// <returns>
    /// The Calmar ratio, or <see cref="double.PositiveInfinity"/> if
    /// <paramref name="maxDrawdown"/> is zero.
    /// </returns>
    double CalculateCalmarRatio(double annualizedReturn, double maxDrawdown);

    /// <summary>
    /// Calculates the profit factor, defined as the ratio of gross profits to
    /// gross losses.
    /// </summary>
    /// <param name="wins">A sequence of positive profit amounts from winning trades.</param>
    /// <param name="losses">
    /// A sequence of positive loss amounts from losing trades (expressed as absolute values).
    /// </param>
    /// <returns>
    /// The profit factor, or <see cref="double.PositiveInfinity"/> if there are no losses.
    /// </returns>
    double CalculateProfitFactor(IReadOnlyList<double> wins, IReadOnlyList<double> losses);

    /// <summary>
    /// Calculates the portfolio beta relative to a benchmark, measuring
    /// systematic risk exposure.
    /// </summary>
    /// <param name="strategyReturns">A sequence of daily strategy returns.</param>
    /// <param name="benchmarkReturns">
    /// A sequence of daily benchmark returns, aligned positionally with
    /// <paramref name="strategyReturns"/>.
    /// </param>
    /// <returns>The beta coefficient.</returns>
    double CalculateBeta(IReadOnlyList<double> strategyReturns, IReadOnlyList<double> benchmarkReturns);

    /// <summary>
    /// Computes a comprehensive set of performance and risk metrics from
    /// daily return data and a trade log.
    /// </summary>
    /// <param name="dailyReturns">
    /// A read-only list of <see cref="DailyReturn"/> records containing date and return value pairs.
    /// </param>
    /// <param name="trades">
    /// A read-only list of <see cref="BacktestTrade"/> records from the backtest execution.
    /// </param>
    /// <param name="riskFreeRate">The annualized risk-free rate used for ratio calculations.</param>
    /// <returns>
    /// A fully populated <see cref="BacktestMetrics"/> instance containing all computed statistics.
    /// </returns>
    BacktestMetrics CalculateFullMetrics(
        IReadOnlyList<DailyReturn> dailyReturns,
        IReadOnlyList<BacktestTrade> trades,
        double riskFreeRate);
}
