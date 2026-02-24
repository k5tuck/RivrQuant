using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RivrQuant.Application.DTOs;
using RivrQuant.Domain.Models.Analysis;
using RivrQuant.Infrastructure.Persistence;

namespace RivrQuant.Application.Services;

/// <summary>
/// Application service for managing AI analysis reports.
/// </summary>
public sealed class AnalysisService
{
    private readonly RivrQuantDbContext _db;
    private readonly BacktestService _backtestService;
    private readonly ILogger<AnalysisService> _logger;

    /// <summary>Initializes a new instance of <see cref="AnalysisService"/>.</summary>
    public AnalysisService(
        RivrQuantDbContext db,
        BacktestService backtestService,
        ILogger<AnalysisService> logger)
    {
        _db = db;
        _backtestService = backtestService;
        _logger = logger;
    }

    /// <summary>Lists all analysis reports.</summary>
    public async Task<IReadOnlyList<AnalysisReportDto>> GetAllAsync(CancellationToken ct)
    {
        var reports = (await _db.AiAnalysisReports.ToListAsync(ct))
            .OrderByDescending(r => r.CreatedAt)
            .ToList();

        var backtestIds = reports.Select(r => r.BacktestResultId).Distinct().ToList();
        var backtests = await _db.BacktestResults
            .Where(b => backtestIds.Contains(b.Id))
            .ToDictionaryAsync(b => b.Id, ct);

        return reports.Select(r => MapToDto(r, backtests.GetValueOrDefault(r.BacktestResultId)?.StrategyName ?? "Unknown")).ToList();
    }

    /// <summary>Retrieves a single analysis report.</summary>
    public async Task<AnalysisReportDto?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var report = await _db.AiAnalysisReports.FindAsync(new object[] { id }, ct);
        if (report is null) return null;

        var backtest = await _db.BacktestResults.FindAsync(new object[] { report.BacktestResultId }, ct);
        return MapToDto(report, backtest?.StrategyName ?? "Unknown");
    }

    /// <summary>Runs on-demand analysis for a backtest.</summary>
    public async Task<AnalysisReportDto> RunAnalysisAsync(Guid backtestId, CancellationToken ct)
    {
        _logger.LogInformation("Running on-demand analysis for backtest {BacktestId}", backtestId);
        var report = await _backtestService.AnalyzeAsync(backtestId, ct);
        var backtest = await _db.BacktestResults.FindAsync(new object[] { backtestId }, ct);
        return MapToDto(report, backtest?.StrategyName ?? "Unknown");
    }

    private static AnalysisReportDto MapToDto(AiAnalysisReport report, string strategyName)
    {
        return new AnalysisReportDto
        {
            Id = report.Id,
            BacktestResultId = report.BacktestResultId,
            StrategyName = strategyName,
            OverallAssessment = report.OverallAssessment,
            Strengths = report.Strengths,
            Weaknesses = report.Weaknesses,
            OverfittingRisk = report.OverfittingRisk,
            OverfittingExplanation = report.OverfittingExplanation,
            ParameterSuggestions = report.ParameterSuggestions,
            RegimeAnalysis = report.RegimeAnalysis,
            DeploymentReadiness = report.DeploymentReadiness,
            PlainEnglishSummary = report.PlainEnglishSummary,
            CriticalWarnings = report.CriticalWarnings,
            TokensUsed = report.TokensUsed,
            EstimatedCost = report.EstimatedCost,
            CreatedAt = report.CreatedAt
        };
    }
}
