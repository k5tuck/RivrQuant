using Microsoft.Extensions.Logging;
using RivrQuant.Domain.Enums;

namespace RivrQuant.Infrastructure.Execution;

/// <summary>
/// Estimates the half-spread cost in basis points for an instrument based on its
/// symbol, broker, and asset class. Uses hardcoded tiers for well-known liquid
/// instruments and conservative defaults for everything else.
/// </summary>
/// <remarks>
/// <para><b>Stocks (Alpaca):</b></para>
/// <list type="bullet">
///   <item>Large-cap liquid names (SPY, QQQ, AAPL, etc.): 0.5 bps</item>
///   <item>All other stocks: 2.0 bps</item>
/// </list>
/// <para><b>Crypto (Bybit):</b></para>
/// <list type="bullet">
///   <item>BTC: 1.0 bps</item>
///   <item>ETH: 1.5 bps</item>
///   <item>Major alts (SOL, AVAX, DOGE, ADA, DOT, LINK, MATIC): 3.0 bps</item>
///   <item>All other tokens: 8.0 bps</item>
/// </list>
/// </remarks>
public sealed class SpreadEstimator
{
    private readonly ILogger<SpreadEstimator> _logger;

    // ── Stock spread tiers ──

    private const decimal LargeCapSpreadBps = 0.5m;
    private const decimal DefaultStockSpreadBps = 2.0m;

    private static readonly HashSet<string> LargeCapSymbols = new(StringComparer.OrdinalIgnoreCase)
    {
        "SPY", "QQQ", "AAPL", "MSFT", "AMZN", "GOOGL", "META", "NVDA",
        "TSLA", "JPM", "V", "JNJ", "WMT", "PG", "UNH"
    };

    // ── Crypto spread tiers ──

    private const decimal BtcSpreadBps = 1.0m;
    private const decimal EthSpreadBps = 1.5m;
    private const decimal MajorAltSpreadBps = 3.0m;
    private const decimal DefaultCryptoSpreadBps = 8.0m;

    private static readonly HashSet<string> MajorAltSymbols = new(StringComparer.OrdinalIgnoreCase)
    {
        "SOL", "AVAX", "DOGE", "ADA", "DOT", "LINK", "MATIC"
    };

    /// <summary>
    /// Initializes a new instance of <see cref="SpreadEstimator"/>.
    /// </summary>
    /// <param name="logger">Logger for diagnostics.</param>
    public SpreadEstimator(ILogger<SpreadEstimator> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Estimates the half-spread cost in basis points for a given symbol.
    /// </summary>
    /// <param name="symbol">The ticker or token symbol (e.g., "AAPL", "BTC", "SOL").</param>
    /// <param name="broker">The broker through which the order will be routed.</param>
    /// <param name="assetClass">The asset class of the instrument.</param>
    /// <returns>Estimated half-spread cost in basis points.</returns>
    public decimal EstimateSpreadBps(string symbol, BrokerType broker, AssetClass assetClass)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(symbol);

        var normalizedSymbol = NormalizeSymbol(symbol);
        var spreadBps = assetClass switch
        {
            AssetClass.Crypto => EstimateCryptoSpread(normalizedSymbol),
            _ => EstimateStockSpread(normalizedSymbol)
        };

        _logger.LogDebug(
            "Spread estimate: symbol={Symbol}, broker={Broker}, asset={AssetClass}, spread={SpreadBps} bps",
            symbol, broker, assetClass, spreadBps);

        return spreadBps;
    }

    /// <summary>
    /// Estimates the half-spread for equity instruments based on membership in the
    /// large-cap liquid universe.
    /// </summary>
    private static decimal EstimateStockSpread(string symbol)
    {
        return LargeCapSymbols.Contains(symbol) ? LargeCapSpreadBps : DefaultStockSpreadBps;
    }

    /// <summary>
    /// Estimates the half-spread for crypto instruments using a tiered model
    /// (BTC, ETH, major alts, everything else).
    /// </summary>
    private static decimal EstimateCryptoSpread(string symbol)
    {
        if (string.Equals(symbol, "BTC", StringComparison.OrdinalIgnoreCase)
            || symbol.StartsWith("BTC", StringComparison.OrdinalIgnoreCase))
        {
            return BtcSpreadBps;
        }

        if (string.Equals(symbol, "ETH", StringComparison.OrdinalIgnoreCase)
            || symbol.StartsWith("ETH", StringComparison.OrdinalIgnoreCase))
        {
            return EthSpreadBps;
        }

        // Check if base token is a major alt (handle suffixed pairs like SOLUSDT)
        foreach (var alt in MajorAltSymbols)
        {
            if (symbol.StartsWith(alt, StringComparison.OrdinalIgnoreCase))
            {
                return MajorAltSpreadBps;
            }
        }

        return DefaultCryptoSpreadBps;
    }

    /// <summary>
    /// Strips common suffixes (USDT, USD, PERP) from crypto symbols and uppercases.
    /// Equity symbols are returned as-is (uppercased).
    /// </summary>
    private static string NormalizeSymbol(string symbol)
    {
        var upper = symbol.Trim().ToUpperInvariant();

        // Remove common pair suffixes for crypto matching
        foreach (var suffix in new[] { "USDT", "USD", "PERP", "/USDT", "/USD" })
        {
            if (upper.EndsWith(suffix, StringComparison.Ordinal) && upper.Length > suffix.Length)
            {
                return upper[..^suffix.Length];
            }
        }

        return upper;
    }
}
