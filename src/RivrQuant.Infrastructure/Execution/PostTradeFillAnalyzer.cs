using MathNet.Numerics.Statistics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RivrQuant.Domain.Interfaces;
using RivrQuant.Domain.Models.Execution;
using RivrQuant.Infrastructure.Persistence;

namespace RivrQuant.Infrastructure.Execution;

/// <summary>
/// Post-trade fill analysis engine that compares actual execution quality against
/// pre-trade estimates. Implements <see cref="IFillAnalyzer"/>.
/// </summary>
/// <remarks>
/// <para>
/// <b>AnalyzeFillAsync</b>: Calculates actual slippage as
/// <c>|actualFillPrice - expectedFillPrice| / expectedFillPrice * 10,000</c> bps,
/// then computes the deviation from the pre-trade estimate and persists a
/// <see cref="FillAnalysis"/> record.
/// </para>
/// <para>
/// <b>GenerateReportAsync</b>: Queries stored <see cref="FillAnalysis"/> records within
/// a date range and produces an <see cref="ExecutionReport"/> with aggregate statistics
/// (mean, std dev, best/worst fills).
/// </para>
/// </remarks>
public sealed class PostTradeFillAnalyzer : IFillAnalyzer
{
    private readonly RivrQuantDbContext _db;
    private readonly ExecutionReportGenerator _reportGenerator;
    private readonly ILogger<PostTradeFillAnalyzer> _logger;

    private const int BpsMultiplier = 10_000;

    /// <summary>
    /// Initializes a new instance of <see cref="PostTradeFillAnalyzer"/>.
    /// </summary>
    public PostTradeFillAnalyzer(
        RivrQuantDbContext db,
        ExecutionReportGenerator reportGenerator,
        ILogger<PostTradeFillAnalyzer> logger)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _reportGenerator = reportGenerator ?? throw new ArgumentNullException(nameof(reportGenerator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Analyzes a single fill by comparing the actual execution price to the
    /// pre-trade expected price and estimated slippage. Persists the analysis to the database.
    /// </summary>
    /// <param name="orderId">The internal order identifier.</param>
    /// <param name="symbol">The instrument symbol.</param>
    /// <param name="expectedFillPrice">The price expected before execution.</param>
    /// <param name="actualFillPrice">The actual price at which the fill occurred.</param>
    /// <param name="quantity">The filled quantity.</param>
    /// <param name="estimatedSlippageBps">The pre-trade estimated slippage in bps.</param>
    /// <param name="estimatedTotalCostBps">The pre-trade estimated total cost in bps.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The persisted <see cref="FillAnalysis"/> with computed deviations.</returns>
    public async Task<FillAnalysis> AnalyzeFillAsync(
        Guid orderId,
        string symbol,
        decimal expectedFillPrice,
        decimal actualFillPrice,
        decimal quantity,
        decimal estimatedSlippageBps,
        decimal estimatedTotalCostBps,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        // Actual slippage = |actual - expected| / expected * 10000 bps
        var actualSlippageBps = expectedFillPrice > 0
            ? Math.Abs(actualFillPrice - expectedFillPrice) / expectedFillPrice * BpsMultiplier
            : 0m;

        var deviationBps = actualSlippageBps - estimatedSlippageBps;
        var priceDifference = actualFillPrice - expectedFillPrice;
        var costDifference = quantity * Math.Abs(priceDifference);

        var analysis = new FillAnalysis
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            Symbol = symbol,
            ExpectedFillPrice = expectedFillPrice,
            ActualFillPrice = actualFillPrice,
            Quantity = quantity,
            EstimatedSlippageBps = estimatedSlippageBps,
            ActualSlippageBps = actualSlippageBps,
            DeviationBps = deviationBps,
            EstimatedTotalCostBps = estimatedTotalCostBps,
            CostDifferenceDollars = costDifference,
            AnalyzedAt = DateTimeOffset.UtcNow
        };

        _db.Set<FillAnalysis>().Add(analysis);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Fill analysis for {Symbol} (order {OrderId}): expected={ExpectedPrice}, actual={ActualPrice}, " +
            "slippage={ActualBps} bps (estimated {EstBps} bps), deviation={DevBps} bps",
            symbol, orderId, expectedFillPrice, actualFillPrice,
            actualSlippageBps, estimatedSlippageBps, deviationBps);

        return analysis;
    }

    /// <summary>
    /// Generates an aggregate execution quality report for all fills within the
    /// specified date range.
    /// </summary>
    /// <param name="from">Inclusive start of the analysis window (UTC).</param>
    /// <param name="to">Inclusive end of the analysis window (UTC).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// An <see cref="ExecutionReport"/> summarizing fill quality, including mean/stddev
    /// of deviation and identification of best/worst fills.
    /// </returns>
    public async Task<ExecutionReport> GenerateReportAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken ct)
    {
        var analyses = await _db.Set<FillAnalysis>()
            .Where(f => f.AnalyzedAt >= from && f.AnalyzedAt <= to)
            .OrderBy(f => f.AnalyzedAt)
            .ToListAsync(ct);

        var report = _reportGenerator.BuildReport(analyses, from, to);

        _logger.LogInformation(
            "Execution report generated for {From} to {To}: {Count} fills, " +
            "mean deviation={MeanDev:F2} bps, stddev={StdDev:F2} bps",
            from, to, analyses.Count, report.MeanDeviationBps, report.StdDevDeviationBps);

        return report;
    }
}
