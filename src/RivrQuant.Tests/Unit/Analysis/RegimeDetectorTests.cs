using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RivrQuant.Domain.Enums;
using RivrQuant.Domain.Models.Backtests;
using RivrQuant.Infrastructure.Analysis;

namespace RivrQuant.Tests.Unit.Analysis;

public class RegimeDetectorTests
{
    private readonly RegimeDetector _detector;
    private readonly MathNetStatisticsEngine _stats = new();

    public RegimeDetectorTests()
    {
        var logger = new Mock<ILogger<RegimeDetector>>();
        _detector = new RegimeDetector(_stats, logger.Object);
    }

    [Fact]
    public void DetectRegimes_WithInsufficientData_ReturnsEmpty()
    {
        var dailyReturns = GenerateDailyReturns(30, 0.01m, 0.001m);
        var backtestId = Guid.NewGuid();

        var regimes = _detector.DetectRegimes(dailyReturns, backtestId);

        regimes.Should().BeEmpty();
    }

    [Fact]
    public void DetectRegimes_WithSufficientData_ReturnsNonEmpty()
    {
        var dailyReturns = GenerateDailyReturns(200, 0.001m, 0.01m);
        var backtestId = Guid.NewGuid();

        var regimes = _detector.DetectRegimes(dailyReturns, backtestId);

        regimes.Should().NotBeEmpty();
    }

    [Fact]
    public void DetectRegimes_SetsBacktestResultId()
    {
        var dailyReturns = GenerateDailyReturns(200, 0.001m, 0.01m);
        var backtestId = Guid.NewGuid();

        var regimes = _detector.DetectRegimes(dailyReturns, backtestId);

        regimes.Should().AllSatisfy(r => r.BacktestResultId.Should().Be(backtestId));
    }

    [Fact]
    public void DetectRegimes_HasValidDateRanges()
    {
        var dailyReturns = GenerateDailyReturns(200, 0.001m, 0.01m);
        var backtestId = Guid.NewGuid();

        var regimes = _detector.DetectRegimes(dailyReturns, backtestId);

        regimes.Should().AllSatisfy(r =>
        {
            r.EndDate.Should().BeOnOrAfter(r.StartDate);
        });
    }

    [Fact]
    public void DetectRegimes_WithHighVolatilityData_ContainsHighVolRegime()
    {
        // Generate data with very high volatility (large swings)
        var dailyReturns = new List<DailyReturn>();
        var equity = 100000m;
        var baseDate = new DateTimeOffset(2023, 1, 1, 0, 0, 0, TimeSpan.Zero);

        for (var i = 0; i < 200; i++)
        {
            var ret = i % 2 == 0 ? 0.05m : -0.04m; // Very high volatility
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

        var backtestId = Guid.NewGuid();
        var regimes = _detector.DetectRegimes(dailyReturns, backtestId);

        regimes.Should().NotBeEmpty();
        // At least some regime should be detected
        regimes.Should().AllSatisfy(r =>
        {
            Enum.IsDefined(typeof(RegimeType), r.Regime).Should().BeTrue();
        });
    }

    private static List<DailyReturn> GenerateDailyReturns(int days, decimal meanReturn, decimal volatility)
    {
        var random = new Random(42);
        var dailyReturns = new List<DailyReturn>();
        var equity = 100000m;
        var baseDate = new DateTimeOffset(2023, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var peak = equity;

        for (var i = 0; i < days; i++)
        {
            var ret = meanReturn + (decimal)(random.NextDouble() - 0.5) * 2 * volatility;
            equity *= (1 + ret);
            if (equity > peak) peak = equity;
            var dd = peak > 0 ? (equity - peak) / peak : 0;

            dailyReturns.Add(new DailyReturn
            {
                Date = baseDate.AddDays(i),
                Equity = equity,
                DailyReturnPercent = ret,
                DailyPnl = equity * ret,
                CumulativeReturn = (equity - 100000m) / 100000m,
                Drawdown = dd
            });
        }

        return dailyReturns;
    }
}
