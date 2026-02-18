// Copyright (c) RivrQuant. All rights reserved.
// Licensed under the MIT License.

namespace RivrQuant.Application.DTOs;

/// <summary>
/// Lightweight summary projection of a backtest result, suitable for list views and dashboards.
/// Contains key performance indicators without the full trade or equity curve data.
/// </summary>
/// <param name="Id">Unique internal identifier for the backtest result.</param>
/// <param name="StrategyName">Name of the strategy that was backtested.</param>
/// <param name="DateRun">Timestamp when the backtest was executed.</param>
/// <param name="SharpeRatio">Annualized Sharpe ratio measuring risk-adjusted return.</param>
/// <param name="MaxDrawdown">Maximum peak-to-trough drawdown as a positive decimal.</param>
/// <param name="TotalReturn">Total return as a decimal (e.g., 0.15 for 15%).</param>
/// <param name="WinRate">Percentage of winning trades as a decimal (e.g., 0.55 for 55%).</param>
/// <param name="AiScore">AI-assessed deployment readiness score (1-10), null if not analyzed.</param>
/// <param name="IsAnalyzed">Whether AI analysis has been performed on this backtest.</param>
public sealed record BacktestSummaryDto(
    Guid Id,
    string StrategyName,
    DateTimeOffset DateRun,
    double SharpeRatio,
    double MaxDrawdown,
    decimal TotalReturn,
    double WinRate,
    int? AiScore,
    bool IsAnalyzed);

/// <summary>
/// Complete detail projection of a backtest result including metrics, trade history,
/// daily returns, regime classifications, walk-forward analysis, and AI report data.
/// </summary>
/// <param name="Id">Unique internal identifier for the backtest result.</param>
/// <param name="ExternalBacktestId">External identifier from the backtest provider.</param>
/// <param name="ProjectId">Project identifier from the backtest provider.</param>
/// <param name="StrategyName">Name of the strategy that was backtested.</param>
/// <param name="StrategyDescription">Optional description of the strategy configuration.</param>
/// <param name="StartDate">Start date of the backtest simulation period.</param>
/// <param name="EndDate">End date of the backtest simulation period.</param>
/// <param name="CreatedAt">Timestamp when the backtest result was persisted.</param>
/// <param name="InitialCapital">Initial capital allocated to the strategy.</param>
/// <param name="FinalEquity">Final portfolio equity at backtest completion.</param>
/// <param name="TotalReturn">Total return as a decimal.</param>
/// <param name="SharpeRatio">Annualized Sharpe ratio.</param>
/// <param name="SortinoRatio">Annualized Sortino ratio.</param>
/// <param name="MaxDrawdown">Maximum peak-to-trough drawdown.</param>
/// <param name="WinRate">Percentage of winning trades.</param>
/// <param name="ProfitFactor">Ratio of gross profits to gross losses.</param>
/// <param name="CalmarRatio">Annualized return divided by maximum drawdown.</param>
/// <param name="AnnualizedReturn">Annualized return extrapolated from the backtest period.</param>
/// <param name="AnnualizedVolatility">Annualized standard deviation of returns.</param>
/// <param name="TotalTrades">Total number of round-trip trades executed.</param>
/// <param name="WinningTrades">Number of profitable trades.</param>
/// <param name="LosingTrades">Number of unprofitable trades.</param>
/// <param name="AverageWin">Average profit per winning trade.</param>
/// <param name="AverageLoss">Average loss per losing trade.</param>
/// <param name="LargestWin">Largest single-trade profit.</param>
/// <param name="LargestLoss">Largest single-trade loss.</param>
/// <param name="AverageHoldingPeriod">Average duration positions were held.</param>
/// <param name="ValueAtRisk95">Value at Risk at the 95% confidence level.</param>
/// <param name="ExpectedShortfall95">Expected Shortfall at the 95% confidence level.</param>
/// <param name="Beta">Beta coefficient relative to the benchmark.</param>
/// <param name="IsAnalyzed">Whether AI analysis has been performed.</param>
/// <param name="Trades">Collection of individual trade records.</param>
/// <param name="AiReport">AI analysis report, null if not yet analyzed.</param>
public sealed record BacktestDetailDto(
    Guid Id,
    string ExternalBacktestId,
    string ProjectId,
    string StrategyName,
    string? StrategyDescription,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    DateTimeOffset CreatedAt,
    decimal InitialCapital,
    decimal FinalEquity,
    decimal TotalReturn,
    double SharpeRatio,
    double SortinoRatio,
    double MaxDrawdown,
    double WinRate,
    double ProfitFactor,
    double CalmarRatio,
    double AnnualizedReturn,
    double AnnualizedVolatility,
    int TotalTrades,
    int WinningTrades,
    int LosingTrades,
    decimal AverageWin,
    decimal AverageLoss,
    decimal LargestWin,
    decimal LargestLoss,
    TimeSpan AverageHoldingPeriod,
    double ValueAtRisk95,
    double ExpectedShortfall95,
    double Beta,
    bool IsAnalyzed,
    IReadOnlyList<BacktestTradeDto> Trades,
    AnalysisReportDto? AiReport);

/// <summary>
/// Projection of a single backtest trade for display in detail views.
/// </summary>
/// <param name="Id">Unique identifier for the trade.</param>
/// <param name="Symbol">Ticker symbol of the traded instrument.</param>
/// <param name="Side">Direction of the trade (Buy or Sell).</param>
/// <param name="EntryTime">Timestamp when the position was opened.</param>
/// <param name="ExitTime">Timestamp when the position was closed.</param>
/// <param name="EntryPrice">Price at which the position was entered.</param>
/// <param name="ExitPrice">Price at which the position was exited.</param>
/// <param name="Quantity">Number of shares or contracts traded.</param>
/// <param name="ProfitLoss">Absolute profit or loss for the trade.</param>
/// <param name="ProfitLossPercent">Profit or loss as a percentage of entry value.</param>
/// <param name="HoldingPeriod">Duration the position was held.</param>
/// <param name="IsWin">Whether the trade was profitable.</param>
public sealed record BacktestTradeDto(
    Guid Id,
    string Symbol,
    string Side,
    DateTimeOffset EntryTime,
    DateTimeOffset ExitTime,
    decimal EntryPrice,
    decimal ExitPrice,
    decimal Quantity,
    decimal ProfitLoss,
    decimal ProfitLossPercent,
    TimeSpan HoldingPeriod,
    bool IsWin);
