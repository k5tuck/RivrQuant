using RivrQuant.Domain.Enums;

namespace RivrQuant.Infrastructure.Execution;

/// <summary>
/// Calculates commission and regulatory fees for an order based on broker, side,
/// order type, and asset class.
/// </summary>
/// <remarks>
/// <para><b>Alpaca (equities):</b> $0 commission. Sell-side only:</para>
/// <list type="bullet">
///   <item>SEC fee: $0.00278 per $1,000 notional</item>
///   <item>TAF fee: $0.000166 per share, capped at $8.30</item>
/// </list>
/// <para><b>Bybit (crypto):</b></para>
/// <list type="bullet">
///   <item>Spot: maker 0.10%, taker 0.10%</item>
///   <item>Perpetual: maker 0.02%, taker 0.055%</item>
/// </list>
/// <para>Taker rates are applied for market orders; maker rates for limit orders.</para>
/// </remarks>
public sealed class CommissionCalculator
{
    // ── Alpaca regulatory fees (sell-side only) ──

    /// <summary>SEC fee rate: $0.00278 per $1,000 notional value.</summary>
    private const decimal SecFeeRatePerThousand = 0.00278m;

    /// <summary>TAF fee rate: $0.000166 per share.</summary>
    private const decimal TafFeePerShare = 0.000166m;

    /// <summary>Maximum TAF fee per trade.</summary>
    private const decimal TafFeeMaximum = 8.30m;

    // ── Bybit fee rates ──

    private const decimal BybitSpotMakerRate = 0.001m;   // 0.10%
    private const decimal BybitSpotTakerRate = 0.001m;    // 0.10%
    private const decimal BybitPerpMakerRate = 0.0002m;   // 0.02%
    private const decimal BybitPerpTakerRate = 0.00055m;  // 0.055%

    /// <summary>
    /// Calculates the total commission and regulatory fees for a proposed order.
    /// </summary>
    /// <param name="broker">The broker through which the order will be routed.</param>
    /// <param name="side">The order direction (Buy or Sell).</param>
    /// <param name="orderType">The order type, used to determine maker/taker classification for Bybit.</param>
    /// <param name="quantity">The number of shares or contracts.</param>
    /// <param name="price">The expected execution price per unit.</param>
    /// <param name="assetClass">The asset class of the instrument.</param>
    /// <returns>Total commission in the account's base currency.</returns>
    public decimal CalculateCommission(
        BrokerType broker,
        OrderSide side,
        OrderType orderType,
        decimal quantity,
        decimal price,
        AssetClass assetClass)
    {
        return broker switch
        {
            BrokerType.Alpaca => CalculateAlpacaCommission(side, quantity, price),
            BrokerType.Bybit => CalculateBybitCommission(orderType, quantity, price, assetClass),
            _ => 0m
        };
    }

    /// <summary>
    /// Calculates Alpaca commission. Alpaca charges $0 commission but passes through
    /// SEC and TAF regulatory fees on sell orders.
    /// </summary>
    private static decimal CalculateAlpacaCommission(
        OrderSide side,
        decimal quantity,
        decimal price)
    {
        // Alpaca has zero commission; regulatory fees apply only to sells
        if (side != OrderSide.Sell)
        {
            return 0m;
        }

        var notionalValue = quantity * price;

        // SEC fee: $0.00278 per $1,000 notional
        var secFee = notionalValue / 1000m * SecFeeRatePerThousand;

        // TAF fee: $0.000166 per share, max $8.30
        var tafFee = Math.Min(quantity * TafFeePerShare, TafFeeMaximum);

        return Math.Round(secFee + tafFee, 4);
    }

    /// <summary>
    /// Calculates Bybit commission based on spot vs. perpetual asset class
    /// and maker (limit) vs. taker (market) classification.
    /// </summary>
    private static decimal CalculateBybitCommission(
        OrderType orderType,
        decimal quantity,
        decimal price,
        AssetClass assetClass)
    {
        var notionalValue = quantity * price;
        var isMaker = orderType is OrderType.Limit or OrderType.StopLimit;

        var rate = assetClass switch
        {
            // Crypto spot
            AssetClass.Crypto when isMaker => BybitSpotMakerRate,
            AssetClass.Crypto => BybitSpotTakerRate,

            // For non-crypto on Bybit (perpetuals / derivatives), use perp rates
            _ when isMaker => BybitPerpMakerRate,
            _ => BybitPerpTakerRate
        };

        return Math.Round(notionalValue * rate, 4);
    }
}
