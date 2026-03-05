using RivrQuant.Domain.Enums;
using RivrQuant.Domain.Models.Backtests;
using RivrQuant.Domain.Models.Execution;

namespace RivrQuant.Domain.Interfaces;

/// <summary>
/// Provides pre-trade execution cost estimation and backtest cost adjustment capabilities.
/// Implementations model slippage, spread, and commission costs based on historical data,
/// market conditions, and broker-specific fee schedules.
/// </summary>
public interface IExecutionCostModel
{
    /// <summary>
    /// Estimates the total execution cost for a proposed trade, including slippage,
    /// spread, and commission components.
    /// </summary>
    /// <param name="symbol">Ticker symbol of the instrument to trade.</param>
    /// <param name="side">Direction of the proposed trade (Buy or Sell).</param>
    /// <param name="quantity">Number of shares or units to trade.</param>
    /// <param name="currentPrice">Current market price of the instrument.</param>
    /// <param name="broker">Broker through which the trade would be executed.</param>
    /// <param name="ct">A cancellation token to cancel the asynchronous operation.</param>
    /// <returns>An <see cref="ExecutionCostEstimate"/> containing the cost breakdown.</returns>
    Task<ExecutionCostEstimate> EstimateCostAsync(
        string symbol,
        OrderSide side,
        decimal quantity,
        decimal currentPrice,
        BrokerType broker,
        CancellationToken ct);

    /// <summary>
    /// Adjusts backtest metrics to account for realistic execution costs that may not
    /// have been modeled during the backtest simulation.
    /// </summary>
    /// <param name="backtest">The backtest result to adjust.</param>
    /// <param name="broker">The broker whose fee schedule and slippage model should be applied.</param>
    /// <param name="ct">A cancellation token to cancel the asynchronous operation.</param>
    /// <returns>
    /// An adjusted <see cref="BacktestMetrics"/> reflecting the impact of execution costs
    /// on strategy performance.
    /// </returns>
    Task<BacktestMetrics> AdjustBacktestForCostsAsync(
        BacktestResult backtest,
        BrokerType broker,
        CancellationToken ct);
}
