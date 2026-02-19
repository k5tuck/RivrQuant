using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RivrQuant.Application.DTOs;
using RivrQuant.Domain.Enums;
using RivrQuant.Domain.Interfaces;
using RivrQuant.Domain.Models.Trading;
using RivrQuant.Infrastructure.Brokers.Alpaca;
using RivrQuant.Infrastructure.Brokers.Bybit;
using RivrQuant.Infrastructure.Persistence;

namespace RivrQuant.Application.Services;

/// <summary>
/// Application service for live trading operations across multiple brokers.
/// </summary>
public sealed class TradingService
{
    private readonly AlpacaBrokerClient _alpaca;
    private readonly BybitBrokerClient _bybit;
    private readonly RivrQuantDbContext _db;
    private readonly ILogger<TradingService> _logger;

    /// <summary>Initializes a new instance of <see cref="TradingService"/>.</summary>
    public TradingService(
        AlpacaBrokerClient alpaca,
        BybitBrokerClient bybit,
        RivrQuantDbContext db,
        ILogger<TradingService> logger)
    {
        _alpaca = alpaca;
        _bybit = bybit;
        _db = db;
        _logger = logger;
    }

    /// <summary>Retrieves all open positions across all brokers.</summary>
    public async Task<IReadOnlyList<Position>> GetAllPositionsAsync(CancellationToken ct)
    {
        var positions = new List<Position>();

        try
        {
            var alpacaPositions = await _alpaca.GetPositionsAsync(ct);
            positions.AddRange(alpacaPositions);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch Alpaca positions");
        }

        try
        {
            var bybitPositions = await _bybit.GetPositionsAsync(ct);
            positions.AddRange(bybitPositions);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch Bybit positions");
        }

        return positions;
    }

    /// <summary>Retrieves order history within a date range.</summary>
    public async Task<IReadOnlyList<Order>> GetOrderHistoryAsync(
        DateTimeOffset? from,
        DateTimeOffset? to,
        CancellationToken ct)
    {
        var start = from ?? DateTimeOffset.UtcNow.AddDays(-30);
        var end = to ?? DateTimeOffset.UtcNow;

        // Fetch from database first (persisted orders)
        var dbOrders = await _db.Orders
            .Include(o => o.Fills)
            .Where(o => o.CreatedAt >= start && o.CreatedAt <= end)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(ct);

        if (dbOrders.Count > 0)
            return dbOrders;

        // Fall back to broker APIs
        var orders = new List<Order>();

        try
        {
            var alpacaOrders = await _alpaca.GetOrderHistoryAsync(start, end, ct);
            orders.AddRange(alpacaOrders);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch Alpaca order history");
        }

        try
        {
            var bybitOrders = await _bybit.GetOrderHistoryAsync(start, end, ct);
            orders.AddRange(bybitOrders);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch Bybit order history");
        }

        return orders.OrderByDescending(o => o.CreatedAt).ToList();
    }

    /// <summary>Places a new order through the appropriate broker.</summary>
    public async Task<Order> PlaceOrderAsync(PlaceOrderDto dto, CancellationToken ct)
    {
        var broker = GetBrokerClient(dto.Broker);
        var request = new OrderRequest(
            Symbol: dto.Symbol,
            Side: dto.Side,
            Type: dto.Type,
            Quantity: dto.Quantity,
            LimitPrice: dto.LimitPrice,
            StopPrice: dto.StopPrice,
            AssetClass: dto.AssetClass);

        _logger.LogInformation(
            "Placing {Side} {Type} order for {Quantity} {Symbol} on {Broker}",
            dto.Side, dto.Type, dto.Quantity, dto.Symbol, dto.Broker);

        var order = await broker.PlaceOrderAsync(request, ct);

        // Persist the order
        _db.Orders.Add(order);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Order placed: {OrderId} (Status: {Status})", order.Id, order.Status);
        return order;
    }

    /// <summary>Cancels an open order.</summary>
    public async Task<Order> CancelOrderAsync(Guid orderId, CancellationToken ct)
    {
        var dbOrder = await _db.Orders.FindAsync(new object[] { orderId }, ct)
            ?? throw new KeyNotFoundException($"Order {orderId} not found.");

        var broker = GetBrokerClient(dbOrder.Broker);
        var cancelled = await broker.CancelOrderAsync(dbOrder.ExternalOrderId, ct);

        dbOrder.Status = OrderStatus.Cancelled;
        dbOrder.CancelledAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Order {OrderId} cancelled", orderId);
        return cancelled;
    }

    /// <summary>Closes all positions across all brokers (kill switch).</summary>
    public async Task CloseAllPositionsAsync(CancellationToken ct)
    {
        _logger.LogWarning("KILL SWITCH ACTIVATED - Closing all positions across all brokers");

        var tasks = new List<Task>();

        try
        {
            tasks.Add(_alpaca.CloseAllPositionsAsync(ct));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to close Alpaca positions");
        }

        try
        {
            tasks.Add(_bybit.CloseAllPositionsAsync(ct));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to close Bybit positions");
        }

        await Task.WhenAll(tasks);
        _logger.LogWarning("Kill switch complete - all position close orders submitted");
    }

    private IBrokerClient GetBrokerClient(BrokerType broker)
    {
        return broker switch
        {
            BrokerType.Alpaca => _alpaca,
            BrokerType.Bybit => _bybit,
            _ => throw new ArgumentOutOfRangeException(nameof(broker), $"Unsupported broker: {broker}")
        };
    }
}
