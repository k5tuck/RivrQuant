using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using RivrQuant.Domain.Enums;

namespace RivrQuant.Infrastructure.Execution;

/// <summary>
/// Estimates order slippage in basis points based on broker, order type, and participation rate.
/// Applies a tiered volume-impact model on top of broker-specific base slippage costs.
/// </summary>
/// <remarks>
/// Base slippage varies by broker and order type:
/// <list type="bullet">
///   <item>Alpaca market: 2.0 bps, limit: 0.5 bps</item>
///   <item>Bybit market: 5.0 bps, limit: 1.0 bps</item>
/// </list>
/// Volume impact is computed from the participation rate (order qty / avg daily volume):
/// <list type="bullet">
///   <item>&lt; 1%: 0 bps</item>
///   <item>1%–5%: participation * 100 bps</item>
///   <item>&gt; 5%: participation * 200 bps</item>
///   <item>No volume data: flat 3.0 bps surcharge</item>
/// </list>
/// </remarks>
public sealed class SimpleSlippageModel
{
    private readonly ILogger<SimpleSlippageModel> _logger;

    /// <summary>
    /// Thread-safe cache of average daily volume data per symbol.
    /// Entries are automatically refreshed after <see cref="VolumeCacheExpiry"/>.
    /// </summary>
    private readonly ConcurrentDictionary<string, (decimal? Volume, DateTimeOffset FetchedAt)> _volumeCache = new();

    /// <summary>
    /// Duration after which a cached volume entry is considered stale and should be refreshed.
    /// </summary>
    private static readonly TimeSpan VolumeCacheExpiry = TimeSpan.FromHours(24);

    // ── Base slippage by broker and order type (bps) ──

    private const decimal AlpacaMarketBps = 2.0m;
    private const decimal AlpacaLimitBps = 0.5m;
    private const decimal BybitMarketBps = 5.0m;
    private const decimal BybitLimitBps = 1.0m;

    // ── Volume-impact thresholds ──

    private const decimal LowParticipationThreshold = 0.01m;
    private const decimal HighParticipationThreshold = 0.05m;
    private const decimal LowParticipationMultiplier = 100m;
    private const decimal HighParticipationMultiplier = 200m;
    private const decimal NoVolumeSurchargeBps = 3.0m;

    /// <summary>
    /// Initializes a new instance of <see cref="SimpleSlippageModel"/>.
    /// </summary>
    /// <param name="logger">Logger for diagnostics and volume-cache misses.</param>
    public SimpleSlippageModel(ILogger<SimpleSlippageModel> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Estimates total slippage in basis points for a proposed order.
    /// </summary>
    /// <param name="broker">The broker through which the order will be routed.</param>
    /// <param name="orderType">The order type (Market, Limit, etc.).</param>
    /// <param name="quantity">The number of shares or contracts in the order.</param>
    /// <param name="avgDailyVolume">
    /// Average daily volume of the instrument. Pass <c>null</c> if unknown;
    /// a flat surcharge of 3.0 bps will be applied instead of volume impact.
    /// </param>
    /// <returns>Estimated total slippage in basis points.</returns>
    public decimal EstimateSlippageBps(
        BrokerType broker,
        OrderType orderType,
        decimal quantity,
        decimal? avgDailyVolume)
    {
        var baseSlippage = GetBaseSlippage(broker, orderType);
        var volumeImpact = CalculateVolumeImpact(quantity, avgDailyVolume);

        var totalSlippage = baseSlippage + volumeImpact;

        _logger.LogDebug(
            "Slippage estimate: broker={Broker}, type={OrderType}, qty={Quantity}, " +
            "adv={AvgDailyVolume}, base={BaseBps} bps, impact={ImpactBps} bps, total={TotalBps} bps",
            broker, orderType, quantity, avgDailyVolume, baseSlippage, volumeImpact, totalSlippage);

        return totalSlippage;
    }

    /// <summary>
    /// Retrieves the cached average daily volume for a symbol, or <c>null</c> if unavailable or stale.
    /// </summary>
    /// <param name="symbol">The ticker symbol to look up.</param>
    /// <returns>The cached volume, or <c>null</c> if no fresh entry exists.</returns>
    public decimal? GetCachedVolume(string symbol)
    {
        if (_volumeCache.TryGetValue(symbol, out var entry)
            && DateTimeOffset.UtcNow - entry.FetchedAt < VolumeCacheExpiry)
        {
            return entry.Volume;
        }

        return null;
    }

    /// <summary>
    /// Updates the cached average daily volume for a symbol.
    /// </summary>
    /// <param name="symbol">The ticker symbol to update.</param>
    /// <param name="volume">The average daily volume value to cache.</param>
    public void UpdateVolumeCache(string symbol, decimal? volume)
    {
        _volumeCache[symbol] = (volume, DateTimeOffset.UtcNow);
    }

    /// <summary>
    /// Determines the base slippage in basis points for a given broker and order type.
    /// Limit-style orders (Limit, StopLimit) receive the lower limit-based rate;
    /// all other order types receive the higher market-based rate.
    /// </summary>
    private static decimal GetBaseSlippage(BrokerType broker, OrderType orderType)
    {
        var isLimitStyle = orderType is OrderType.Limit or OrderType.StopLimit;

        return broker switch
        {
            BrokerType.Alpaca => isLimitStyle ? AlpacaLimitBps : AlpacaMarketBps,
            BrokerType.Bybit => isLimitStyle ? BybitLimitBps : BybitMarketBps,
            _ => AlpacaMarketBps // Conservative default
        };
    }

    /// <summary>
    /// Calculates the volume-impact component of slippage based on the
    /// participation rate (order quantity / average daily volume).
    /// </summary>
    private decimal CalculateVolumeImpact(decimal quantity, decimal? avgDailyVolume)
    {
        if (avgDailyVolume is null or <= 0)
        {
            _logger.LogDebug(
                "No average daily volume data available; applying flat surcharge of {Surcharge} bps",
                NoVolumeSurchargeBps);
            return NoVolumeSurchargeBps;
        }

        var participationRate = quantity / avgDailyVolume.Value;

        if (participationRate < LowParticipationThreshold)
        {
            return 0m;
        }

        if (participationRate <= HighParticipationThreshold)
        {
            return participationRate * LowParticipationMultiplier;
        }

        return participationRate * HighParticipationMultiplier;
    }
}
