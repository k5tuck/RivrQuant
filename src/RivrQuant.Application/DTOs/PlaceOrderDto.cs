using RivrQuant.Domain.Enums;

namespace RivrQuant.Application.DTOs;

/// <summary>
/// Data transfer object for placing a new trading order.
/// </summary>
public sealed record PlaceOrderDto
{
    /// <summary>Ticker symbol.</summary>
    public string Symbol { get; init; } = string.Empty;

    /// <summary>Order side (Buy or Sell).</summary>
    public OrderSide Side { get; init; }

    /// <summary>Order type (Market, Limit, StopLoss, etc.).</summary>
    public OrderType Type { get; init; }

    /// <summary>Quantity to trade.</summary>
    public decimal Quantity { get; init; }

    /// <summary>Limit price for Limit/StopLimit orders.</summary>
    public decimal? LimitPrice { get; init; }

    /// <summary>Stop price for StopLoss/StopLimit/TrailingStop orders.</summary>
    public decimal? StopPrice { get; init; }

    /// <summary>Target broker.</summary>
    public BrokerType Broker { get; init; }

    /// <summary>Asset class.</summary>
    public AssetClass AssetClass { get; init; } = AssetClass.Stock;
}
