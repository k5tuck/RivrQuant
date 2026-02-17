namespace RivrQuant.Domain.Models.Market;

/// <summary>Represents a tradeable asset (stock or crypto).</summary>
public class Asset
{
    /// <summary>Ticker symbol (e.g., AAPL, BTCUSDT).</summary>
    public string Symbol { get; init; } = string.Empty;

    /// <summary>Full name of the asset.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Asset class (Stock or Crypto).</summary>
    public Enums.AssetClass AssetClass { get; init; }

    /// <summary>Broker that provides this asset.</summary>
    public Enums.BrokerType Broker { get; init; }

    /// <summary>Exchange where the asset is traded.</summary>
    public string? Exchange { get; init; }

    /// <summary>Whether the asset is currently tradeable.</summary>
    public bool IsTradeable { get; init; } = true;

    /// <summary>Minimum order size.</summary>
    public decimal? MinOrderSize { get; init; }

    /// <summary>Maximum order size.</summary>
    public decimal? MaxOrderSize { get; init; }

    /// <summary>Number of decimal places for price.</summary>
    public int PriceDecimals { get; init; } = 2;

    /// <summary>Number of decimal places for quantity.</summary>
    public int QuantityDecimals { get; init; }
}
