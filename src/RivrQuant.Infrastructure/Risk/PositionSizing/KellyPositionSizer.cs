using Microsoft.Extensions.Logging;
using RivrQuant.Domain.Enums;
using RivrQuant.Domain.Interfaces;
using RivrQuant.Domain.Models.Risk;

namespace RivrQuant.Infrastructure.Risk.PositionSizing;

/// <summary>
/// Position sizer based on the Kelly criterion, which determines the theoretically
/// optimal fraction of capital to wager based on historical win rate and payoff ratio.
/// Implements <see cref="IPositionSizer"/> with <see cref="PositionSizingMethod.Kelly"/>.
/// </summary>
/// <remarks>
/// <para>The full Kelly fraction is calculated as:</para>
/// <code>FullKelly = WinRate - ((1 - WinRate) / AvgWinLossRatio)</code>
/// <para>A fractional Kelly (default 0.25, configurable 0.1–0.5) is applied to reduce
/// variance at the cost of slightly lower expected growth.</para>
/// <para>Rules:</para>
/// <list type="bullet">
///   <item>If FullKelly &lt;= 0, return quantity 0 with reasoning "Kelly negative"</item>
///   <item>If FullKelly &gt; 0.5, cap at 0.5 and log a warning</item>
///   <item>Requires minimum 30 historical trades (HistoricalWinRate must not be null)</item>
///   <item>Quantity = (PortfolioValue * FractionalKelly) / CurrentPrice</item>
/// </list>
/// </remarks>
public sealed class KellyPositionSizer : IPositionSizer
{
    private readonly ILogger<KellyPositionSizer> _logger;

    /// <summary>
    /// Default fractional Kelly multiplier applied to the full Kelly fraction.
    /// </summary>
    private const decimal DefaultKellyFraction = 0.25m;

    /// <summary>
    /// Minimum allowable Kelly fraction.
    /// </summary>
    private const decimal MinKellyFraction = 0.1m;

    /// <summary>
    /// Maximum allowable Kelly fraction.
    /// </summary>
    private const decimal MaxKellyFraction = 0.5m;

    /// <summary>
    /// Cap on the full Kelly value to prevent extreme position sizes.
    /// </summary>
    private const decimal FullKellyCap = 0.5m;

    /// <summary>
    /// Minimum number of historical trades required for a valid Kelly calculation.
    /// </summary>
    private const int MinHistoricalTrades = 30;

    /// <summary>
    /// Gets the position sizing method implemented by this sizer.
    /// </summary>
    public PositionSizingMethod Method => PositionSizingMethod.Kelly;

    /// <summary>
    /// Initializes a new instance of <see cref="KellyPositionSizer"/>.
    /// </summary>
    /// <param name="logger">Logger for diagnostics and warnings.</param>
    public KellyPositionSizer(ILogger<KellyPositionSizer> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Calculates a position size recommendation using the fractional Kelly criterion.
    /// </summary>
    /// <param name="request">The position size request containing strategy and portfolio context.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A <see cref="PositionSizeRecommendation"/> with the computed quantity and reasoning.</returns>
    public Task<PositionSizeRecommendation> CalculateAsync(PositionSizeRequest request, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        // Validate historical data availability
        if (request.HistoricalWinRate is null || request.HistoricalTradeCount < MinHistoricalTrades)
        {
            _logger.LogWarning(
                "Kelly sizer requires at least {MinTrades} historical trades. " +
                "Symbol={Symbol}, available trades={TradeCount}",
                MinHistoricalTrades, request.Symbol, request.HistoricalTradeCount);

            return Task.FromResult(new PositionSizeRecommendation
            {
                Symbol = request.Symbol,
                Method = Method,
                RecommendedQuantity = 0m,
                ConfidenceScore = 0m,
                Reasoning = $"Insufficient historical data: {request.HistoricalTradeCount} trades " +
                            $"(minimum {MinHistoricalTrades} required for Kelly criterion)"
            });
        }

        var winRate = request.HistoricalWinRate.Value;
        var avgWinLossRatio = request.AvgWinLossRatio > 0 ? request.AvgWinLossRatio : 1m;

        // Full Kelly = WinRate - ((1 - WinRate) / AvgWinLossRatio)
        var fullKelly = winRate - ((1m - winRate) / avgWinLossRatio);

        if (fullKelly <= 0)
        {
            _logger.LogInformation(
                "Kelly criterion is negative for {Symbol}: FullKelly={FullKelly:F4}, " +
                "WinRate={WinRate:P1}, WinLossRatio={Ratio:F2}",
                request.Symbol, fullKelly, winRate, avgWinLossRatio);

            return Task.FromResult(new PositionSizeRecommendation
            {
                Symbol = request.Symbol,
                Method = Method,
                RecommendedQuantity = 0m,
                ConfidenceScore = 0m,
                Reasoning = $"Kelly negative ({fullKelly:F4}): win rate {winRate:P1} with " +
                            $"avg win/loss ratio {avgWinLossRatio:F2} does not justify a position"
            });
        }

        // Cap full Kelly at 0.5 to prevent extreme sizing
        if (fullKelly > FullKellyCap)
        {
            _logger.LogWarning(
                "Full Kelly {FullKelly:F4} exceeds cap of {Cap}; capping. Symbol={Symbol}",
                fullKelly, FullKellyCap, request.Symbol);
            fullKelly = FullKellyCap;
        }

        // Apply fractional Kelly
        var kellyFraction = Math.Clamp(request.KellyFraction ?? DefaultKellyFraction, MinKellyFraction, MaxKellyFraction);
        var fractionalKelly = fullKelly * kellyFraction;

        // Calculate target position size in shares/contracts
        var targetDollarSize = request.PortfolioValue * fractionalKelly;
        var quantity = request.CurrentPrice > 0
            ? Math.Floor(targetDollarSize / request.CurrentPrice)
            : 0m;

        var confidence = Math.Min(1m, (decimal)request.HistoricalTradeCount / 100m);

        _logger.LogInformation(
            "Kelly sizer for {Symbol}: FullKelly={FullKelly:F4}, Fraction={Fraction:F2}, " +
            "FractionalKelly={FracKelly:F4}, Qty={Qty}, Confidence={Conf:F2}",
            request.Symbol, fullKelly, kellyFraction, fractionalKelly, quantity, confidence);

        return Task.FromResult(new PositionSizeRecommendation
        {
            Symbol = request.Symbol,
            Method = Method,
            RecommendedQuantity = quantity,
            TargetDollarSize = targetDollarSize,
            ConfidenceScore = confidence,
            Reasoning = $"Fractional Kelly ({kellyFraction:P0} of full {fullKelly:F4}): " +
                        $"win rate {winRate:P1}, win/loss ratio {avgWinLossRatio:F2}, " +
                        $"target allocation {fractionalKelly:P2} of portfolio"
        });
    }
}
