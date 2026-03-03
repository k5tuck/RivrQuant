using System.Text.Json;

namespace RivrQuant.Domain.Models.Exposure;

/// <summary>
/// Represents a point-in-time snapshot of portfolio-level exposure metrics,
/// including net/gross exposure, beta, asset class breakdown, and sector breakdown.
/// Persisted to the database as periodic snapshots for trend analysis and compliance monitoring.
/// </summary>
public class PortfolioExposure
{
    /// <summary>
    /// Unique internal identifier for the exposure snapshot.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Total portfolio value including all positions and cash at the time of the snapshot.
    /// </summary>
    public decimal TotalPortfolioValue { get; init; }

    /// <summary>
    /// Net exposure in dollar terms (long notional minus short notional).
    /// </summary>
    public decimal NetExposureDollars { get; init; }

    /// <summary>
    /// Net exposure as a percentage of total portfolio value.
    /// </summary>
    public decimal NetExposurePercent { get; init; }

    /// <summary>
    /// Gross exposure in dollar terms (sum of absolute long and short notional values).
    /// </summary>
    public decimal GrossExposureDollars { get; init; }

    /// <summary>
    /// Gross exposure as a percentage of total portfolio value.
    /// </summary>
    public decimal GrossExposurePercent { get; init; }

    /// <summary>
    /// Beta-weighted exposure of the portfolio relative to the market benchmark.
    /// </summary>
    public decimal PortfolioBeta { get; init; }

    /// <summary>
    /// Percentage of portfolio value allocated to stock (equity) positions.
    /// </summary>
    public decimal StockExposurePercent { get; init; }

    /// <summary>
    /// Percentage of portfolio value allocated to cryptocurrency positions.
    /// </summary>
    public decimal CryptoExposurePercent { get; init; }

    /// <summary>
    /// Percentage of portfolio value held in cash.
    /// </summary>
    public decimal CashPercent { get; init; }

    /// <summary>
    /// JSON-serialized breakdown of per-asset exposure details.
    /// Use <see cref="GetAssetBreakdown"/> and <see cref="SetAssetBreakdown"/> for typed access.
    /// </summary>
    public string AssetBreakdownJson { get; set; } = "[]";

    /// <summary>
    /// JSON-serialized breakdown of per-sector exposure details.
    /// Use <see cref="GetSectorBreakdown"/> and <see cref="SetSectorBreakdown"/> for typed access.
    /// </summary>
    public string SectorBreakdownJson { get; set; } = "[]";

    /// <summary>
    /// Timestamp when this exposure snapshot was taken.
    /// </summary>
    public DateTimeOffset SnapshotAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Deserializes the <see cref="AssetBreakdownJson"/> property into a typed list of
    /// <see cref="AssetExposure"/> records. Returns an empty list if the JSON is null or empty.
    /// </summary>
    /// <returns>A list of per-asset exposure records.</returns>
    public IReadOnlyList<AssetExposure> GetAssetBreakdown()
    {
        if (string.IsNullOrWhiteSpace(AssetBreakdownJson))
            return Array.Empty<AssetExposure>();

        return JsonSerializer.Deserialize<List<AssetExposure>>(AssetBreakdownJson) ?? new List<AssetExposure>();
    }

    /// <summary>
    /// Serializes the provided list of <see cref="AssetExposure"/> records into the
    /// <see cref="AssetBreakdownJson"/> property.
    /// </summary>
    /// <param name="breakdown">The list of per-asset exposure records to serialize.</param>
    public void SetAssetBreakdown(IReadOnlyList<AssetExposure> breakdown)
    {
        AssetBreakdownJson = JsonSerializer.Serialize(breakdown);
    }

    /// <summary>
    /// Deserializes the <see cref="SectorBreakdownJson"/> property into a typed list of
    /// <see cref="SectorExposure"/> records. Returns an empty list if the JSON is null or empty.
    /// </summary>
    /// <returns>A list of per-sector exposure records.</returns>
    public IReadOnlyList<SectorExposure> GetSectorBreakdown()
    {
        if (string.IsNullOrWhiteSpace(SectorBreakdownJson))
            return Array.Empty<SectorExposure>();

        return JsonSerializer.Deserialize<List<SectorExposure>>(SectorBreakdownJson) ?? new List<SectorExposure>();
    }

    /// <summary>
    /// Serializes the provided list of <see cref="SectorExposure"/> records into the
    /// <see cref="SectorBreakdownJson"/> property.
    /// </summary>
    /// <param name="breakdown">The list of per-sector exposure records to serialize.</param>
    public void SetSectorBreakdown(IReadOnlyList<SectorExposure> breakdown)
    {
        SectorBreakdownJson = JsonSerializer.Serialize(breakdown);
    }
}
