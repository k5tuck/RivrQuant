using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RivrQuant.Application.DTOs;
using RivrQuant.Domain.Enums;
using RivrQuant.Domain.Interfaces;
using RivrQuant.Domain.Models.Trading;
using RivrQuant.Infrastructure.Persistence;

namespace RivrQuant.Application.Services;

/// <summary>
/// Application service for live trading operations across multiple brokers.
/// Broker routing is performed via <see cref="IBrokerClientFactory"/> so that
/// the service is decoupled from concrete broker implementations and strategies
/// can nominate their own broker.
/// </summary>
public sealed class TradingService
{
    private readonly IBrokerClientFactory _brokerFactory;
    private readonly RivrQuantDbContext _db;
    private readonly ILogger<TradingService> _logger;

    /// <summary>Initializes a new instance of <see cref="TradingService"/>.</summary>
    public TradingService(
        IBrokerClientFactory brokerFactory,
        RivrQuantDbContext db,
        ILogger<TradingService> logger)
    {
        _brokerFactory = brokerFactory;
        _db = db;
        _logger = logger;
    }

    /// <summary>Retrieves all open positions across all brokers.</summary>
    public async Task<IReadOnlyList<Position>> GetAllPositionsAsync(CancellationToken ct)
    {
        var positions = new List<Position>();

        foreach (var brokerType in Enum.GetValues<BrokerType>())
        {
            try
            {
                var brokerPositions = await _brokerFactory.GetClient(brokerType).GetPositionsAsync(ct);
                positions.AddRange(brokerPositions);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to fetch {Broker} positions", brokerType);
            }
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
        var end   = to   ?? DateTimeOffset.UtcNow;

        // Fetch from database first (persisted orders).
        var dbOrders = (await _db.Orders
            .Include(o => o.Fills)
            .Where(o => o.CreatedAt >= start && o.CreatedAt <= end)
            .ToListAsync(ct))
            .OrderByDescending(o => o.CreatedAt)
            .ToList();

        if (dbOrders.Count > 0)
            return dbOrders;

        // Fall back to broker APIs if the DB has no records in range.
        var orders = new List<Order>();

        foreach (var brokerType in Enum.GetValues<BrokerType>())
        {
            try
            {
                var brokerOrders = await _brokerFactory.GetClient(brokerType).GetOrderHistoryAsync(start, end, ct);
                orders.AddRange(brokerOrders);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to fetch {Broker} order history", brokerType);
            }
        }

        return orders.OrderByDescending(o => o.CreatedAt).ToList();
    }

    /// <summary>Places a new order through the broker specified in the DTO.</summary>
    public async Task<Order> PlaceOrderAsync(PlaceOrderDto dto, CancellationToken ct)
    {
        var broker = _brokerFactory.GetClient(dto.Broker);
        var request = new OrderRequest(
            Symbol:      dto.Symbol,
            Side:        dto.Side,
            Type:        dto.Type,
            Quantity:    dto.Quantity,
            LimitPrice:  dto.LimitPrice,
            StopPrice:   dto.StopPrice,
            AssetClass:  dto.AssetClass);

        _logger.LogInformation(
            "Placing {Side} {Type} order for {Quantity} {Symbol} on {Broker}",
            dto.Side, dto.Type, dto.Quantity, dto.Symbol, dto.Broker);

        var order = await broker.PlaceOrderAsync(request, ct);

        _db.Orders.Add(order);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Order placed: {OrderId} (Status: {Status})", order.Id, order.Status);
        return order;
    }

    /// <summary>
    /// Places a new order for a strategy, resolving the target broker from the
    /// strategy's configured <see cref="Domain.Enums.BrokerType"/> stored in the database.
    /// </summary>
    public async Task<Order> PlaceOrderForStrategyAsync(Guid strategyId, OrderRequest request, CancellationToken ct)
    {
        var strategy = await _db.Strategies.FindAsync(new object[] { strategyId }, ct)
            ?? throw new KeyNotFoundException($"Strategy {strategyId} not found.");

        var broker = _brokerFactory.GetClient(strategy.Broker);

        _logger.LogInformation(
            "Placing {Side} {Type} order for {Quantity} {Symbol} via strategy {StrategyName} on {Broker}",
            request.Side, request.Type, request.Quantity, request.Symbol, strategy.Name, strategy.Broker);

        var order = await broker.PlaceOrderAsync(request, ct);

        _db.Orders.Add(order);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Order placed: {OrderId} for strategy {StrategyName} (Status: {Status})",
            order.Id, strategy.Name, order.Status);
        return order;
    }

    /// <summary>Cancels an open order.</summary>
    public async Task<Order> CancelOrderAsync(Guid orderId, CancellationToken ct)
    {
        var dbOrder = await _db.Orders.FindAsync(new object[] { orderId }, ct)
            ?? throw new KeyNotFoundException($"Order {orderId} not found.");

        var broker = _brokerFactory.GetClient(dbOrder.Broker);
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

        var tasks = Enum.GetValues<BrokerType>()
            .Select(async brokerType =>
            {
                try
                {
                    await _brokerFactory.GetClient(brokerType).CloseAllPositionsAsync(ct);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogError(ex, "Failed to close {Broker} positions", brokerType);
                }
            });

        await Task.WhenAll(tasks);
        _logger.LogWarning("Kill switch complete - all position close orders submitted");
    }
}
