namespace RivrQuant.Infrastructure.Persistence.Configurations;

using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RivrQuant.Domain.Enums;
using RivrQuant.Domain.Models.Alerts;
using RivrQuant.Domain.Models.Analysis;
using RivrQuant.Domain.Models.Backtests;
using RivrQuant.Domain.Models.Strategies;
using RivrQuant.Domain.Models.Trading;

/// <summary>EF Core configuration for <see cref="BacktestResult"/>.</summary>
public sealed class BacktestResultConfiguration : IEntityTypeConfiguration<BacktestResult>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<BacktestResult> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.ExternalBacktestId).HasMaxLength(256);
        builder.Property(e => e.ProjectId).HasMaxLength(128);
        builder.Property(e => e.StrategyName).HasMaxLength(256).IsRequired();
        builder.Property(e => e.StrategyDescription).HasMaxLength(2000);
        builder.Property(e => e.InitialCapital).HasPrecision(18, 2);
        builder.Property(e => e.FinalEquity).HasPrecision(18, 2);
        builder.Property(e => e.TotalReturn).HasPrecision(18, 8);
        builder.HasIndex(e => e.ExternalBacktestId);
        builder.HasIndex(e => e.ProjectId);
        builder.HasIndex(e => e.CreatedAt);
        builder.HasMany(e => e.Trades).WithOne().HasForeignKey(e => e.BacktestResultId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(e => e.DailyReturns).WithOne().HasForeignKey(e => e.BacktestResultId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(e => e.Metrics).WithOne().HasForeignKey<BacktestMetrics>(e => e.BacktestResultId).OnDelete(DeleteBehavior.Cascade);
    }
}

/// <summary>EF Core configuration for <see cref="BacktestTrade"/>.</summary>
public sealed class BacktestTradeConfiguration : IEntityTypeConfiguration<BacktestTrade>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<BacktestTrade> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Symbol).HasMaxLength(32).IsRequired();
        builder.Property(e => e.EntryPrice).HasPrecision(18, 8);
        builder.Property(e => e.ExitPrice).HasPrecision(18, 8);
        builder.Property(e => e.Quantity).HasPrecision(18, 8);
        builder.Property(e => e.ProfitLoss).HasPrecision(18, 8);
        builder.Property(e => e.ProfitLossPercent).HasPrecision(18, 8);
        builder.Property(e => e.Side).HasConversion<string>().HasMaxLength(16);
        builder.Ignore(e => e.HoldingPeriod);
        builder.Ignore(e => e.IsWin);
        builder.HasIndex(e => e.BacktestResultId);
        builder.HasIndex(e => e.Symbol);
    }
}

/// <summary>EF Core configuration for <see cref="DailyReturn"/>.</summary>
public sealed class DailyReturnConfiguration : IEntityTypeConfiguration<DailyReturn>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<DailyReturn> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Equity).HasPrecision(18, 2);
        builder.Property(e => e.DailyPnl).HasPrecision(18, 2);
        builder.Property(e => e.DailyReturnPercent).HasPrecision(18, 8);
        builder.Property(e => e.CumulativeReturn).HasPrecision(18, 8);
        builder.Property(e => e.Drawdown).HasPrecision(18, 8);
        builder.Property(e => e.BenchmarkEquity).HasPrecision(18, 2);
        builder.HasIndex(e => e.BacktestResultId);
        builder.HasIndex(e => e.Date);
    }
}

/// <summary>EF Core configuration for <see cref="BacktestMetrics"/>.</summary>
public sealed class BacktestMetricsConfiguration : IEntityTypeConfiguration<BacktestMetrics>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<BacktestMetrics> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.AverageWin).HasPrecision(18, 8);
        builder.Property(e => e.AverageLoss).HasPrecision(18, 8);
        builder.Property(e => e.LargestWin).HasPrecision(18, 8);
        builder.Property(e => e.LargestLoss).HasPrecision(18, 8);
        builder.Ignore(e => e.LossRate);
        builder.HasIndex(e => e.BacktestResultId).IsUnique();
    }
}

/// <summary>EF Core configuration for <see cref="Strategy"/>.</summary>
public sealed class StrategyConfiguration : IEntityTypeConfiguration<Strategy>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Strategy> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).HasMaxLength(256).IsRequired();
        builder.Property(e => e.Description).HasMaxLength(2000);
        builder.Property(e => e.AssetClass).HasConversion<string>().HasMaxLength(32);
        builder.HasIndex(e => e.Name);
        builder.HasIndex(e => e.CreatedAt);
        builder.HasMany(e => e.Parameters).WithOne().HasForeignKey(e => e.StrategyId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(e => e.Versions).WithOne().HasForeignKey(e => e.StrategyId).OnDelete(DeleteBehavior.Cascade);
    }
}

/// <summary>EF Core configuration for <see cref="StrategyParameter"/>.</summary>
public sealed class StrategyParameterConfiguration : IEntityTypeConfiguration<StrategyParameter>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<StrategyParameter> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).HasMaxLength(128).IsRequired();
        builder.Property(e => e.Description).HasMaxLength(512);
        builder.Property(e => e.Value).HasPrecision(18, 8);
        builder.Property(e => e.MinValue).HasPrecision(18, 8);
        builder.Property(e => e.MaxValue).HasPrecision(18, 8);
        builder.Property(e => e.StepSize).HasPrecision(18, 8);
        builder.HasIndex(e => e.StrategyId);
    }
}

/// <summary>EF Core configuration for <see cref="StrategyVersion"/>.</summary>
public sealed class StrategyVersionConfiguration : IEntityTypeConfiguration<StrategyVersion>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<StrategyVersion> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.ChangeNotes).HasMaxLength(2000);
        builder.Property(e => e.ParametersSnapshot).HasMaxLength(8000);
        builder.HasIndex(e => e.StrategyId);
    }
}

/// <summary>EF Core configuration for <see cref="Order"/>.</summary>
public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.ExternalOrderId).HasMaxLength(256);
        builder.Property(e => e.ClientOrderId).HasMaxLength(256);
        builder.Property(e => e.Symbol).HasMaxLength(32).IsRequired();
        builder.Property(e => e.Side).HasConversion<string>().HasMaxLength(16);
        builder.Property(e => e.Type).HasConversion<string>().HasMaxLength(32);
        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(32);
        builder.Property(e => e.Quantity).HasPrecision(18, 8);
        builder.Property(e => e.LimitPrice).HasPrecision(18, 8);
        builder.Property(e => e.StopPrice).HasPrecision(18, 8);
        builder.Property(e => e.FilledQuantity).HasPrecision(18, 8);
        builder.Property(e => e.FilledAveragePrice).HasPrecision(18, 8);
        builder.Property(e => e.Broker).HasConversion<string>().HasMaxLength(32);
        builder.Property(e => e.AssetClass).HasConversion<string>().HasMaxLength(32);
        builder.Property(e => e.RejectReason).HasMaxLength(1000);
        builder.HasIndex(e => e.ExternalOrderId);
        builder.HasIndex(e => e.ClientOrderId);
        builder.HasIndex(e => e.Symbol);
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.CreatedAt);
        builder.HasMany(e => e.Fills).WithOne().HasForeignKey(e => e.OrderId).OnDelete(DeleteBehavior.Cascade);
    }
}

/// <summary>EF Core configuration for <see cref="Fill"/>.</summary>
public sealed class FillConfiguration : IEntityTypeConfiguration<Fill>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Fill> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.ExternalFillId).HasMaxLength(256);
        builder.Property(e => e.Price).HasPrecision(18, 8);
        builder.Property(e => e.Quantity).HasPrecision(18, 8);
        builder.Property(e => e.Commission).HasPrecision(18, 8);
        builder.HasIndex(e => e.OrderId);
    }
}

/// <summary>EF Core configuration for <see cref="PerformanceSnapshot"/>.</summary>
public sealed class PerformanceSnapshotConfiguration : IEntityTypeConfiguration<PerformanceSnapshot>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<PerformanceSnapshot> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.TotalEquity).HasPrecision(18, 2);
        builder.Property(e => e.DailyPnl).HasPrecision(18, 2);
        builder.Property(e => e.DailyReturnPercent).HasPrecision(18, 8);
        builder.Property(e => e.CumulativeReturn).HasPrecision(18, 8);
        builder.Property(e => e.CurrentDrawdown).HasPrecision(18, 8);
        builder.HasIndex(e => e.Timestamp);
    }
}

/// <summary>EF Core configuration for <see cref="AiAnalysisReport"/>.</summary>
public sealed class AiAnalysisReportConfiguration : IEntityTypeConfiguration<AiAnalysisReport>
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<AiAnalysisReport> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.OverallAssessment).HasMaxLength(4000);
        builder.Property(e => e.OverfittingRisk).HasMaxLength(32);
        builder.Property(e => e.OverfittingExplanation).HasMaxLength(4000);
        builder.Property(e => e.RegimeAnalysis).HasMaxLength(8000);
        builder.Property(e => e.PlainEnglishSummary).HasMaxLength(8000);
        builder.Property(e => e.EstimatedCost).HasPrecision(18, 8);

        builder.Property(e => e.Strengths).HasConversion(
            v => JsonSerializer.Serialize(v, JsonOptions),
            v => JsonSerializer.Deserialize<IReadOnlyList<string>>(v, JsonOptions) ?? Array.Empty<string>()
        ).HasMaxLength(8000);

        builder.Property(e => e.Weaknesses).HasConversion(
            v => JsonSerializer.Serialize(v, JsonOptions),
            v => JsonSerializer.Deserialize<IReadOnlyList<string>>(v, JsonOptions) ?? Array.Empty<string>()
        ).HasMaxLength(8000);

        builder.Property(e => e.ParameterSuggestions).HasConversion(
            v => JsonSerializer.Serialize(v, JsonOptions),
            v => JsonSerializer.Deserialize<IReadOnlyList<string>>(v, JsonOptions) ?? Array.Empty<string>()
        ).HasMaxLength(8000);

        builder.Property(e => e.CriticalWarnings).HasConversion(
            v => JsonSerializer.Serialize(v, JsonOptions),
            v => JsonSerializer.Deserialize<IReadOnlyList<string>>(v, JsonOptions) ?? Array.Empty<string>()
        ).HasMaxLength(8000);

        builder.HasIndex(e => e.BacktestResultId);
        builder.HasIndex(e => e.CreatedAt);
    }
}

/// <summary>EF Core configuration for <see cref="RegimeClassification"/>.</summary>
public sealed class RegimeClassificationConfiguration : IEntityTypeConfiguration<RegimeClassification>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<RegimeClassification> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Regime).HasConversion<string>().HasMaxLength(32);
        builder.HasIndex(e => e.BacktestResultId);
        builder.HasIndex(e => e.StartDate);
    }
}

/// <summary>EF Core configuration for <see cref="WalkForwardResult"/>.</summary>
public sealed class WalkForwardResultConfiguration : IEntityTypeConfiguration<WalkForwardResult>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<WalkForwardResult> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Ignore(e => e.Efficiency);
        builder.HasIndex(e => e.BacktestResultId);
    }
}

/// <summary>EF Core configuration for <see cref="AlertRule"/>.</summary>
public sealed class AlertRuleConfiguration : IEntityTypeConfiguration<AlertRule>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<AlertRule> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).HasMaxLength(256).IsRequired();
        builder.Property(e => e.ConditionType).HasMaxLength(128).IsRequired();
        builder.Property(e => e.Threshold).HasPrecision(18, 8);
        builder.Property(e => e.ComparisonOperator).HasMaxLength(32);
        builder.Property(e => e.Severity).HasConversion<string>().HasMaxLength(32);
        builder.HasIndex(e => e.IsActive);
        builder.HasIndex(e => e.CreatedAt);
    }
}

/// <summary>EF Core configuration for <see cref="AlertEvent"/>.</summary>
public sealed class AlertEventConfiguration : IEntityTypeConfiguration<AlertEvent>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<AlertEvent> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.RuleName).HasMaxLength(256);
        builder.Property(e => e.Severity).HasConversion<string>().HasMaxLength(32);
        builder.Property(e => e.Message).HasMaxLength(2000);
        builder.Property(e => e.CurrentValue).HasPrecision(18, 8);
        builder.Property(e => e.ThresholdValue).HasPrecision(18, 8);
        builder.Property(e => e.DeliveryError).HasMaxLength(2000);
        builder.HasIndex(e => e.AlertRuleId);
        builder.HasIndex(e => e.TriggeredAt);
        builder.HasIndex(e => e.Severity);
    }
}

/// <summary>EF Core configuration for <see cref="AlertChannel"/>.</summary>
public sealed class AlertChannelConfiguration : IEntityTypeConfiguration<AlertChannel>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<AlertChannel> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.ChannelType).HasMaxLength(32).IsRequired();
        builder.Property(e => e.Destination).HasMaxLength(256).IsRequired();
        builder.HasIndex(e => e.ChannelType);
    }
}
