using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RivrQuant.Application.DTOs;
using RivrQuant.Domain.Interfaces;
using RivrQuant.Domain.Models.Analysis;
using RivrQuant.Domain.Models.Backtests;
using RivrQuant.Infrastructure.Analysis;
using RivrQuant.Infrastructure.Persistence;

namespace RivrQuant.Application.Services;

/// <summary>
/// Application service for managing backtest results, computing metrics,
/// and triggering AI analysis.
/// </summary>
public sealed class BacktestService
{
    private readonly RivrQuantDbContext _db;
    private readonly IStatisticsEngine _statistics;
    private readonly IAiAnalyzer _aiAnalyzer;
    private readonly RegimeDetector _regimeDetector;
    private readonly ILogger<BacktestService> _logger;

    /// <summary>Initializes a new instance of <see cref="BacktestService"/>.</summary>
    public BacktestService(
        RivrQuantDbContext db,
        IStatisticsEngine statistics,
        IAiAnalyzer aiAnalyzer,
        RegimeDetector regimeDetector,
        ILogger<BacktestService> logger)
    {
        _db = db;
        _statistics = statistics;
        _aiAnalyzer = aiAnalyzer;
        _regimeDetector = regimeDetector;
        _logger = logger;
    }

    /// <summary>Lists all backtest results as summaries.</summary>
    public async Task<IReadOnlyList<BacktestSummaryDto>> GetAllAsync(CancellationToken ct)
    {
        var results = (await _db.BacktestResults
            .Include(b => b.Metrics)
            .Include(b => b.Trades)
            .ToListAsync(ct))
            .OrderByDescending(b => b.CreatedAt)
            .ToList();

        return results.Select(b => new BacktestSummaryDto
        {
            Id = b.Id,
            ExternalBacktestId = b.ExternalBacktestId,
            ProjectId = b.ProjectId,
            ProjectName = b.ProjectName,
            StrategyName = b.StrategyName,
            StartDate = b.StartDate,
            EndDate = b.EndDate,
            TotalReturn = b.TotalReturn,
            FinalEquity = b.FinalEquity,
            IsAnalyzed = b.IsAnalyzed,
            SharpeRatio = b.Metrics?.SharpeRatio,
            MaxDrawdown = b.Metrics?.MaxDrawdown,
            TradeCount = b.Trades.Count,
            CreatedAt = b.CreatedAt
        }).ToList();
    }

    /// <summary>
    /// Groups all backtests by QuantConnect project and returns one summary row
    /// per distinct project (algorithm/bot).
    /// </summary>
    public async Task<IReadOnlyList<ProjectSummaryDto>> GetProjectSummariesAsync(CancellationToken ct)
    {
        var backtests = await _db.BacktestResults
            .Include(b => b.Metrics)
            .ToListAsync(ct);

        return backtests
            .GroupBy(b => b.ProjectId)
            .Select(g => new ProjectSummaryDto
            {
                ProjectId = g.Key,
                ProjectName = g.Where(b => b.ProjectName != null)
                               .Select(b => b.ProjectName!)
                               .FirstOrDefault() ?? g.Key,
                BacktestCount = g.Count(),
                AnalyzedCount = g.Count(b => b.IsAnalyzed),
                BestSharpe = g.Max(b => b.Metrics != null ? b.Metrics.SharpeRatio : (double?)null),
                BestTotalReturn = g.Max(b => b.TotalReturn),
                LatestBacktest = g.Max(b => b.CreatedAt)
            })
            .OrderByDescending(p => p.LatestBacktest)
            .ToList();
    }

    /// <summary>Retrieves a backtest with full detail.</summary>
    public async Task<BacktestDetailDto?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var b = await _db.BacktestResults
            .Include(x => x.Metrics)
            .Include(x => x.Trades)
            .Include(x => x.DailyReturns)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (b is null) return null;

        var report = await _db.AiAnalysisReports
            .FirstOrDefaultAsync(r => r.BacktestResultId == id, ct);

        return new BacktestDetailDto
        {
            Id = b.Id,
            ExternalBacktestId = b.ExternalBacktestId,
            ProjectId = b.ProjectId,
            StrategyName = b.StrategyName,
            StrategyDescription = b.StrategyDescription,
            StartDate = b.StartDate,
            EndDate = b.EndDate,
            InitialCapital = b.InitialCapital,
            FinalEquity = b.FinalEquity,
            TotalReturn = b.TotalReturn,
            IsAnalyzed = b.IsAnalyzed,
            Metrics = b.Metrics,
            Trades = b.Trades.ToList(),
            DailyReturns = b.DailyReturns.ToList(),
            AnalysisReport = report,
            CreatedAt = b.CreatedAt
        };
    }

    /// <summary>
    /// Runs full analysis pipeline on a backtest: compute metrics, detect regimes,
    /// and generate AI analysis report.
    /// </summary>
    public async Task<AiAnalysisReport> AnalyzeAsync(Guid backtestId, CancellationToken ct)
    {
        var backtest = await _db.BacktestResults
            .Include(b => b.Trades)
            .Include(b => b.DailyReturns)
            .Include(b => b.Metrics)
            .FirstOrDefaultAsync(b => b.Id == backtestId, ct)
            ?? throw new KeyNotFoundException($"Backtest {backtestId} not found.");

        _logger.LogInformation("Starting analysis pipeline for backtest {BacktestId}", backtestId);

        // Compute metrics if not already done
        if (backtest.Metrics is null)
        {
            _logger.LogInformation("Computing metrics for backtest {BacktestId}", backtestId);
            var metrics = _statistics.CalculateFullMetrics(
                backtest.DailyReturns.ToList(),
                backtest.Trades.ToList(),
                0.05);
            var metricsEntity = new BacktestMetrics
            {
                Id = metrics.Id,
                BacktestResultId = backtestId,
                SharpeRatio = metrics.SharpeRatio,
                SortinoRatio = metrics.SortinoRatio,
                MaxDrawdown = metrics.MaxDrawdown,
                WinRate = metrics.WinRate,
                ProfitFactor = metrics.ProfitFactor,
                CalmarRatio = metrics.CalmarRatio,
                ValueAtRisk95 = metrics.ValueAtRisk95,
                ExpectedShortfall95 = metrics.ExpectedShortfall95,
                Beta = metrics.Beta,
                AnnualizedReturn = metrics.AnnualizedReturn,
                AnnualizedVolatility = metrics.AnnualizedVolatility,
                TotalTrades = metrics.TotalTrades,
                WinningTrades = metrics.WinningTrades,
                LosingTrades = metrics.LosingTrades,
                AverageWin = metrics.AverageWin,
                AverageLoss = metrics.AverageLoss,
                LargestWin = metrics.LargestWin,
                LargestLoss = metrics.LargestLoss,
                AverageHoldingPeriod = metrics.AverageHoldingPeriod
            };
            _db.BacktestMetrics.Add(metricsEntity);
            backtest.Metrics = metricsEntity;
            await _db.SaveChangesAsync(ct);
        }

        // Detect regimes
        var regimes = _regimeDetector.DetectRegimes(
            backtest.DailyReturns.OrderBy(d => d.Date).ToList(), backtestId);

        // Save regime classifications
        foreach (var regime in regimes)
        {
            var existing = await _db.RegimeClassifications
                .AnyAsync(r => r.BacktestResultId == backtestId
                    && r.StartDate == regime.StartDate
                    && r.EndDate == regime.EndDate, ct);
            if (!existing)
            {
                _db.RegimeClassifications.Add(new RegimeClassification
                {
                    BacktestResultId = backtestId,
                    Regime = regime.Regime,
                    StartDate = regime.StartDate,
                    EndDate = regime.EndDate,
                    DurationDays = regime.DurationDays,
                    AnnualizedReturn = regime.AnnualizedReturn,
                    SharpeRatio = regime.SharpeRatio,
                    MaxDrawdown = regime.MaxDrawdown,
                    Volatility = regime.Volatility,
                    TradeCount = regime.TradeCount,
                    WinRate = regime.WinRate
                });
            }
        }
        await _db.SaveChangesAsync(ct);

        // Run AI analysis
        _logger.LogInformation("Running AI analysis for backtest {BacktestId}", backtestId);
        var report = await _aiAnalyzer.AnalyzeBacktestAsync(backtest, backtest.Metrics!, regimes, ct);
        _db.AiAnalysisReports.Add(report);
        backtest.IsAnalyzed = true;
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Analysis complete for backtest {BacktestId}. Deployment readiness: {Score}",
            backtestId, report.DeploymentReadiness);

        return report;
    }

    /// <summary>Compares multiple backtests.</summary>
    public async Task<BacktestComparison> CompareAsync(IReadOnlyList<Guid> backtestIds, CancellationToken ct)
    {
        var backtests = new List<BacktestResult>();
        var metrics = new List<BacktestMetrics>();

        foreach (var id in backtestIds)
        {
            var b = await _db.BacktestResults
                .Include(x => x.Metrics)
                .Include(x => x.Trades)
                .Include(x => x.DailyReturns)
                .FirstOrDefaultAsync(x => x.Id == id, ct)
                ?? throw new KeyNotFoundException($"Backtest {id} not found.");

            backtests.Add(b);

            if (b.Metrics is null)
            {
                var m = _statistics.CalculateFullMetrics(
                    b.DailyReturns.ToList(),
                    b.Trades.ToList(),
                    0.05);
                _db.BacktestMetrics.Add(m);
                b.Metrics = m;
                await _db.SaveChangesAsync(ct);
            }

            metrics.Add(b.Metrics!);
        }

        var comparisonReport = await _aiAnalyzer.CompareBacktestsAsync(backtests, metrics, ct);

        return new BacktestComparison
        {
            Backtests = backtests,
            MetricsComparison = metrics,
            AiComparisonSummary = comparisonReport.OverallAssessment
        };
    }
}
