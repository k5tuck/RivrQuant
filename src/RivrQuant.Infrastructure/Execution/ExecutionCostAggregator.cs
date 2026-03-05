using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RivrQuant.Domain.Enums;
using RivrQuant.Domain.Interfaces;
using RivrQuant.Domain.Models.Backtests;
using RivrQuant.Domain.Models.Execution;
using RivrQuant.Infrastructure.Persistence;

namespace RivrQuant.Infrastructure.Execution;

/// <summary>
/// Aggregates slippage, spread, and commission estimates into a single
/// <see cref="ExecutionCostEstimate"/>. Implements <see cref="IExecutionCostModel"/>
/// and provides backtest cost adjustment capability.
/// </summary>
/// <remarks>
/// This aggregator combines three independent cost components:
/// <list type="bullet">
///   <item><see cref="SimpleSlippageModel"/> — volume-aware slippage in bps</item>
///   <item><see cref="SpreadEstimator"/> — half-spread cost in bps</item>
///   <item><see cref="CommissionCalculator"/> — broker-specific commissions and regulatory fees</item>
/// </list>
/// The total cost is expressed both in basis points and in the account's base currency.
/// </remarks>
public sealed class ExecutionCostAggregator : IExecutionCostModel
{
    private readonly SimpleSlippageModel _slippageModel;
    private readonly SpreadEstimator _spreadEstimator;
    private readonly CommissionCalculator _commissionCalculator;
    private readonly RivrQuantDbContext _db;
    private readonly ILogger<ExecutionCostAggregator> _logger;

    private const int BpsPerUnit = 10_000;

    /// <summary>
    /// Initializes a new instance of <see cref="ExecutionCostAggregator"/>.
    /// </summary>
    public ExecutionCostAggregator(
        SimpleSlippageModel slippageModel,
        SpreadEstimator spreadEstimator,
        CommissionCalculator commissionCalculator,
        RivrQuantDbContext db,
        ILogger<ExecutionCostAggregator> logger)
    {
        _slippageModel = slippageModel ?? throw new ArgumentNullException(nameof(slippageModel));
        _spreadEstimator = spreadEstimator ?? throw new ArgumentNullException(nameof(spreadEstimator));
        _commissionCalculator = commissionCalculator ?? throw new ArgumentNullException(nameof(commissionCalculator));
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Estimates the total execution cost for a proposed order, combining slippage,
    /// half-spread, and commission components.
    /// </summary>
    /// <param name="symbol">The ticker or token symbol.</param>
    /// <param name="broker">The target broker.</param>
    /// <param name="side">The order direction.</param>
    /// <param name="orderType">The order type.</param>
    /// <param name="quantity">The number of shares or contracts.</param>
    /// <param name="price">The expected execution price per unit.</param>
    /// <param name="assetClass">The asset class of the instrument.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>An <see cref="ExecutionCostEstimate"/> with the full cost breakdown.</returns>
    public Task<ExecutionCostEstimate> EstimateCostAsync(
        string symbol,
        BrokerType broker,
        OrderSide side,
        OrderType orderType,
        decimal quantity,
        decimal price,
        AssetClass assetClass,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var avgDailyVolume = _slippageModel.GetCachedVolume(symbol);

        var slippageBps = _slippageModel.EstimateSlippageBps(broker, orderType, quantity, avgDailyVolume);
        var spreadBps = _spreadEstimator.EstimateSpreadBps(symbol, broker, assetClass);
        var commissionDollars = _commissionCalculator.CalculateCommission(
            broker, side, orderType, quantity, price, assetClass);

        var notionalValue = quantity * price;
        var totalBps = slippageBps + spreadBps;

        // Convert commission to bps for total cost computation
        var commissionBps = notionalValue > 0
            ? commissionDollars / notionalValue * BpsPerUnit
            : 0m;

        var totalCostBps = totalBps + commissionBps;
        var totalCostDollars = notionalValue * totalCostBps / BpsPerUnit;

        var estimate = new ExecutionCostEstimate
        {
            Symbol = symbol,
            Broker = broker,
            Side = side,
            OrderType = orderType,
            Quantity = quantity,
            Price = price,
            AssetClass = assetClass,
            SlippageBps = slippageBps,
            SpreadBps = spreadBps,
            CommissionDollars = commissionDollars,
            CommissionBps = commissionBps,
            TotalCostBps = totalCostBps,
            TotalCostDollars = totalCostDollars,
            EstimatedAt = DateTimeOffset.UtcNow
        };

        _logger.LogInformation(
            "Execution cost estimate for {Symbol}: slippage={SlippageBps} bps, spread={SpreadBps} bps, " +
            "commission=${Commission}, total={TotalBps} bps (${TotalDollars})",
            symbol, slippageBps, spreadBps, commissionDollars, totalCostBps, totalCostDollars);

        return Task.FromResult(estimate);
    }

    /// <summary>
    /// Adjusts a backtest's metrics to account for estimated execution costs across
    /// all trades. Reduces the Sharpe ratio, annualized return, and total return
    /// proportionally to the aggregate cost impact.
    /// </summary>
    /// <param name="backtestResultId">The ID of the backtest result to adjust.</param>
    /// <param name="broker">The broker assumed for cost estimation.</param>
    /// <param name="assetClass">The asset class of the backtest instruments.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The adjusted <see cref="BacktestMetrics"/>, or <c>null</c> if the backtest was not found.</returns>
    public async Task<BacktestMetrics?> AdjustBacktestForCostsAsync(
        Guid backtestResultId,
        BrokerType broker,
        AssetClass assetClass,
        CancellationToken ct)
    {
        var backtest = await _db.BacktestResults
            .Include(b => b.Trades)
            .Include(b => b.Metrics)
            .FirstOrDefaultAsync(b => b.Id == backtestResultId, ct);

        if (backtest?.Metrics is null)
        {
            _logger.LogWarning("Backtest {Id} not found or has no metrics", backtestResultId);
            return null;
        }

        var totalCostDollars = 0m;

        foreach (var trade in backtest.Trades)
        {
            ct.ThrowIfCancellationRequested();

            // Entry cost
            var entryCost = await EstimateCostAsync(
                trade.Symbol, broker, OrderSide.Buy, OrderType.Market,
                trade.Quantity, trade.EntryPrice, assetClass, ct);

            // Exit cost
            var exitCost = await EstimateCostAsync(
                trade.Symbol, broker, OrderSide.Sell, OrderType.Market,
                trade.Quantity, trade.ExitPrice, assetClass, ct);

            totalCostDollars += entryCost.TotalCostDollars + exitCost.TotalCostDollars;
        }

        // Calculate the cost impact as a fraction of initial capital
        var costImpactFraction = backtest.InitialCapital > 0
            ? (double)(totalCostDollars / backtest.InitialCapital)
            : 0.0;

        var metrics = backtest.Metrics;

        // Adjust metrics by subtracting cost impact
        var adjustedAnnualizedReturn = metrics.AnnualizedReturn - costImpactFraction;
        var adjustedSharpe = metrics.AnnualizedVolatility > 0
            ? adjustedAnnualizedReturn / metrics.AnnualizedVolatility
            : 0.0;

        var adjustedTotalReturn = backtest.TotalReturn - (decimal)costImpactFraction;

        _logger.LogInformation(
            "Backtest {Id} cost adjustment: total costs=${TotalCosts:F2}, " +
            "Sharpe {OrigSharpe:F3} -> {AdjSharpe:F3}, " +
            "Return {OrigReturn:P2} -> {AdjReturn:P2}",
            backtestResultId, totalCostDollars,
            metrics.SharpeRatio, adjustedSharpe,
            backtest.TotalReturn, adjustedTotalReturn);

        // Return a new metrics record with adjusted values
        return new BacktestMetrics
        {
            BacktestResultId = backtestResultId,
            SharpeRatio = adjustedSharpe,
            SortinoRatio = metrics.SortinoRatio * (adjustedSharpe / (metrics.SharpeRatio != 0 ? metrics.SharpeRatio : 1.0)),
            MaxDrawdown = metrics.MaxDrawdown,
            WinRate = metrics.WinRate,
            ProfitFactor = metrics.ProfitFactor,
            CalmarRatio = Math.Abs(metrics.MaxDrawdown) > 1e-10
                ? adjustedAnnualizedReturn / Math.Abs(metrics.MaxDrawdown)
                : 0.0,
            ValueAtRisk95 = metrics.ValueAtRisk95,
            ExpectedShortfall95 = metrics.ExpectedShortfall95,
            Beta = metrics.Beta,
            AnnualizedReturn = adjustedAnnualizedReturn,
            AnnualizedVolatility = metrics.AnnualizedVolatility,
            TotalTrades = metrics.TotalTrades,
            WinningTrades = metrics.WinningTrades,
            LosingTrades = metrics.LosingTrades,
            AverageWin = metrics.AverageWin,
            AverageLoss = metrics.AverageLoss,
            LargestWin = metrics.LargestWin,
            LargestLoss = metrics.LargestLoss,
            AverageHoldingPeriod = metrics.AverageHoldingPeriod
        };
    }
}
