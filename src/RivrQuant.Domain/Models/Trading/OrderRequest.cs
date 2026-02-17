using RivrQuant.Domain.Enums;

namespace RivrQuant.Domain.Models.Trading;

/// <summary>
/// Immutable request to place a new order with a broker.
/// Used as a data transfer object between application layers when submitting orders.
/// </summary>
/// <param name="Symbol">Ticker symbol of the instrument to trade.</param>
/// <param name="Side">Direction of the order (Buy or Sell).</param>
/// <param name="Type">Type of the order determining execution behavior (Market, Limit, etc.).</param>
/// <param name="Quantity">Number of shares or contracts to trade.</param>
/// <param name="LimitPrice">Limit price for Limit and StopLimit orders. Null for Market orders.</param>
/// <param name="StopPrice">Stop/trigger price for StopLoss and StopLimit orders. Null for Market/Limit orders.</param>
/// <param name="ClientOrderId">Optional client-generated identifier for order tracking. Auto-generated if null.</param>
/// <param name="AssetClass">Asset class of the instrument. Defaults to <see cref="Enums.AssetClass.Stock"/>.</param>
public record OrderRequest(
    string Symbol,
    OrderSide Side,
    OrderType Type,
    decimal Quantity,
    decimal? LimitPrice = null,
    decimal? StopPrice = null,
    string? ClientOrderId = null,
    AssetClass AssetClass = AssetClass.Stock
);
