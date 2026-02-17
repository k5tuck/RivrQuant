namespace RivrQuant.Infrastructure.Brokers.Bybit;

/// <summary>Configuration for Bybit API v5.</summary>
public sealed class BybitConfiguration
{
    /// <summary>Configuration section name.</summary>
    public const string SectionName = "Bybit";

    /// <summary>Bybit API key.</summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>Bybit API secret.</summary>
    public string ApiSecret { get; set; } = string.Empty;

    /// <summary>Whether to use testnet.</summary>
    public bool UseTestnet { get; set; } = true;

    /// <summary>Testnet API URL.</summary>
    public string TestnetUrl { get; init; } = "https://api-testnet.bybit.com";

    /// <summary>Live API URL.</summary>
    public string LiveUrl { get; init; } = "https://api.bybit.com";

    /// <summary>Request freshness window in milliseconds.</summary>
    public int RecvWindow { get; init; } = 5000;

    /// <summary>Effective base URL based on testnet/live mode.</summary>
    public string BaseUrl => UseTestnet ? TestnetUrl : LiveUrl;

    /// <summary>WebSocket URL for streaming.</summary>
    public string WebSocketUrl => UseTestnet
        ? "wss://stream-testnet.bybit.com/v5/private"
        : "wss://stream.bybit.com/v5/private";

    /// <summary>Validates that required configuration values are present.</summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(ApiKey))
            throw new InvalidOperationException("BYBIT_API_KEY is required. Set it in environment variables or appsettings.");
        if (string.IsNullOrWhiteSpace(ApiSecret))
            throw new InvalidOperationException("BYBIT_API_SECRET is required. Set it in environment variables or appsettings.");
    }
}
