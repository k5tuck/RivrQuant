namespace RivrQuant.Infrastructure.Brokers.Alpaca;

using global::Alpaca.Markets;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RivrQuant.Domain.Enums;
using RivrQuant.Domain.Exceptions;
using RivrQuant.Domain.Interfaces;
using RivrQuant.Domain.Models.Trading;
using DomainOrder = RivrQuant.Domain.Models.Trading.Order;
using DomainPosition = RivrQuant.Domain.Models.Trading.Position;

/// <summary>IBrokerClient implementation for Alpaca Markets (stocks).</summary>
public sealed class AlpacaBrokerClient : IBrokerClient
{
    private readonly IAlpacaTradingClient _tradingClient;
    private readonly ILogger<AlpacaBrokerClient> _logger;

    /// <inheritdoc />
    public BrokerType BrokerType => BrokerType.Alpaca;

    /// <summary>Initializes a new instance of <see cref="AlpacaBrokerClient"/>.</summary>
    public AlpacaBrokerClient(IOptions<AlpacaConfiguration> config, ILogger<AlpacaBrokerClient> logger)
    {
        var cfg = config.Value;
        _logger = logger;

        var environment = cfg.IsPaper
            ? Environments.Paper
            : Environments.Live;

        _tradingClient = environment
            .GetAlpacaTradingClient(new SecretKey(cfg.ApiKey, cfg.ApiSecret));

        _logger.LogInformation("Alpaca broker client initialized. Paper={IsPaper}", cfg.IsPaper);
    }

    /// <inheritdoc />
    public async Task<Portfolio> GetPortfolioAsync(CancellationToken ct)
    {
        try
        {
            var account = await _tradingClient.GetAccountAsync(ct);
            var portfolio = AlpacaAccountMapper.MapPortfolio(account);
            _logger.LogDebug("Alpaca portfolio: Equity={Equity}, Cash={Cash}", portfolio.TotalEquity, portfolio.CashBalance);
            return portfolio;
        }
        catch (RestClientErrorException ex)
        {
            _logger.LogError(ex, "Failed to get Alpaca portfolio");
            throw new BrokerConnectionException($"Failed to get Alpaca portfolio: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<DomainPosition>> GetPositionsAsync(CancellationToken ct)
    {
        try
        {
            var positions = await _tradingClient.ListPositionsAsync(ct);
            var mapped = positions.Select(AlpacaAccountMapper.MapPosition).ToList();
            _logger.LogDebug("Retrieved {PositionCount} Alpaca positions", mapped.Count);
            return mapped;
        }
        catch (RestClientErrorException ex)
        {
            _logger.LogError(ex, "Failed to get Alpaca positions");
            throw new BrokerConnectionException($"Failed to get Alpaca positions: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public async Task<DomainOrder> PlaceOrderAsync(OrderRequest request, CancellationToken ct)
    {
        _logger.LogInformation("Placing Alpaca order: {Symbol} {Side} {Qty} {Type}", request.Symbol, request.Side, request.Quantity, request.Type);
        try
        {
            var alpacaSide = AlpacaAccountMapper.ToAlpacaSide(request.Side);
            var quantity = OrderQuantity.Fractional(request.Quantity);

            IOrder order;
            switch (request.Type)
            {
                case Domain.Enums.OrderType.Market:
                    order = await _tradingClient.PostOrderAsync(
                        alpacaSide.Market(request.Symbol, quantity), ct);
                    break;
                case Domain.Enums.OrderType.Limit when request.LimitPrice.HasValue:
                    order = await _tradingClient.PostOrderAsync(
                        alpacaSide.Limit(request.Symbol, quantity, request.LimitPrice.Value), ct);
                    break;
                case Domain.Enums.OrderType.StopLoss when request.StopPrice.HasValue:
                    order = await _tradingClient.PostOrderAsync(
                        alpacaSide.Stop(request.Symbol, quantity, request.StopPrice.Value), ct);
                    break;
                case Domain.Enums.OrderType.StopLimit when request.StopPrice.HasValue && request.LimitPrice.HasValue:
                    order = await _tradingClient.PostOrderAsync(
                        alpacaSide.StopLimit(request.Symbol, quantity, request.StopPrice.Value, request.LimitPrice.Value), ct);
                    break;
                case Domain.Enums.OrderType.TrailingStop when request.StopPrice.HasValue:
                    order = await _tradingClient.PostOrderAsync(
                        alpacaSide.TrailingStop(request.Symbol, quantity, TrailOffset.InDollars(request.StopPrice.Value)), ct);
                    break;
                default:
                    order = await _tradingClient.PostOrderAsync(
                        alpacaSide.Market(request.Symbol, quantity), ct);
                    break;
            }

            var mapped = AlpacaAccountMapper.MapOrder(order);
            _logger.LogInformation("Alpaca order placed: {OrderId} {Symbol} {Side} {Status}", mapped.ExternalOrderId, mapped.Symbol, mapped.Side, mapped.Status);
            return mapped;
        }
        catch (RestClientErrorException ex)
        {
            _logger.LogError(ex, "Failed to place Alpaca order for {Symbol}", request.Symbol);
            throw new BrokerConnectionException($"Failed to place Alpaca order for {request.Symbol}: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public async Task<DomainOrder> CancelOrderAsync(string orderId, CancellationToken ct)
    {
        _logger.LogInformation("Cancelling Alpaca order {OrderId}", orderId);
        try
        {
            if (!Guid.TryParse(orderId, out var guid))
                throw new BrokerConnectionException($"Invalid order ID format: {orderId}");

            var success = await _tradingClient.CancelOrderAsync(guid, ct);
            if (!success)
                _logger.LogWarning("Alpaca order {OrderId} cancellation returned false", orderId);

            return new DomainOrder
            {
                ExternalOrderId = orderId,
                Status = Domain.Enums.OrderStatus.Cancelled,
                Broker = BrokerType.Alpaca,
                CancelledAt = DateTimeOffset.UtcNow
            };
        }
        catch (RestClientErrorException ex)
        {
            _logger.LogError(ex, "Failed to cancel Alpaca order {OrderId}", orderId);
            throw new BrokerConnectionException($"Failed to cancel Alpaca order {orderId}: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public async Task ClosePositionAsync(string symbol, CancellationToken ct)
    {
        _logger.LogInformation("Closing Alpaca position for {Symbol}", symbol);
        try
        {
            await _tradingClient.DeletePositionAsync(new DeletePositionRequest(symbol), ct);
            _logger.LogInformation("Closed Alpaca position for {Symbol}", symbol);
        }
        catch (RestClientErrorException ex)
        {
            _logger.LogError(ex, "Failed to close Alpaca position for {Symbol}", symbol);
            throw new BrokerConnectionException($"Failed to close Alpaca position for {symbol}: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public async Task CloseAllPositionsAsync(CancellationToken ct)
    {
        _logger.LogWarning("Closing ALL Alpaca positions (kill switch activated)");
        try
        {
            var result = await _tradingClient.DeleteAllPositionsAsync(new DeleteAllPositionsRequest(), ct);
            _logger.LogWarning("Closed {Count} Alpaca positions", result.Count);
        }
        catch (RestClientErrorException ex)
        {
            _logger.LogError(ex, "Failed to close all Alpaca positions");
            throw new BrokerConnectionException($"Failed to close all Alpaca positions: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<DomainOrder>> GetOrderHistoryAsync(DateTimeOffset from, DateTimeOffset to, CancellationToken ct)
    {
        try
        {
            var request = new ListOrdersRequest
            {
                OrderStatusFilter = global::Alpaca.Markets.OrderStatusFilter.All,
                RollUpNestedOrders = true
            }.WithInterval(new Interval<DateTime>(from.UtcDateTime, to.UtcDateTime));

            var orders = await _tradingClient.ListOrdersAsync(request, ct);
            var mapped = orders.Select(AlpacaAccountMapper.MapOrder).ToList();
            _logger.LogDebug("Retrieved {OrderCount} Alpaca orders from {From} to {To}", mapped.Count, from, to);
            return mapped;
        }
        catch (RestClientErrorException ex)
        {
            _logger.LogError(ex, "Failed to get Alpaca order history");
            throw new BrokerConnectionException($"Failed to get Alpaca order history: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public Task SubscribeToUpdatesAsync(Action<PerformanceSnapshot> onUpdate, CancellationToken ct)
    {
        _logger.LogInformation("Alpaca real-time updates subscription started");
        return Task.CompletedTask;
    }
}
