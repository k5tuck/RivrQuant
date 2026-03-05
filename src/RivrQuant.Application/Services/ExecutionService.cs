namespace RivrQuant.Application.Services;

using Microsoft.Extensions.Logging;
using RivrQuant.Domain.Enums;
using RivrQuant.Domain.Interfaces;
using RivrQuant.Domain.Models.Backtests;
using RivrQuant.Domain.Models.Execution;
using RivrQuant.Domain.Models.Trading;

/// <summary>Coordinates execution cost estimation and fill quality analysis.</summary>
public sealed class ExecutionService
{
    private readonly IExecutionCostModel _costModel;
    private readonly IFillAnalyzer _fillAnalyzer;
    private readonly ILogger<ExecutionService> _logger;

    public ExecutionService(
        IExecutionCostModel costModel,
        IFillAnalyzer fillAnalyzer,
        ILogger<ExecutionService> logger)
    {
        _costModel = costModel;
        _fillAnalyzer = fillAnalyzer;
        _logger = logger;
    }

    /// <summary>Estimate execution costs for a hypothetical order before placement.</summary>
    public async Task<ExecutionCostEstimate> EstimateCostAsync(
        string symbol,
        OrderSide side,
        decimal quantity,
        decimal currentPrice,
        BrokerType broker,
        CancellationToken ct)
    {
        var estimate = await _costModel.EstimateCostAsync(symbol, side, quantity, currentPrice, broker, ct);

        _logger.LogInformation(
            "Cost estimate for {Symbol}: {TotalBps:F1}bps ({TotalDollars:F2} USD). Slippage: {SlipBps:F1}bps, Spread: {SpreadBps:F1}bps, Commission: {Comm:F4} USD",
            symbol, estimate.TotalCostBps, estimate.TotalCostDollars,
            estimate.EstimatedSlippageBps, estimate.EstimatedSpreadCostBps, estimate.CommissionDollars);

        return estimate;
    }

    /// <summary>Analyze a completed fill against the pre-trade cost estimate.</summary>
    public async Task<FillAnalysis> AnalyzeFillAsync(
        Order order,
        Fill fill,
        ExecutionCostEstimate preTradeEstimate,
        CancellationToken ct)
    {
        var analysis = await _fillAnalyzer.AnalyzeFillAsync(order, fill, preTradeEstimate, ct);

        if (Math.Abs(analysis.SlippageDeviationBps) > 3m)
        {
            _logger.LogWarning(
                "Execution cost model deviation for {Symbol}: actual slippage {ActualBps:F1}bps vs estimated {EstBps:F1}bps (deviation: {DevBps:F1}bps)",
                analysis.Symbol, analysis.SlippageBps, analysis.EstimatedSlippageBps, analysis.SlippageDeviationBps);
        }

        return analysis;
    }

    /// <summary>Generate an execution quality report over a time period.</summary>
    public Task<ExecutionReport> GenerateReportAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        BrokerType? brokerFilter,
        CancellationToken ct)
        => _fillAnalyzer.GenerateReportAsync(from, to, brokerFilter, ct);

    /// <summary>Adjust backtest metrics for realistic execution costs.</summary>
    public async Task<BacktestMetrics> AdjustBacktestForCostsAsync(
        BacktestResult backtest,
        BrokerType broker,
        CancellationToken ct)
    {
        var adjusted = await _costModel.AdjustBacktestForCostsAsync(backtest, broker, ct);

        _logger.LogInformation(
            "Backtest {Id} cost-adjusted: Raw Sharpe {RawSharpe:F2} -> Adjusted Sharpe {AdjSharpe:F2}",
            backtest.Id, backtest.Metrics?.SharpeRatio ?? 0, adjusted.SharpeRatio);

        return adjusted;
    }
}
