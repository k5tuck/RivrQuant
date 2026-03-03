using Microsoft.Extensions.Logging;
using RivrQuant.Domain.Enums;
using RivrQuant.Domain.Interfaces;
using RivrQuant.Domain.Models.Risk;

namespace RivrQuant.Infrastructure.Risk.PositionSizing;

/// <summary>
/// Position sizer that scales position size inversely with realized volatility,
/// targeting a constant portfolio risk level. Implements <see cref="IPositionSizer"/>
/// with <see cref="PositionSizingMethod.VolatilityTarget"/>.
/// </summary>
/// <remarks>
/// <para>Calculation steps:</para>
/// <list type="bullet">
///   <item>VolMultiplier = TargetVol / RealizedVol, clamped to [0.25, 2.0]</item>
///   <item>BasePositionSize = PortfolioValue * MaxSinglePositionPercent / CurrentPrice</item>
///   <item>MaxSinglePositionPercent: 10% for stocks, 5% for crypto</item>
///   <item>AdjustedSize = BasePositionSize * VolMultiplier</item>
/// </list>
/// <para>If fewer than 5 days of volatility data are available, a conservative
/// default multiplier of 0.5 is used.</para>
/// </remarks>
public sealed class VolatilityTargetSizer : IPositionSizer
{
    private readonly ILogger<VolatilityTargetSizer> _logger;

    /// <summary>Maximum single-position allocation for equities.</summary>
    private const decimal StockMaxPositionPercent = 0.10m;

    /// <summary>Maximum single-position allocation for crypto.</summary>
    private const decimal CryptoMaxPositionPercent = 0.05m;

    /// <summary>Lower bound for the volatility adjustment multiplier.</summary>
    private const decimal MinVolMultiplier = 0.25m;

    /// <summary>Upper bound for the volatility adjustment multiplier.</summary>
    private const decimal MaxVolMultiplier = 2.0m;

    /// <summary>Conservative fallback multiplier when insufficient volatility data is available.</summary>
    private const decimal ConservativeDefaultMultiplier = 0.5m;

    /// <summary>Minimum number of daily return observations needed for a valid vol estimate.</summary>
    private const int MinVolatilityDays = 5;

    /// <summary>
    /// Gets the position sizing method implemented by this sizer.
    /// </summary>
    public PositionSizingMethod Method => PositionSizingMethod.VolatilityTarget;

    /// <summary>
    /// Initializes a new instance of <see cref="VolatilityTargetSizer"/>.
    /// </summary>
    /// <param name="logger">Logger for diagnostics.</param>
    public VolatilityTargetSizer(ILogger<VolatilityTargetSizer> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Calculates a position size recommendation using volatility-target scaling.
    /// </summary>
    /// <param name="request">The position size request containing volatility and portfolio context.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A <see cref="PositionSizeRecommendation"/> with the computed quantity and reasoning.</returns>
    public Task<PositionSizeRecommendation> CalculateAsync(PositionSizeRequest request, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var maxPositionPercent = request.AssetClass == AssetClass.Crypto
            ? CryptoMaxPositionPercent
            : StockMaxPositionPercent;

        decimal volMultiplier;
        string volReasoning;

        if (request.RealizedVolatility is null or <= 0 || request.VolatilityDays < MinVolatilityDays)
        {
            volMultiplier = ConservativeDefaultMultiplier;
            volReasoning = $"Insufficient volatility data ({request.VolatilityDays} days, " +
                           $"minimum {MinVolatilityDays}); using conservative multiplier {ConservativeDefaultMultiplier}";

            _logger.LogWarning(
                "Vol-target sizer using conservative default for {Symbol}: only {Days} days of vol data",
                request.Symbol, request.VolatilityDays);
        }
        else
        {
            var targetVol = request.TargetVolatility ?? 0.10m; // Default 10% annualized
            var realizedVol = request.RealizedVolatility.Value;

            volMultiplier = realizedVol > 0 ? targetVol / realizedVol : ConservativeDefaultMultiplier;
            volMultiplier = Math.Clamp(volMultiplier, MinVolMultiplier, MaxVolMultiplier);

            volReasoning = $"TargetVol={targetVol:P1}, RealizedVol={realizedVol:P1}, " +
                           $"VolMultiplier={volMultiplier:F3} (clamped [{MinVolMultiplier}, {MaxVolMultiplier}])";
        }

        // Base position size before volatility adjustment
        var basePositionDollars = request.PortfolioValue * maxPositionPercent;
        var basePositionSize = request.CurrentPrice > 0
            ? basePositionDollars / request.CurrentPrice
            : 0m;

        // Apply volatility multiplier
        var adjustedSize = Math.Floor(basePositionSize * volMultiplier);

        var targetDollarSize = adjustedSize * request.CurrentPrice;
        var confidence = request.VolatilityDays >= 20 ? 0.9m : (decimal)request.VolatilityDays / 20m;

        _logger.LogInformation(
            "Vol-target sizer for {Symbol}: maxPosPercent={MaxPct:P0}, base={Base:F1}, " +
            "volMult={VolMult:F3}, adjusted={Adjusted:F1}, ${Dollars:F0}",
            request.Symbol, maxPositionPercent, basePositionSize,
            volMultiplier, adjustedSize, targetDollarSize);

        return Task.FromResult(new PositionSizeRecommendation
        {
            Symbol = request.Symbol,
            Method = Method,
            RecommendedQuantity = adjustedSize,
            TargetDollarSize = targetDollarSize,
            ConfidenceScore = confidence,
            Reasoning = $"Vol-target sizing: MaxPos={maxPositionPercent:P0}, {volReasoning}, " +
                        $"Base={basePositionSize:F1} -> Adjusted={adjustedSize:F1}"
        });
    }
}
