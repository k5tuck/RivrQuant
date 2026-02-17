namespace RivrQuant.Domain.Models.Market;

/// <summary>Represents a market regime with associated metadata.</summary>
public class MarketRegime
{
    /// <summary>The type of market regime.</summary>
    public Enums.RegimeType Type { get; init; }

    /// <summary>Human-readable description of the regime.</summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>VIX level at the time of detection.</summary>
    public double VixLevel { get; init; }

    /// <summary>Strength of the detected trend (0 = no trend, 1 = strong trend).</summary>
    public double TrendStrength { get; init; }

    /// <summary>When this regime was detected.</summary>
    public DateTimeOffset DetectedAt { get; init; } = DateTimeOffset.UtcNow;
}
