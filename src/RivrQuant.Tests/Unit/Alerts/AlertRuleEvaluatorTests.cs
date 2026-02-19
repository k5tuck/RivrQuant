using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RivrQuant.Domain.Enums;
using RivrQuant.Domain.Models.Alerts;
using RivrQuant.Domain.Models.Trading;
using RivrQuant.Infrastructure.Alerts;

namespace RivrQuant.Tests.Unit.Alerts;

public class AlertRuleEvaluatorTests
{
    private readonly AlertRuleEvaluator _evaluator;

    public AlertRuleEvaluatorTests()
    {
        var logger = new Mock<ILogger<AlertRuleEvaluator>>();
        _evaluator = new AlertRuleEvaluator(logger.Object);
    }

    [Fact]
    public void Evaluate_InactiveRule_ReturnsNull()
    {
        var rule = new AlertRule
        {
            Name = "Test Rule",
            ConditionType = "DrawdownExceedsPercent",
            Threshold = 0.10m,
            IsActive = false,
            Severity = AlertSeverity.Warning
        };

        var portfolio = CreatePortfolio();
        var snapshot = CreateSnapshot(currentDrawdown: -0.15m);

        var result = _evaluator.Evaluate(rule, portfolio, snapshot);

        result.Should().BeNull();
    }

    [Fact]
    public void Evaluate_DrawdownExceedsThreshold_ReturnsAlertEvent()
    {
        var rule = new AlertRule
        {
            Name = "Drawdown Alert",
            ConditionType = "DrawdownExceedsPercent",
            Threshold = 0.10m,
            IsActive = true,
            Severity = AlertSeverity.Critical
        };

        var portfolio = CreatePortfolio();
        var snapshot = CreateSnapshot(currentDrawdown: -0.15m);

        var result = _evaluator.Evaluate(rule, portfolio, snapshot);

        result.Should().NotBeNull();
        result!.RuleName.Should().Be("Drawdown Alert");
        result.Severity.Should().Be(AlertSeverity.Critical);
    }

    [Fact]
    public void Evaluate_DrawdownBelowThreshold_ReturnsNull()
    {
        var rule = new AlertRule
        {
            Name = "Drawdown Alert",
            ConditionType = "DrawdownExceedsPercent",
            Threshold = 0.10m,
            IsActive = true,
            Severity = AlertSeverity.Warning
        };

        var portfolio = CreatePortfolio();
        var snapshot = CreateSnapshot(currentDrawdown: -0.05m);

        var result = _evaluator.Evaluate(rule, portfolio, snapshot);

        result.Should().BeNull();
    }

    [Fact]
    public void Evaluate_DailyLossExceedsAmount_ReturnsAlertEvent()
    {
        var rule = new AlertRule
        {
            Name = "Daily Loss Alert",
            ConditionType = "DailyLossExceedsAmount",
            Threshold = -500m,
            IsActive = true,
            Severity = AlertSeverity.Warning
        };

        var portfolio = new Portfolio
        {
            TotalEquity = 9000m,
            CashBalance = 5000m,
            BuyingPower = 10000m,
            UnrealizedPnl = -300m,
            RealizedPnlToday = -400m,
            DailyChangePercent = -0.07m,
            Broker = BrokerType.Alpaca
        };

        var result = _evaluator.Evaluate(rule, portfolio, null);

        result.Should().NotBeNull();
        result!.RuleName.Should().Be("Daily Loss Alert");
    }

    [Fact]
    public void Evaluate_DailyLossWithinLimit_ReturnsNull()
    {
        var rule = new AlertRule
        {
            Name = "Daily Loss Alert",
            ConditionType = "DailyLossExceedsAmount",
            Threshold = -500m,
            IsActive = true,
            Severity = AlertSeverity.Warning
        };

        var portfolio = new Portfolio
        {
            TotalEquity = 10000m,
            CashBalance = 5000m,
            BuyingPower = 10000m,
            UnrealizedPnl = -100m,
            RealizedPnlToday = -50m,
            DailyChangePercent = -0.015m,
            Broker = BrokerType.Alpaca
        };

        var result = _evaluator.Evaluate(rule, portfolio, null);

        result.Should().BeNull();
    }

    [Fact]
    public void Evaluate_WithinCooldownPeriod_ReturnsNull()
    {
        var rule = new AlertRule
        {
            Name = "Drawdown Alert",
            ConditionType = "DrawdownExceedsPercent",
            Threshold = 0.10m,
            IsActive = true,
            Severity = AlertSeverity.Critical,
            CooldownPeriod = TimeSpan.FromHours(1),
            LastTriggeredAt = DateTimeOffset.UtcNow.AddMinutes(-30)
        };

        var portfolio = CreatePortfolio();
        var snapshot = CreateSnapshot(currentDrawdown: -0.15m);

        var result = _evaluator.Evaluate(rule, portfolio, snapshot);

        result.Should().BeNull();
    }

    [Fact]
    public void Evaluate_CooldownExpired_ReturnsAlertEvent()
    {
        var rule = new AlertRule
        {
            Name = "Drawdown Alert",
            ConditionType = "DrawdownExceedsPercent",
            Threshold = 0.10m,
            IsActive = true,
            Severity = AlertSeverity.Critical,
            CooldownPeriod = TimeSpan.FromHours(1),
            LastTriggeredAt = DateTimeOffset.UtcNow.AddHours(-2)
        };

        var portfolio = CreatePortfolio();
        var snapshot = CreateSnapshot(currentDrawdown: -0.15m);

        var result = _evaluator.Evaluate(rule, portfolio, snapshot);

        result.Should().NotBeNull();
    }

    [Fact]
    public void Evaluate_NullSnapshot_ReturnsNullForDrawdown()
    {
        var rule = new AlertRule
        {
            Name = "Drawdown Alert",
            ConditionType = "DrawdownExceedsPercent",
            Threshold = 0.10m,
            IsActive = true,
            Severity = AlertSeverity.Warning
        };

        var portfolio = CreatePortfolio();

        var result = _evaluator.Evaluate(rule, portfolio, null);

        result.Should().BeNull();
    }

    [Fact]
    public void CreateEventAlert_ReturnsCorrectAlertEvent()
    {
        var rule = new AlertRule
        {
            Name = "New Backtest",
            ConditionType = "NewBacktestComplete",
            IsActive = true,
            Severity = AlertSeverity.Info
        };

        var result = _evaluator.CreateEventAlert(rule, "New backtest detected: MyStrategy");

        result.Should().NotBeNull();
        result.RuleName.Should().Be("New Backtest");
        result.Message.Should().Contain("MyStrategy");
        result.Severity.Should().Be(AlertSeverity.Info);
    }

    private static Portfolio CreatePortfolio() => new()
    {
        TotalEquity = 10000m,
        CashBalance = 5000m,
        BuyingPower = 10000m,
        UnrealizedPnl = -200m,
        RealizedPnlToday = -100m,
        DailyChangePercent = -0.03m,
        Broker = BrokerType.Alpaca
    };

    private static PerformanceSnapshot CreateSnapshot(decimal currentDrawdown) => new()
    {
        TotalEquity = 10000m,
        DailyPnl = -300m,
        DailyReturnPercent = -0.03m,
        CumulativeReturn = 0.10m,
        CurrentDrawdown = currentDrawdown,
        OpenPositionCount = 3
    };
}
