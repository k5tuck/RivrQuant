namespace RivrQuant.Infrastructure.Brokers.Bybit;

using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RivrQuant.Domain.Models.Trading;

/// <summary>WebSocket client for Bybit real-time data feeds using API v5.</summary>
public sealed class BybitWebSocketClient : IAsyncDisposable
{
    private readonly BybitConfiguration _config;
    private readonly BybitAuthenticator _authenticator;
    private readonly ILogger<BybitWebSocketClient> _logger;
    private ClientWebSocket? _webSocket;
    private CancellationTokenSource? _cts;
    private bool _disposed;

    /// <summary>Event raised when an execution update is received.</summary>
    public event Action<PerformanceSnapshot>? OnExecutionUpdate;

    /// <summary>Initializes a new instance of <see cref="BybitWebSocketClient"/>.</summary>
    public BybitWebSocketClient(IOptions<BybitConfiguration> config, ILogger<BybitWebSocketClient> logger)
    {
        _config = config.Value;
        _authenticator = new BybitAuthenticator(_config.ApiKey, _config.ApiSecret);
        _logger = logger;
    }

    /// <summary>Connects to the Bybit WebSocket and authenticates.</summary>
    public async Task ConnectAsync(CancellationToken ct)
    {
        _webSocket = new ClientWebSocket();
        _cts = CancellationTokenSource.CreateLinkedTokenSource(ct);

        _logger.LogInformation("Connecting to Bybit WebSocket: {Url}", _config.WebSocketUrl);
        await _webSocket.ConnectAsync(new Uri(_config.WebSocketUrl), ct);

        var expires = DateTimeOffset.UtcNow.AddSeconds(30).ToUnixTimeMilliseconds();
        var signature = _authenticator.ComputeWebSocketSignature(expires);
        var authMessage = JsonSerializer.Serialize(new
        {
            op = "auth",
            args = new object[] { _config.ApiKey, expires, signature }
        });

        await SendAsync(authMessage, ct);
        _logger.LogInformation("Bybit WebSocket authentication sent");

        _ = Task.Run(() => ReceiveLoopAsync(_cts.Token), _cts.Token);
    }

    /// <summary>Subscribes to execution and order updates.</summary>
    public async Task SubscribeAsync(CancellationToken ct)
    {
        var subscribeMessage = JsonSerializer.Serialize(new
        {
            op = "subscribe",
            args = new[] { "execution", "order" }
        });
        await SendAsync(subscribeMessage, ct);
        _logger.LogInformation("Subscribed to Bybit execution and order updates");
    }

    private async Task SendAsync(string message, CancellationToken ct)
    {
        if (_webSocket?.State != WebSocketState.Open) return;
        var bytes = Encoding.UTF8.GetBytes(message);
        await _webSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, ct);
    }

    private async Task ReceiveLoopAsync(CancellationToken ct)
    {
        var buffer = new byte[4096];
        while (!ct.IsCancellationRequested && _webSocket?.State == WebSocketState.Open)
        {
            try
            {
                var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), ct);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    _logger.LogWarning("Bybit WebSocket closed by server");
                    break;
                }

                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                HandleMessage(message);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (WebSocketException ex)
            {
                _logger.LogError(ex, "Bybit WebSocket error. Attempting reconnect...");
                await ReconnectWithBackoffAsync(ct);
            }
        }
    }

    private void HandleMessage(string message)
    {
        try
        {
            var doc = JsonDocument.Parse(message);
            var root = doc.RootElement;

            if (root.TryGetProperty("op", out var op) && op.GetString() == "pong")
                return;

            if (root.TryGetProperty("topic", out var topic))
            {
                var topicStr = topic.GetString();
                if (topicStr == "execution" || topicStr == "order")
                {
                    _logger.LogDebug("Bybit {Topic} update received", topicStr);
                    OnExecutionUpdate?.Invoke(new PerformanceSnapshot { Timestamp = DateTimeOffset.UtcNow });
                }
            }
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to parse Bybit WebSocket message");
        }
    }

    private async Task ReconnectWithBackoffAsync(CancellationToken ct)
    {
        var delays = new[] { 1000, 2000, 4000, 8000, 16000 };
        foreach (var delay in delays)
        {
            if (ct.IsCancellationRequested) return;
            _logger.LogInformation("Attempting Bybit WebSocket reconnect in {Delay}ms", delay);
            await Task.Delay(delay, ct);
            try
            {
                _webSocket?.Dispose();
                _webSocket = new ClientWebSocket();
                await _webSocket.ConnectAsync(new Uri(_config.WebSocketUrl), ct);
                _logger.LogInformation("Bybit WebSocket reconnected successfully");
                return;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Bybit WebSocket reconnect attempt failed");
            }
        }
        _logger.LogError("Bybit WebSocket reconnection failed after all attempts");
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;
        _cts?.Cancel();
        if (_webSocket?.State == WebSocketState.Open)
        {
            try
            {
                await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Disposing", CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error closing Bybit WebSocket");
            }
        }
        _webSocket?.Dispose();
        _cts?.Dispose();
        _logger.LogInformation("Bybit WebSocket client disposed");
    }
}
