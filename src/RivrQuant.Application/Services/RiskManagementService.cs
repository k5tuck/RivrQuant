namespace RivrQuant.Application.Services;

using Microsoft.Extensions.Logging;
using RivrQuant.Domain.Enums;
using RivrQuant.Domain.Interfaces;
using RivrQuant.Domain.Models.Execution;
using RivrQuant.Domain.Models.Risk;
using RivrQuant.Domain.Models.Trading;

/// <summary>Coordinates position sizing, drawdown management, and volatility targeting.</summary>
public sealed class RiskManagementService
{
    private readonly IPositionSizer _compositeSizer;
    private readonly IDrawdownManager _drawdownManager;
    private readonly IVolatilityTargetEngine _volEngine;
    private readonly IExecutionCostModel _costModel;
    private readonly IPortfolioTracker _portfolioTracker;
    private readonly ILogger<RiskManagementService> _logger;

    public RiskManagementService(
        IPositionSizer compositeSizer,
        IDrawdownManager drawdownManager,
        IVolatilityTargetEngine volEngine,
        IExecutionCostModel costModel,
        IPortfolioTracker portfolioTracker,
        ILogger<RiskManagementService> logger)
    {
        _compositeSizer = compositeSizer;
        _drawdownManager = drawdownManager;
        _volEngine = volEngine;
        _costModel = costModel;
        _portfolioTracker = portfolioTracker;
        _logger = logger;
    }

    /// <summary>
    /// Calculate recommended position size for a trade signal, accounting for
    /// drawdown state, volatility target, and execution costs.
    /// </summary>
    public async Task<PositionSizeRecommendation> CalculatePositionSizeAsync(
        string symbol,
        OrderSide side,
        decimal currentPrice,
        decimal signalConfidence,
        decimal? historicalWinRate,
        decimal? historicalAvgWinLossRatio,
        decimal? assetAnnualizedVol,
        BrokerType broker,
        CancellationToken ct)
    {
        var portfolio = await _portfolioTracker.GetAggregatePortfolioAsync(ct);
        var drawdownState = await _drawdownManager.GetCurrentStateAsync(ct);
        var volTarget = await _volEngine.GetCurrentTargetAsync(ct);

        if (drawdownState.CurrentMultiplier == 0)
        {
            _logger.LogWarning("Trading halted — emergency drawdown level. Returning zero size for {Symbol}", symbol);
            return new PositionSizeRecommendation
            {
                Symbol = symbol,
                RecommendedQuantity = 0,
                RecommendedNotional = 0,
                PercentOfPortfolio = 0,
                RiskPerTrade = 0,
                MethodUsed = PositionSizingMethod.Composite,
                Reasoning = "Trading halted — emergency drawdown level reached. All new positions blocked.",
                WasReducedByDrawdown = true,
                WasReducedByVolTarget = false,
                PreAdjustmentQuantity = 0,
                DrawdownMultiplier = 0,
                VolatilityMultiplier = volTarget.VolMultiplier,
                CalculatedAt = DateTimeOffset.UtcNow
            };
        }

        var costEstimate = await _costModel.EstimateCostAsync(symbol, side, 1m, currentPrice, broker, ct);

        var request = new PositionSizeRequest
        {
            Symbol = symbol,
            Side = side,
            CurrentPrice = currentPrice,
            SignalConfidence = signalConfidence,
            CurrentPortfolio = portfolio,
            CurrentDrawdownState = drawdownState,
            CurrentVolTarget = volTarget,
            EstimatedCosts = costEstimate,
            HistoricalWinRate = historicalWinRate,
            HistoricalAvgWinLossRatio = historicalAvgWinLossRatio,
            AssetAnnualizedVol = assetAnnualizedVol
        };

        var recommendation = await _compositeSizer.CalculateSizeAsync(request, ct);

        if (costEstimate.CostAsPercentOfPrice > 1.0m)
        {
            _logger.LogWarning(
                "High execution cost warning for {Symbol}: {CostPct:F2}% of position value. Consider reducing trade frequency.",
                symbol, costEstimate.CostAsPercentOfPrice);
        }

        _logger.LogInformation(
            "Position size for {Symbol}: {Qty} ({Pct:F2}% of portfolio) via {Method}. Drawdown mult: {DD:F2}, Vol mult: {Vol:F2}",
            symbol, recommendation.RecommendedQuantity, recommendation.PercentOfPortfolio,
            recommendation.MethodUsed, recommendation.DrawdownMultiplier, recommendation.VolatilityMultiplier);

        return recommendation;
    }

    /// <summary>Gets the current drawdown state for display on the risk dashboard.</summary>
    public Task<DrawdownState> GetDrawdownStateAsync(CancellationToken ct)
        => _drawdownManager.GetCurrentStateAsync(ct);

    /// <summary>Gets the current volatility target state.</summary>
    public Task<VolatilityTarget> GetVolatilityTargetAsync(CancellationToken ct)
        => _volEngine.GetCurrentTargetAsync(ct);

    /// <summary>Gets the current drawdown multiplier for order validation.</summary>
    public Task<decimal> GetDrawdownMultiplierAsync(CancellationToken ct)
        => _drawdownManager.GetDrawdownMultiplierAsync(ct);
}
