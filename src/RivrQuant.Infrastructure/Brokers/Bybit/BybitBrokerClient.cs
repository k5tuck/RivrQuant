namespace RivrQuant.Infrastructure.Brokers.Bybit;

using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RivrQuant.Domain.Enums;
using RivrQuant.Domain.Exceptions;
using RivrQuant.Domain.Interfaces;
using RivrQuant.Domain.Models.Trading;
using DomainOrder = RivrQuant.Domain.Models.Trading.Order;
using DomainPosition = RivrQuant.Domain.Models.Trading.Position;

/// <summary>IBrokerClient implementation for Bybit (crypto) using API v5.</summary>
public sealed class BybitBrokerClient : IBrokerClient, IAsyncDisposable
{
    private readonly HttpClient _httpClient;
    private readonly BybitAuthenticator _authenticator;
    private readonly BybitConfiguration _config;
    private readonly ILogger<BybitBrokerClient> _logger;

    /// <inheritdoc />
    public BrokerType BrokerType => BrokerType.Bybit;

    /// <summary>Initializes a new instance of <see cref="BybitBrokerClient"/>.</summary>
    public BybitBrokerClient(HttpClient httpClient, IOptions<BybitConfiguration> config, ILogger<BybitBrokerClient> logger)
    {
        _config = config.Value;
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(_config.BaseUrl);
        _authenticator = new BybitAuthenticator(_config.ApiKey, _config.ApiSecret);
        _logger = logger;
        _logger.LogInformation("Bybit broker client initialized. Testnet={UseTestnet}", _config.UseTestnet);
    }

    /// <inheritdoc />
    public async Task<Portfolio> GetPortfolioAsync(CancellationToken ct)
    {
        var result = await SendGetAsync("/v5/account/wallet-balance?accountType=UNIFIED", ct);
        var list = result.GetProperty("list");
        if (list.GetArrayLength() == 0)
            return new Portfolio { Broker = BrokerType.Bybit };
        return BybitAccountMapper.MapWalletBalance(list[0]);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<DomainPosition>> GetPositionsAsync(CancellationToken ct)
    {
        var result = await SendGetAsync("/v5/position/list?category=linear&settleCoin=USDT", ct);
        var list = result.GetProperty("list");
        var positions = new List<DomainPosition>();
        foreach (var item in list.EnumerateArray())
        {
            var size = item.TryGetProperty("size", out var s) ? s.GetString() : "0";
            if (size != "0" && !string.IsNullOrEmpty(size))
                positions.Add(BybitAccountMapper.MapPosition(item));
        }
        _logger.LogDebug("Retrieved {PositionCount} Bybit positions", positions.Count);
        return positions;
    }

    /// <inheritdoc />
    public async Task<DomainOrder> PlaceOrderAsync(OrderRequest request, CancellationToken ct)
    {
        _logger.LogInformation("Placing Bybit order: {Symbol} {Side} {Qty} {Type}", request.Symbol, request.Side, request.Quantity, request.Type);
        var body = new Dictionary<string, object>
        {
            ["category"] = "linear",
            ["symbol"] = request.Symbol,
            ["side"] = request.Side == OrderSide.Buy ? "Buy" : "Sell",
            ["orderType"] = request.Type == Domain.Enums.OrderType.Limit ? "Limit" : "Market",
            ["qty"] = request.Quantity.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["timeInForce"] = "GTC"
        };

        if (request.LimitPrice.HasValue && request.Type == Domain.Enums.OrderType.Limit)
            body["price"] = request.LimitPrice.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);

        if (!string.IsNullOrEmpty(request.ClientOrderId))
            body["orderLinkId"] = request.ClientOrderId;

        var result = await SendPostAsync("/v5/order/create", body, ct);
        var orderId = result.TryGetProperty("orderId", out var oid) ? oid.GetString() ?? string.Empty : string.Empty;

        _logger.LogInformation("Bybit order placed: {OrderId} for {Symbol}", orderId, request.Symbol);
        return new DomainOrder
        {
            ExternalOrderId = orderId,
            Symbol = request.Symbol,
            Side = request.Side,
            Type = request.Type,
            Status = Domain.Enums.OrderStatus.Pending,
            Quantity = request.Quantity,
            LimitPrice = request.LimitPrice,
            Broker = BrokerType.Bybit,
            AssetClass = AssetClass.Crypto,
            ClientOrderId = request.ClientOrderId ?? Guid.NewGuid().ToString()
        };
    }

    /// <inheritdoc />
    public async Task<DomainOrder> CancelOrderAsync(string orderId, CancellationToken ct)
    {
        _logger.LogInformation("Cancelling Bybit order {OrderId}", orderId);
        var body = new Dictionary<string, object>
        {
            ["category"] = "linear",
            ["orderId"] = orderId
        };
        await SendPostAsync("/v5/order/cancel", body, ct);
        return new DomainOrder
        {
            ExternalOrderId = orderId,
            Status = Domain.Enums.OrderStatus.Cancelled,
            Broker = BrokerType.Bybit,
            CancelledAt = DateTimeOffset.UtcNow
        };
    }

    /// <inheritdoc />
    public async Task ClosePositionAsync(string symbol, CancellationToken ct)
    {
        _logger.LogInformation("Closing Bybit position for {Symbol}", symbol);
        var positions = await GetPositionsAsync(ct);
        var position = positions.FirstOrDefault(p => p.Symbol == symbol);
        if (position is null)
        {
            _logger.LogWarning("No open Bybit position found for {Symbol}", symbol);
            return;
        }

        var closeSide = position.Side == OrderSide.Buy ? OrderSide.Sell : OrderSide.Buy;
        await PlaceOrderAsync(new OrderRequest(symbol, closeSide, Domain.Enums.OrderType.Market, position.Quantity, AssetClass: AssetClass.Crypto), ct);
        _logger.LogInformation("Closed Bybit position for {Symbol}", symbol);
    }

    /// <inheritdoc />
    public async Task CloseAllPositionsAsync(CancellationToken ct)
    {
        _logger.LogWarning("Closing ALL Bybit positions (kill switch activated)");
        var positions = await GetPositionsAsync(ct);
        foreach (var position in positions)
        {
            try
            {
                await ClosePositionAsync(position.Symbol, ct);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Failed to close Bybit position for {Symbol}", position.Symbol);
            }
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<DomainOrder>> GetOrderHistoryAsync(DateTimeOffset from, DateTimeOffset to, CancellationToken ct)
    {
        var startTime = from.ToUnixTimeMilliseconds();
        var endTime = to.ToUnixTimeMilliseconds();
        var result = await SendGetAsync($"/v5/order/history?category=linear&startTime={startTime}&endTime={endTime}", ct);
        var list = result.GetProperty("list");
        var orders = new List<DomainOrder>();
        foreach (var item in list.EnumerateArray())
        {
            orders.Add(BybitAccountMapper.MapOrder(item));
        }
        return orders;
    }

    /// <inheritdoc />
    public Task SubscribeToUpdatesAsync(Action<PerformanceSnapshot> onUpdate, CancellationToken ct)
    {
        _logger.LogInformation("Bybit real-time updates subscription started");
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync()
    {
        _httpClient.Dispose();
        return ValueTask.CompletedTask;
    }

    private async Task<JsonElement> SendGetAsync(string path, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, path);
        _authenticator.SignRequest(request, null, _config.RecvWindow);
        var response = await _httpClient.SendAsync(request, ct);
        return await ParseResponseAsync(response, ct);
    }

    private async Task<JsonElement> SendPostAsync(string path, object body, CancellationToken ct)
    {
        var json = JsonSerializer.Serialize(body);
        using var request = new HttpRequestMessage(HttpMethod.Post, path)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
        _authenticator.SignRequest(request, json, _config.RecvWindow);
        var response = await _httpClient.SendAsync(request, ct);
        return await ParseResponseAsync(response, ct);
    }

    private async Task<JsonElement> ParseResponseAsync(HttpResponseMessage response, CancellationToken ct)
    {
        var body = await response.Content.ReadAsStringAsync(ct);
        var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;

        var retCode = root.TryGetProperty("retCode", out var rc) ? rc.GetInt32() : -1;
        if (retCode != 0)
        {
            var retMsg = root.TryGetProperty("retMsg", out var rm) ? rm.GetString() : "Unknown error";
            _logger.LogError("Bybit API error: retCode={RetCode}, retMsg={RetMsg}", retCode, retMsg);

            var errorMessage = retCode switch
            {
                10001 => $"Bybit parameter error: {retMsg}",
                10003 => $"Bybit invalid API key: {retMsg}. Verify BYBIT_API_KEY is correct.",
                110007 => $"Bybit insufficient balance: {retMsg}",
                _ => $"Bybit API error (code {retCode}): {retMsg}"
            };

            throw new BrokerConnectionException(errorMessage);
        }

        return root.TryGetProperty("result", out var result) ? result : root;
    }
}
