namespace RivrQuant.Infrastructure.Brokers.Bybit;

using System.Text.Json;
using RivrQuant.Domain.Enums;
using DomainOrder = RivrQuant.Domain.Models.Trading.Order;
using DomainPosition = RivrQuant.Domain.Models.Trading.Position;
using DomainPortfolio = RivrQuant.Domain.Models.Trading.Portfolio;
using DomainFill = RivrQuant.Domain.Models.Trading.Fill;

/// <summary>Maps Bybit API v5 JSON responses to RivrQuant domain models.</summary>
public static class BybitAccountMapper
{
    /// <summary>Maps a Bybit wallet balance response to a domain Portfolio.</summary>
    public static DomainPortfolio MapWalletBalance(JsonElement result)
    {
        var totalEquity = GetDecimal(result, "totalEquity");
        var availableBalance = GetDecimal(result, "totalAvailableBalance");
        var unrealizedPnl = GetDecimal(result, "totalPerpUPL");

        return new DomainPortfolio
        {
            TotalEquity = totalEquity,
            CashBalance = availableBalance,
            BuyingPower = availableBalance,
            UnrealizedPnl = unrealizedPnl,
            Broker = BrokerType.Bybit
        };
    }

    /// <summary>Maps a Bybit position response item to a domain Position.</summary>
    public static DomainPosition MapPosition(JsonElement item)
    {
        var symbol = item.GetProperty("symbol").GetString() ?? string.Empty;
        var side = item.GetProperty("side").GetString() ?? "Buy";
        var size = GetDecimal(item, "size");
        var avgPrice = GetDecimal(item, "avgPrice");
        var markPrice = GetDecimal(item, "markPrice");

        return new DomainPosition
        {
            Symbol = symbol,
            Side = side.Equals("Buy", StringComparison.OrdinalIgnoreCase) ? OrderSide.Buy : OrderSide.Sell,
            Quantity = size,
            AverageEntryPrice = avgPrice,
            CurrentPrice = markPrice,
            Broker = BrokerType.Bybit,
            AssetClass = AssetClass.Crypto
        };
    }

    /// <summary>Maps a Bybit order response item to a domain Order.</summary>
    public static DomainOrder MapOrder(JsonElement item)
    {
        var orderId = item.GetProperty("orderId").GetString() ?? string.Empty;
        var symbol = item.GetProperty("symbol").GetString() ?? string.Empty;
        var side = item.GetProperty("side").GetString() ?? "Buy";
        var orderType = item.GetProperty("orderType").GetString() ?? "Market";
        var status = item.GetProperty("orderStatus").GetString() ?? "New";
        var qty = GetDecimal(item, "qty");
        var price = GetDecimalOrNull(item, "price");
        var avgPrice = GetDecimalOrNull(item, "avgPrice");
        var cumExecQty = GetDecimalOrNull(item, "cumExecQty");

        return new DomainOrder
        {
            ExternalOrderId = orderId,
            Symbol = symbol,
            Side = side.Equals("Buy", StringComparison.OrdinalIgnoreCase) ? OrderSide.Buy : OrderSide.Sell,
            Type = MapOrderType(orderType),
            Status = MapOrderStatus(status),
            Quantity = qty,
            LimitPrice = price,
            FilledQuantity = cumExecQty,
            FilledAveragePrice = avgPrice,
            Broker = BrokerType.Bybit,
            AssetClass = AssetClass.Crypto
        };
    }

    /// <summary>Maps a Bybit execution response item to a domain Fill.</summary>
    public static DomainFill MapExecution(JsonElement item)
    {
        var execId = item.GetProperty("execId").GetString() ?? string.Empty;
        var price = GetDecimal(item, "execPrice");
        var qty = GetDecimal(item, "execQty");
        var fee = GetDecimal(item, "execFee");
        var time = item.TryGetProperty("execTime", out var t) ? t.GetString() : null;

        return new DomainFill
        {
            ExternalFillId = execId,
            Price = price,
            Quantity = qty,
            Commission = Math.Abs(fee),
            FilledAt = long.TryParse(time, out var ms) ? DateTimeOffset.FromUnixTimeMilliseconds(ms) : DateTimeOffset.UtcNow
        };
    }

    private static Domain.Enums.OrderType MapOrderType(string type)
    {
        return type.ToUpperInvariant() switch
        {
            "MARKET" => Domain.Enums.OrderType.Market,
            "LIMIT" => Domain.Enums.OrderType.Limit,
            _ => Domain.Enums.OrderType.Market
        };
    }

    private static Domain.Enums.OrderStatus MapOrderStatus(string status)
    {
        return status switch
        {
            "New" => Domain.Enums.OrderStatus.Pending,
            "Created" => Domain.Enums.OrderStatus.Pending,
            "PartiallyFilled" => Domain.Enums.OrderStatus.PartiallyFilled,
            "Filled" => Domain.Enums.OrderStatus.Filled,
            "Cancelled" => Domain.Enums.OrderStatus.Cancelled,
            "Rejected" => Domain.Enums.OrderStatus.Rejected,
            "Deactivated" => Domain.Enums.OrderStatus.Cancelled,
            _ => Domain.Enums.OrderStatus.Pending
        };
    }

    private static decimal GetDecimal(JsonElement element, string property)
    {
        if (element.TryGetProperty(property, out var prop))
        {
            var str = prop.GetString();
            if (decimal.TryParse(str, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var value))
                return value;
        }
        return 0;
    }

    private static decimal? GetDecimalOrNull(JsonElement element, string property)
    {
        if (element.TryGetProperty(property, out var prop))
        {
            var str = prop.GetString();
            if (!string.IsNullOrEmpty(str) && decimal.TryParse(str, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var value))
                return value;
        }
        return null;
    }
}
