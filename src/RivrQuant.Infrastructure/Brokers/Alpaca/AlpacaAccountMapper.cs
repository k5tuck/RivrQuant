namespace RivrQuant.Infrastructure.Brokers.Alpaca;

using global::Alpaca.Markets;
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
            TotalEquity = account.Equity ?? 0m,
            CashBalance = account.TradableCash,
            BuyingPower = account.BuyingPower ?? 0m,
            UnrealizedPnl = 0,
            RealizedPnlToday = 0,
            DailyChangePercent = account.LastEquity != 0
                ? (account.Equity.GetValueOrDefault() - account.LastEquity) / account.LastEquity * 100
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
            Side = position.Side == PositionSide.Long ? Domain.Enums.OrderSide.Buy : Domain.Enums.OrderSide.Sell,
            Quantity = Math.Abs(position.IntegerQuantity),
            AverageEntryPrice = position.AverageEntryPrice,
            CurrentPrice = position.AssetCurrentPrice ?? 0,
            Broker = BrokerType.Alpaca,
            AssetClass = Domain.Enums.AssetClass.Stock
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
            FilledQuantity = order.FilledQuantity,
            FilledAveragePrice = order.AverageFillPrice.HasValue ? (decimal)order.AverageFillPrice.Value : null,
            Broker = BrokerType.Alpaca,
            AssetClass = Domain.Enums.AssetClass.Stock,
            CreatedAt = order.CreatedAtUtc ?? DateTimeOffset.UtcNow,
            FilledAt = order.FilledAtUtc
        };
    }

    /// <summary>Maps Alpaca order side to domain OrderSide.</summary>
    public static Domain.Enums.OrderSide MapOrderSide(global::Alpaca.Markets.OrderSide side)
    {
        return side == global::Alpaca.Markets.OrderSide.Buy ? Domain.Enums.OrderSide.Buy : Domain.Enums.OrderSide.Sell;
    }

    /// <summary>Maps domain OrderSide to Alpaca order side.</summary>
    public static global::Alpaca.Markets.OrderSide ToAlpacaSide(Domain.Enums.OrderSide side)
    {
        return side == Domain.Enums.OrderSide.Buy ? global::Alpaca.Markets.OrderSide.Buy : global::Alpaca.Markets.OrderSide.Sell;
    }

    /// <summary>Maps Alpaca order type to domain OrderType.</summary>
    public static Domain.Enums.OrderType MapOrderType(global::Alpaca.Markets.OrderType type)
    {
        return type switch
        {
            global::Alpaca.Markets.OrderType.Market => Domain.Enums.OrderType.Market,
            global::Alpaca.Markets.OrderType.Limit => Domain.Enums.OrderType.Limit,
            global::Alpaca.Markets.OrderType.Stop => Domain.Enums.OrderType.StopLoss,
            global::Alpaca.Markets.OrderType.StopLimit => Domain.Enums.OrderType.StopLimit,
            global::Alpaca.Markets.OrderType.TrailingStop => Domain.Enums.OrderType.TrailingStop,
            _ => Domain.Enums.OrderType.Market
        };
    }

    /// <summary>Maps Alpaca order status to domain OrderStatus.</summary>
    public static Domain.Enums.OrderStatus MapOrderStatus(global::Alpaca.Markets.OrderStatus status)
    {
        return status switch
        {
            global::Alpaca.Markets.OrderStatus.New => Domain.Enums.OrderStatus.Pending,
            global::Alpaca.Markets.OrderStatus.Accepted => Domain.Enums.OrderStatus.Pending,
            global::Alpaca.Markets.OrderStatus.PendingNew => Domain.Enums.OrderStatus.Pending,
            global::Alpaca.Markets.OrderStatus.PartiallyFilled => Domain.Enums.OrderStatus.PartiallyFilled,
            global::Alpaca.Markets.OrderStatus.Filled => Domain.Enums.OrderStatus.Filled,
            global::Alpaca.Markets.OrderStatus.Canceled => Domain.Enums.OrderStatus.Cancelled,
            global::Alpaca.Markets.OrderStatus.Rejected => Domain.Enums.OrderStatus.Rejected,
            global::Alpaca.Markets.OrderStatus.Expired => Domain.Enums.OrderStatus.Expired,
            _ => Domain.Enums.OrderStatus.Pending
        };
    }
}
