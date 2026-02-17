namespace RivrQuant.Infrastructure.Brokers.Alpaca;

using Alpaca.Markets;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RivrQuant.Domain.Models.Trading;

/// <summary>Manages real-time WebSocket streaming from Alpaca Markets.</summary>
public sealed class AlpacaStreamClient : IAsyncDisposable
{
    private readonly AlpacaConfiguration _config;
    private readonly ILogger<AlpacaStreamClient> _logger;
    private IAlpacaStreamingClient? _streamingClient;
    private bool _disposed;

    /// <summary>Initializes a new instance of <see cref="AlpacaStreamClient"/>.</summary>
    public AlpacaStreamClient(IOptions<AlpacaConfiguration> config, ILogger<AlpacaStreamClient> logger)
    {
        _config = config.Value;
        _logger = logger;
    }

    /// <summary>Connects to the Alpaca streaming API and subscribes to trade updates.</summary>
    public async Task ConnectAndSubscribeAsync(Action<PerformanceSnapshot> onUpdate, CancellationToken ct)
    {
        _logger.LogInformation("Connecting to Alpaca streaming API. Paper={IsPaper}", _config.IsPaper);

        var environment = _config.IsPaper ? Environments.Paper : Environments.Live;
        _streamingClient = environment.GetAlpacaStreamingClient(new SecretKey(_config.ApiKey, _config.ApiSecret));

        _streamingClient.OnTradeUpdate += args =>
        {
            _logger.LogDebug("Alpaca trade update: {Symbol} {Event}", args.Order.Symbol, args.Event);
            var snapshot = new PerformanceSnapshot
            {
                Timestamp = DateTimeOffset.UtcNow
            };
            onUpdate(snapshot);
        };

        var authStatus = await _streamingClient.ConnectAndAuthenticateAsync(ct);
        if (authStatus != AuthStatus.Authorized)
        {
            _logger.LogError("Alpaca streaming authentication failed: {Status}", authStatus);
            return;
        }

        _logger.LogInformation("Connected to Alpaca streaming API successfully");
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;

        if (_streamingClient is not null)
        {
            await _streamingClient.DisconnectAsync(CancellationToken.None);
            _streamingClient.Dispose();
            _logger.LogInformation("Alpaca streaming client disconnected and disposed");
        }
    }
}
