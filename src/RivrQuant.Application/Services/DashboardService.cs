using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RivrQuant.Application.DTOs;
using RivrQuant.Domain.Interfaces;
using RivrQuant.Domain.Models.Trading;
using RivrQuant.Infrastructure.Persistence;

namespace RivrQuant.Application.Services;

/// <summary>
/// Application service providing aggregated dashboard data for the frontend.
/// </summary>
public sealed class DashboardService
{
    private readonly RivrQuantDbContext _db;
    private readonly IPortfolioTracker _portfolioTracker;
    private readonly TradingService _tradingService;
    private readonly ILogger<DashboardService> _logger;

    /// <summary>Initializes a new instance of <see cref="DashboardService"/>.</summary>
    public DashboardService(
        RivrQuantDbContext db,
        IPortfolioTracker portfolioTracker,
        TradingService tradingService,
        ILogger<DashboardService> logger)
    {
        _db = db;
        _portfolioTracker = portfolioTracker;
        _tradingService = tradingService;
        _logger = logger;
    }

    /// <summary>Retrieves the full aggregated dashboard.</summary>
    public async Task<DashboardDto> GetDashboardAsync(CancellationToken ct)
    {
        Portfolio? portfolio = null;
        try
        {
            portfolio = await _portfolioTracker.GetAggregatePortfolioAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch aggregate portfolio for dashboard");
        }

        var positions = await _tradingService.GetAllPositionsAsync(ct);

        var snapshots = await _db.PerformanceSnapshots
            .OrderByDescending(s => s.Timestamp)
            .Take(100)
            .ToListAsync(ct);

        var totalBacktests = await _db.BacktestResults.CountAsync(ct);
        var analyzedBacktests = await _db.BacktestResults.CountAsync(b => b.IsAnalyzed, ct);
        var activeStrategies = await _db.Strategies.CountAsync(s => s.IsActive, ct);
        var unacknowledgedAlerts = await _db.AlertEvents.CountAsync(e => !e.IsAcknowledged, ct);

        return new DashboardDto
        {
            Portfolio = portfolio,
            Positions = positions,
            RecentSnapshots = snapshots,
            TotalBacktests = totalBacktests,
            AnalyzedBacktests = analyzedBacktests,
            ActiveStrategies = activeStrategies,
            UnacknowledgedAlerts = unacknowledgedAlerts
        };
    }

    /// <summary>Retrieves portfolio data only.</summary>
    public async Task<Portfolio?> GetPortfolioAsync(CancellationToken ct)
    {
        try
        {
            return await _portfolioTracker.GetAggregatePortfolioAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch aggregate portfolio");
            return null;
        }
    }

    /// <summary>Retrieves performance metrics only.</summary>
    public async Task<IReadOnlyList<PerformanceSnapshot>> GetMetricsAsync(CancellationToken ct)
    {
        return await _db.PerformanceSnapshots
            .OrderByDescending(s => s.Timestamp)
            .Take(100)
            .ToListAsync(ct);
    }
}
