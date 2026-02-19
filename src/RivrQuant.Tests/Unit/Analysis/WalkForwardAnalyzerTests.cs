using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RivrQuant.Domain.Models.Backtests;
using RivrQuant.Infrastructure.Analysis;

namespace RivrQuant.Tests.Unit.Analysis;

public class WalkForwardAnalyzerTests
{
    private readonly WalkForwardAnalyzer _analyzer;
    private readonly MathNetStatisticsEngine _stats = new();

    public WalkForwardAnalyzerTests()
    {
        var logger = new Mock<ILogger<WalkForwardAnalyzer>>();
        _analyzer = new WalkForwardAnalyzer(_stats, logger.Object);
    }

    [Fact]
    public void RunWalkForwardAnalysis_WithInsufficientData_ReturnsEmpty()
    {
        var dailyReturns = GenerateDailyReturns(10);
        var backtestId = Guid.NewGuid();

        var results = _analyzer.RunWalkForwardAnalysis(dailyReturns, 5, 0.7, backtestId);

        results.Should().BeEmpty();
    }

    [Fact]
    public void RunWalkForwardAnalysis_WithSufficientData_ReturnsExpectedWindowCount()
    {
        var dailyReturns = GenerateDailyReturns(500);
        var backtestId = Guid.NewGuid();

        var results = _analyzer.RunWalkForwardAnalysis(dailyReturns, 5, 0.7, backtestId);

        results.Should().HaveCount(5);
    }

    [Fact]
    public void RunWalkForwardAnalysis_SetsCorrectWindowNumbers()
    {
        var dailyReturns = GenerateDailyReturns(500);
        var backtestId = Guid.NewGuid();

        var results = _analyzer.RunWalkForwardAnalysis(dailyReturns, 5, 0.7, backtestId);

        results.Select(r => r.WindowNumber).Should().BeEquivalentTo(new[] { 1, 2, 3, 4, 5 });
    }

    [Fact]
    public void RunWalkForwardAnalysis_SetsBacktestResultId()
    {
        var dailyReturns = GenerateDailyReturns(500);
        var backtestId = Guid.NewGuid();

        var results = _analyzer.RunWalkForwardAnalysis(dailyReturns, 5, 0.7, backtestId);

        results.Should().AllSatisfy(r => r.BacktestResultId.Should().Be(backtestId));
    }

    [Fact]
    public void RunWalkForwardAnalysis_InSamplePrecedesOutOfSample()
    {
        var dailyReturns = GenerateDailyReturns(500);
        var backtestId = Guid.NewGuid();

        var results = _analyzer.RunWalkForwardAnalysis(dailyReturns, 3, 0.7, backtestId);

        results.Should().AllSatisfy(r =>
        {
            r.InSampleEnd.Should().BeBefore(r.OutOfSampleStart);
            r.InSampleStart.Should().BeOnOrBefore(r.InSampleEnd);
            r.OutOfSampleStart.Should().BeOnOrBefore(r.OutOfSampleEnd);
        });
    }

    [Fact]
    public void RunWalkForwardAnalysis_SharpeDecayIsComputed()
    {
        var dailyReturns = GenerateDailyReturns(500);
        var backtestId = Guid.NewGuid();

        var results = _analyzer.RunWalkForwardAnalysis(dailyReturns, 3, 0.7, backtestId);

        results.Should().AllSatisfy(r =>
        {
            // SharpeDecay is a computed property: 1 - (OOS / IS)
            if (r.InSampleSharpe != 0)
            {
                var expectedDecay = 1 - (r.OutOfSampleSharpe / r.InSampleSharpe);
                r.SharpeDecay.Should().BeApproximately(expectedDecay, 0.001);
            }
        });
    }

    private static List<DailyReturn> GenerateDailyReturns(int days)
    {
        var random = new Random(42);
        var dailyReturns = new List<DailyReturn>();
        var equity = 100000m;
        var baseDate = new DateTimeOffset(2023, 1, 1, 0, 0, 0, TimeSpan.Zero);

        for (var i = 0; i < days; i++)
        {
            var ret = (decimal)(random.NextDouble() - 0.48) * 0.02m;
            equity *= (1 + ret);
            dailyReturns.Add(new DailyReturn
            {
                Date = baseDate.AddDays(i),
                Equity = equity,
                DailyReturnPercent = ret,
                DailyPnl = equity * ret,
                CumulativeReturn = (equity - 100000m) / 100000m
            });
        }

        return dailyReturns;
    }
}
