namespace RivrQuant.Infrastructure.Brokers.Alpaca;

using Alpaca.Markets;
using RivrQuant.Domain.Enums;
using DomainOrder = RivrQuant.Domain.Models.Trading.Order;
using DomainPosition = RivrQuant.Domain.Models.Trading.Position;
using DomainPortfolio = RivrQuant.Domain.Models.Trading.Portfolio;

/// <summary>Maps Alpaca Markets SDK models to RivrQuant domain models.</summary>
public static class AlpacaAccountMapper
{
    /// <summary>Maps an Alpaca account to a domain Portfolio.</summary>
    public static DomainPortfolio MapPortfolio(IAccount account)
    {
        return new DomainPortfolio
        {
            TotalEquity = (decimal)(account.Equity ?? 0),
            CashBalance = (decimal)(account.TradableCash ?? 0),
            BuyingPower = (decimal)(account.BuyingPower ?? 0),
            UnrealizedPnl = 0,
            RealizedPnlToday = 0,
            DailyChangePercent = account.Equity.HasValue && account.LastEquity.HasValue && account.LastEquity.Value != 0
                ? (decimal)((account.Equity.Value - account.LastEquity.Value) / account.LastEquity.Value * 100)
                : 0,
            Broker = BrokerType.Alpaca
        };
    }

    /// <summary>Maps an Alpaca position to a domain Position.</summary>
    public static DomainPosition MapPosition(IPosition position)
    {
        return new DomainPosition
        {
            Symbol = position.Symbol,
            Side = position.Side == PositionSide.Long ? OrderSide.Buy : OrderSide.Sell,
            Quantity = Math.Abs(position.IntegerQuantity),
            AverageEntryPrice = position.AverageEntryPrice,
            CurrentPrice = position.AssetCurrentPrice,
            Broker = BrokerType.Alpaca,
            AssetClass = AssetClass.Stock
        };
    }

    /// <summary>Maps an Alpaca order to a domain Order.</summary>
    public static DomainOrder MapOrder(IOrder order)
    {
        return new DomainOrder
        {
            ExternalOrderId = order.OrderId.ToString(),
            ClientOrderId = order.ClientOrderId,
            Symbol = order.Symbol,
            Side = MapOrderSide(order.OrderSide),
            Type = MapOrderType(order.OrderType),
            Status = MapOrderStatus(order.OrderStatus),
            Quantity = (decimal)(order.Quantity ?? 0),
            LimitPrice = order.LimitPrice.HasValue ? (decimal)order.LimitPrice.Value : null,
            StopPrice = order.StopPrice.HasValue ? (decimal)order.StopPrice.Value : null,
            FilledQuantity = order.FilledQuantity.HasValue ? (decimal)order.FilledQuantity.Value : null,
            FilledAveragePrice = order.AverageFillPrice.HasValue ? (decimal)order.AverageFillPrice.Value : null,
            Broker = BrokerType.Alpaca,
            AssetClass = AssetClass.Stock,
            CreatedAt = order.CreatedAtUtc ?? DateTimeOffset.UtcNow,
            FilledAt = order.FilledAtUtc
        };
    }

    /// <summary>Maps Alpaca order side to domain OrderSide.</summary>
    public static OrderSide MapOrderSide(Alpaca.Markets.OrderSide side)
    {
        return side == Alpaca.Markets.OrderSide.Buy ? OrderSide.Buy : OrderSide.Sell;
    }

    /// <summary>Maps domain OrderSide to Alpaca order side.</summary>
    public static Alpaca.Markets.OrderSide ToAlpacaSide(OrderSide side)
    {
        return side == OrderSide.Buy ? Alpaca.Markets.OrderSide.Buy : Alpaca.Markets.OrderSide.Sell;
    }

    /// <summary>Maps Alpaca order type to domain OrderType.</summary>
    public static Domain.Enums.OrderType MapOrderType(Alpaca.Markets.OrderType type)
    {
        return type switch
        {
            Alpaca.Markets.OrderType.Market => Domain.Enums.OrderType.Market,
            Alpaca.Markets.OrderType.Limit => Domain.Enums.OrderType.Limit,
            Alpaca.Markets.OrderType.Stop => Domain.Enums.OrderType.StopLoss,
            Alpaca.Markets.OrderType.StopLimit => Domain.Enums.OrderType.StopLimit,
            Alpaca.Markets.OrderType.TrailingStop => Domain.Enums.OrderType.TrailingStop,
            _ => Domain.Enums.OrderType.Market
        };
    }

    /// <summary>Maps Alpaca order status to domain OrderStatus.</summary>
    public static Domain.Enums.OrderStatus MapOrderStatus(Alpaca.Markets.OrderStatus status)
    {
        return status switch
        {
            Alpaca.Markets.OrderStatus.New => Domain.Enums.OrderStatus.Pending,
            Alpaca.Markets.OrderStatus.Accepted => Domain.Enums.OrderStatus.Pending,
            Alpaca.Markets.OrderStatus.PendingNew => Domain.Enums.OrderStatus.Pending,
            Alpaca.Markets.OrderStatus.PartiallyFilled => Domain.Enums.OrderStatus.PartiallyFilled,
            Alpaca.Markets.OrderStatus.Filled => Domain.Enums.OrderStatus.Filled,
            Alpaca.Markets.OrderStatus.Canceled => Domain.Enums.OrderStatus.Cancelled,
            Alpaca.Markets.OrderStatus.Rejected => Domain.Enums.OrderStatus.Rejected,
            Alpaca.Markets.OrderStatus.Expired => Domain.Enums.OrderStatus.Expired,
            _ => Domain.Enums.OrderStatus.Pending
        };
    }
}
