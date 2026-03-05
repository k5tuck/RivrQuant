using Microsoft.Extensions.Logging;
using RivrQuant.Domain.Enums;
using RivrQuant.Domain.Interfaces;
using RivrQuant.Domain.Models.Risk;

namespace RivrQuant.Infrastructure.Risk.PositionSizing;

/// <summary>
/// Position sizer that risks a constant percentage of portfolio equity on each trade,
/// adjusting quantity based on the stop-loss distance. Implements <see cref="IPositionSizer"/>
/// with <see cref="PositionSizingMethod.FixedFractional"/>.
/// </summary>
/// <remarks>
/// <para>Calculation:</para>
/// <code>
/// RiskPerTrade = PortfolioValue * RiskFraction (default 1%, range 0.5%–3%)
/// Quantity = RiskPerTrade / (CurrentPrice * StopLossPercent)
/// StopLossPercent default: 5% if no stop is defined
/// </code>
/// <para>This method ensures that the maximum loss per trade is bounded by a fixed
/// fraction of portfolio equity, regardless of position price or volatility.</para>
/// </remarks>
public sealed class FixedFractionalSizer : IPositionSizer
{
    private readonly ILogger<FixedFractionalSizer> _logger;

    /// <summary>Default risk fraction per trade (1%).</summary>
    private const decimal DefaultRiskFraction = 0.01m;

    /// <summary>Minimum allowable risk fraction per trade (0.5%).</summary>
    private const decimal MinRiskFraction = 0.005m;

    /// <summary>Maximum allowable risk fraction per trade (3%).</summary>
    private const decimal MaxRiskFraction = 0.03m;

    /// <summary>Default stop-loss percentage when none is provided (5%).</summary>
    private const decimal DefaultStopLossPercent = 0.05m;

    /// <summary>
    /// Gets the position sizing method implemented by this sizer.
    /// </summary>
    public PositionSizingMethod Method => PositionSizingMethod.FixedFractional;

    /// <summary>
    /// Initializes a new instance of <see cref="FixedFractionalSizer"/>.
    /// </summary>
    /// <param name="logger">Logger for diagnostics.</param>
    public FixedFractionalSizer(ILogger<FixedFractionalSizer> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Calculates a position size recommendation using fixed-fractional risk management.
    /// </summary>
    /// <param name="request">The position size request containing portfolio and stop-loss context.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A <see cref="PositionSizeRecommendation"/> with the computed quantity and reasoning.</returns>
    public Task<PositionSizeRecommendation> CalculateAsync(PositionSizeRequest request, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var riskFraction = Math.Clamp(
            request.RiskFractionPerTrade ?? DefaultRiskFraction,
            MinRiskFraction,
            MaxRiskFraction);

        var stopLossPercent = request.StopLossPercent is > 0
            ? request.StopLossPercent.Value
            : DefaultStopLossPercent;

        var riskPerTrade = request.PortfolioValue * riskFraction;
        var riskPerShare = request.CurrentPrice * stopLossPercent;

        var quantity = riskPerShare > 0
            ? Math.Floor(riskPerTrade / riskPerShare)
            : 0m;

        var targetDollarSize = quantity * request.CurrentPrice;

        _logger.LogInformation(
            "Fixed-fractional sizer for {Symbol}: risk={RiskFrac:P1}, stop={Stop:P1}, " +
            "riskPerTrade=${RiskPerTrade:F0}, qty={Qty}",
            request.Symbol, riskFraction, stopLossPercent, riskPerTrade, quantity);

        return Task.FromResult(new PositionSizeRecommendation
        {
            Symbol = request.Symbol,
            Method = Method,
            RecommendedQuantity = quantity,
            TargetDollarSize = targetDollarSize,
            ConfidenceScore = 0.8m, // Fixed-fractional is always computable
            Reasoning = $"Fixed-fractional: risk {riskFraction:P1} of portfolio (${riskPerTrade:F0}), " +
                        $"stop loss at {stopLossPercent:P1}, risk per share ${riskPerShare:F2}, " +
                        $"quantity={quantity:F0}"
        });
    }
}
