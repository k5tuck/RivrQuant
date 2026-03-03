namespace RivrQuant.Application.Services;

using Microsoft.Extensions.Logging;
using RivrQuant.Domain.Interfaces;
using RivrQuant.Domain.Models.Exposure;

/// <summary>Coordinates portfolio exposure tracking, correlation analysis, and limit enforcement.</summary>
public sealed class ExposureService
{
    private readonly IExposureTracker _exposureTracker;
    private readonly ILogger<ExposureService> _logger;

    // Configurable limits (defaults from prompt spec)
    private const decimal MaxNetExposurePercent = 150m;
    private const decimal MaxGrossExposurePercent = 200m;
    private const decimal MaxSingleAssetPercentStock = 20m;
    private const decimal MaxSingleAssetPercentCrypto = 10m;
    private const decimal MaxSectorPercent = 40m;
    private const decimal MaxCryptoPercent = 30m;
    private const decimal MaxPortfolioBeta = 1.5m;

    public ExposureService(
        IExposureTracker exposureTracker,
        ILogger<ExposureService> logger)
    {
        _exposureTracker = exposureTracker;
        _logger = logger;
    }

    /// <summary>Gets the current portfolio exposure breakdown.</summary>
    public async Task<PortfolioExposure> GetCurrentExposureAsync(CancellationToken ct)
    {
        var exposure = await _exposureTracker.GetCurrentExposureAsync(ct);
        CheckLimits(exposure);
        return exposure;
    }

    /// <summary>Gets the rolling cross-asset correlation matrix.</summary>
    public Task<CorrelationSnapshot> GetCorrelationMatrixAsync(int lookbackDays, CancellationToken ct)
        => _exposureTracker.GetCorrelationMatrixAsync(lookbackDays, ct);

    /// <summary>Snapshot current exposure to database.</summary>
    public Task<PortfolioExposure> SnapshotAsync(CancellationToken ct)
        => _exposureTracker.SnapshotAsync(ct);

    /// <summary>Validates whether a new order would breach exposure limits.</summary>
    public async Task<(bool IsAllowed, string? BlockReason)> ValidateOrderExposureAsync(
        string symbol,
        decimal notionalValue,
        bool isLong,
        CancellationToken ct)
    {
        var exposure = await _exposureTracker.GetCurrentExposureAsync(ct);

        if (Math.Abs(exposure.NetExposurePercent) > MaxNetExposurePercent)
            return (false, $"Net exposure {exposure.NetExposurePercent:F1}% exceeds limit {MaxNetExposurePercent}%");

        if (exposure.GrossExposurePercent > MaxGrossExposurePercent)
            return (false, $"Gross exposure {exposure.GrossExposurePercent:F1}% exceeds limit {MaxGrossExposurePercent}%");

        if (exposure.CryptoExposurePercent > MaxCryptoPercent)
            return (false, $"Crypto exposure {exposure.CryptoExposurePercent:F1}% exceeds limit {MaxCryptoPercent}%");

        if (exposure.PortfolioBeta > MaxPortfolioBeta)
            return (false, $"Portfolio beta {exposure.PortfolioBeta:F2} exceeds limit {MaxPortfolioBeta}");

        return (true, null);
    }

    private void CheckLimits(PortfolioExposure exposure)
    {
        if (Math.Abs(exposure.NetExposurePercent) > MaxNetExposurePercent)
            _logger.LogWarning("Net exposure {Pct:F1}% exceeds limit {Limit}%", exposure.NetExposurePercent, MaxNetExposurePercent);

        if (exposure.GrossExposurePercent > MaxGrossExposurePercent)
            _logger.LogWarning("Gross exposure {Pct:F1}% exceeds limit {Limit}%", exposure.GrossExposurePercent, MaxGrossExposurePercent);

        if (exposure.PortfolioBeta > MaxPortfolioBeta)
            _logger.LogWarning("Portfolio beta {Beta:F2} exceeds limit {Limit}", exposure.PortfolioBeta, MaxPortfolioBeta);

        if (exposure.CryptoExposurePercent > MaxCryptoPercent)
            _logger.LogWarning("Crypto exposure {Pct:F1}% exceeds limit {Limit}%", exposure.CryptoExposurePercent, MaxCryptoPercent);
    }
}
