namespace RivrQuant.Infrastructure.Brokers.Alpaca;

/// <summary>Configuration for Alpaca Markets API.</summary>
public sealed class AlpacaConfiguration
{
    /// <summary>Configuration section name.</summary>
    public const string SectionName = "Alpaca";

    /// <summary>Alpaca API key.</summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>Alpaca API secret.</summary>
    public string ApiSecret { get; set; } = string.Empty;

    /// <summary>Whether to use paper trading.</summary>
    public bool IsPaper { get; set; } = true;

    /// <summary>Base URL for the Alpaca API.</summary>
    public string BaseUrl { get; set; } = "https://paper-api.alpaca.markets";

    /// <summary>Whether extended hours trading is enabled.</summary>
    public bool ExtendedHoursEnabled { get; init; }

    /// <summary>Validates that required configuration values are present.</summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(ApiKey))
            throw new InvalidOperationException("ALPACA_API_KEY is required. Set it in environment variables or appsettings.");
        if (string.IsNullOrWhiteSpace(ApiSecret))
            throw new InvalidOperationException("ALPACA_API_SECRET is required. Set it in environment variables or appsettings.");
    }
}
