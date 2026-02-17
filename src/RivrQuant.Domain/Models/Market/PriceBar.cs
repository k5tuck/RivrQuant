namespace RivrQuant.Domain.Models.Market;

/// <summary>Represents an OHLCV price bar for a given symbol and timestamp.</summary>
public class PriceBar
{
    /// <summary>Ticker symbol.</summary>
    public string Symbol { get; init; } = string.Empty;

    /// <summary>Timestamp of the bar.</summary>
    public DateTimeOffset Timestamp { get; init; }

    /// <summary>Opening price.</summary>
    public decimal Open { get; init; }

    /// <summary>Highest price during the bar.</summary>
    public decimal High { get; init; }

    /// <summary>Lowest price during the bar.</summary>
    public decimal Low { get; init; }

    /// <summary>Closing price.</summary>
    public decimal Close { get; init; }

    /// <summary>Trading volume during the bar.</summary>
    public decimal Volume { get; init; }
}
