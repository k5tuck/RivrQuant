namespace RivrQuant.Application.Services;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RivrQuant.Domain.Interfaces;
using RivrQuant.Domain.Models.Allocation;
using RivrQuant.Domain.Models.Strategies;
using RivrQuant.Infrastructure.Persistence;

/// <summary>Coordinates capital allocation across active strategies.</summary>
public sealed class AllocationService
{
    private readonly ICapitalAllocator _allocator;
    private readonly IPortfolioTracker _portfolioTracker;
    private readonly RivrQuantDbContext _db;
    private readonly ILogger<AllocationService> _logger;

    public AllocationService(
        ICapitalAllocator allocator,
        IPortfolioTracker portfolioTracker,
        RivrQuantDbContext db,
        ILogger<AllocationService> logger)
    {
        _allocator = allocator;
        _portfolioTracker = portfolioTracker;
        _db = db;
        _logger = logger;
    }

    /// <summary>Calculate and return current capital allocations for all active strategies.</summary>
    public async Task<IReadOnlyList<StrategyAllocation>> GetAllocationsAsync(CancellationToken ct)
    {
        var portfolio = await _portfolioTracker.GetAggregatePortfolioAsync(ct);
        var strategies = await _db.Strategies
            .Where(s => s.IsActive)
            .ToListAsync(ct);

        if (strategies.Count == 0)
        {
            _logger.LogDebug("No active strategies for allocation");
            return Array.Empty<StrategyAllocation>();
        }

        var allocations = await _allocator.AllocateAsync(portfolio.TotalEquity, strategies, ct);

        _logger.LogInformation(
            "Allocated {Capital:C} across {Count} strategies",
            portfolio.TotalEquity, strategies.Count);

        return allocations;
    }

    /// <summary>Evaluate whether rebalancing is needed based on drift from target allocations.</summary>
    public async Task<AllocationDecision> EvaluateRebalanceAsync(CancellationToken ct)
    {
        var decision = await _allocator.EvaluateRebalanceAsync(ct);

        if (decision.RebalanceNeeded)
        {
            _logger.LogInformation(
                "Rebalance recommended: max drift {Drift:F1}%. Reason: {Reason}",
                decision.MaxDriftPercent, decision.DriftReason);
        }

        return decision;
    }

    /// <summary>Gets strategy performance rankings for allocation decisions.</summary>
    public async Task<IReadOnlyList<StrategyPerformanceRank>> GetStrategyRankingsAsync(CancellationToken ct)
    {
        var strategies = await _db.Strategies
            .Where(s => s.IsActive)
            .ToListAsync(ct);

        var snapshots = await _db.PerformanceSnapshots
            .OrderByDescending(s => s.Timestamp)
            .Take(60)
            .ToListAsync(ct);

        var dailyReturns = snapshots
            .OrderBy(s => s.Timestamp)
            .Select(s => (double)s.DailyReturnPercent)
            .ToList();

        var rankings = new List<StrategyPerformanceRank>();
        var rank = 1;

        foreach (var strategy in strategies)
        {
            // Use portfolio-level returns as proxy since per-strategy attribution isn't available yet
            var sharpe30 = dailyReturns.Count >= 30
                ? CalculateRollingSharpe(dailyReturns.TakeLast(30).ToList())
                : 0.0;
            var sharpe60 = dailyReturns.Count >= 60
                ? CalculateRollingSharpe(dailyReturns.TakeLast(60).ToList())
                : 0.0;

            rankings.Add(new StrategyPerformanceRank
            {
                StrategyId = strategy.Id,
                StrategyName = strategy.Name,
                RollingSharpe30d = sharpe30,
                RollingSharpe60d = sharpe60,
                Rank = rank++,
                IsActive = strategy.IsActive,
                RankedAt = DateTimeOffset.UtcNow
            });
        }

        return rankings.OrderByDescending(r => r.RollingSharpe30d).ToList();
    }

    private static double CalculateRollingSharpe(IReadOnlyList<double> returns)
    {
        if (returns.Count < 2) return 0;
        var mean = returns.Average();
        var variance = returns.Select(r => (r - mean) * (r - mean)).Average();
        var stdDev = Math.Sqrt(variance);
        return stdDev == 0 ? 0 : (mean / stdDev) * Math.Sqrt(252);
    }
}
