using FluentAssertions;
using RivrQuant.Infrastructure.Analysis;

namespace RivrQuant.Tests.Unit.Analysis;

public class StatisticsEngineTests
{
    private readonly MathNetStatisticsEngine _engine = new();

    [Fact]
    public void CalculateSharpeRatio_WithPositiveReturns_ReturnsPositiveValue()
    {
        var returns = Enumerable.Range(0, 252)
            .Select(_ => 0.001)
            .ToArray();

        var sharpe = _engine.CalculateSharpeRatio(returns, 0.0);

        sharpe.Should().BeGreaterThan(0);
    }

    [Fact]
    public void CalculateSharpeRatio_WithInsufficientData_ReturnsZero()
    {
        var returns = new double[] { 0.01 };

        var sharpe = _engine.CalculateSharpeRatio(returns, 0.0);

        sharpe.Should().Be(0);
    }

    [Fact]
    public void CalculateSharpeRatio_WithConstantReturns_ReturnsZero()
    {
        // Zero standard deviation results in zero Sharpe (not infinity)
        var returns = new double[] { 0.0, 0.0, 0.0, 0.0, 0.0 };

        var sharpe = _engine.CalculateSharpeRatio(returns, 0.0);

        sharpe.Should().Be(0);
    }

    [Fact]
    public void CalculateSortinoRatio_WithOnlyPositiveReturns_ReturnsMaxValue()
    {
        var returns = new double[] { 0.01, 0.02, 0.015, 0.01, 0.005 };

        var sortino = _engine.CalculateSortinoRatio(returns, 0.0);

        sortino.Should().Be(double.MaxValue);
    }

    [Fact]
    public void CalculateSortinoRatio_WithMixedReturns_ReturnsPositiveValue()
    {
        var returns = new double[] { 0.01, -0.005, 0.02, -0.01, 0.015, -0.002 };

        var sortino = _engine.CalculateSortinoRatio(returns, 0.0);

        sortino.Should().BeGreaterThan(0);
    }

    [Fact]
    public void CalculateMaxDrawdown_WithNoDrawdown_ReturnsZero()
    {
        var equity = new double[] { 100, 101, 102, 103, 104 };

        var maxDd = _engine.CalculateMaxDrawdown(equity);

        maxDd.Should().Be(0);
    }

    [Fact]
    public void CalculateMaxDrawdown_WithDrawdown_ReturnsCorrectValue()
    {
        var equity = new double[] { 100, 110, 90, 95, 105 };

        var maxDd = _engine.CalculateMaxDrawdown(equity);

        // Peak = 110, trough = 90, dd = 20/110 = 0.1818...
        maxDd.Should().BeApproximately(20.0 / 110.0, 0.001);
    }

    [Fact]
    public void CalculateMaxDrawdown_EmptyEquityCurve_ReturnsZero()
    {
        var maxDd = _engine.CalculateMaxDrawdown(Array.Empty<double>());

        maxDd.Should().Be(0);
    }

    [Fact]
    public void CalculateValueAtRisk_Returns95thPercentileLoss()
    {
        // 100 returns: 99 are 0.01 and 1 is -0.10
        var returns = new List<double>();
        returns.Add(-0.10);
        for (var i = 0; i < 99; i++) returns.Add(0.01);

        var var95 = _engine.CalculateValueAtRisk(returns, 0.95);

        var95.Should().BeLessThan(0);
    }

    [Fact]
    public void CalculateExpectedShortfall_IsLessThanOrEqualToVaR()
    {
        var random = new Random(42);
        var returns = Enumerable.Range(0, 500)
            .Select(_ => (random.NextDouble() - 0.5) * 0.04)
            .ToArray();

        var var95 = _engine.CalculateValueAtRisk(returns, 0.95);
        var es95 = _engine.CalculateExpectedShortfall(returns, 0.95);

        es95.Should().BeLessOrEqualTo(var95);
    }

    [Fact]
    public void CalculateCalmarRatio_WithZeroDrawdown_ReturnsZero()
    {
        var calmar = _engine.CalculateCalmarRatio(0.15, 0.0);

        calmar.Should().Be(0);
    }

    [Fact]
    public void CalculateCalmarRatio_WithValidInputs_ReturnsCorrectRatio()
    {
        var calmar = _engine.CalculateCalmarRatio(0.20, 0.10);

        calmar.Should().BeApproximately(2.0, 0.001);
    }

    [Fact]
    public void CalculateProfitFactor_WithWinsAndLosses_ReturnsCorrectRatio()
    {
        var wins = new double[] { 100, 200, 150 };
        var losses = new double[] { -50, -75, -25 };

        var pf = _engine.CalculateProfitFactor(wins, losses);

        pf.Should().BeApproximately(450.0 / 150.0, 0.001);
    }

    [Fact]
    public void CalculateProfitFactor_WithNoLosses_ReturnsMaxValue()
    {
        var wins = new double[] { 100, 200 };
        var losses = Array.Empty<double>();

        var pf = _engine.CalculateProfitFactor(wins, losses);

        pf.Should().Be(double.MaxValue);
    }

    [Fact]
    public void CalculateBeta_WithCorrelatedReturns_ReturnsPositiveBeta()
    {
        var strategy = new double[] { 0.02, -0.01, 0.03, -0.02, 0.01, -0.01 };
        var benchmark = new double[] { 0.01, -0.005, 0.015, -0.01, 0.005, -0.005 };

        var beta = _engine.CalculateBeta(strategy, benchmark);

        beta.Should().BeGreaterThan(0);
    }

    [Fact]
    public void CalculateRollingStatistic_ReturnsCorrectWindowCount()
    {
        var returns = Enumerable.Range(0, 100).Select(i => (double)i * 0.001).ToArray();

        var rolling = _engine.CalculateRollingStatistic(returns, 20, window => window.Average());

        rolling.Count.Should().Be(81); // 100 - 20 + 1
    }

    [Fact]
    public void CalculateRollingStatistic_InsufficientData_ReturnsEmpty()
    {
        var returns = new double[] { 0.01, 0.02 };

        var rolling = _engine.CalculateRollingStatistic(returns, 20, window => window.Average());

        rolling.Should().BeEmpty();
    }

    [Fact]
    public void CalculateMonteCarloDrawdownProbability_ReturnsValueBetweenZeroAndOne()
    {
        var returns = new double[] { 0.01, -0.01, 0.005, -0.005, 0.02, -0.015, 0.01, -0.008 };

        var prob = _engine.CalculateMonteCarloDrawdownProbability(returns, 0.05, 1000, 252);

        prob.Should().BeGreaterOrEqualTo(0);
        prob.Should().BeLessOrEqualTo(1);
    }
}
