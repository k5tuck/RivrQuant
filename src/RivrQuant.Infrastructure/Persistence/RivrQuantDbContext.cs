namespace RivrQuant.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using RivrQuant.Domain.Models.Alerts;
using RivrQuant.Domain.Models.Analysis;
using RivrQuant.Domain.Models.Backtests;
using RivrQuant.Domain.Models.Strategies;
using RivrQuant.Domain.Models.Trading;

/// <summary>Entity Framework Core database context for RivrQuant.</summary>
public sealed class RivrQuantDbContext : DbContext
{
    /// <summary>Initializes a new instance of <see cref="RivrQuantDbContext"/>.</summary>
    public RivrQuantDbContext(DbContextOptions<RivrQuantDbContext> options) : base(options) { }

    /// <summary>Backtest result records.</summary>
    public DbSet<BacktestResult> BacktestResults => Set<BacktestResult>();

    /// <summary>Individual trades from backtests.</summary>
    public DbSet<BacktestTrade> BacktestTrades => Set<BacktestTrade>();

    /// <summary>Daily return/equity data from backtests.</summary>
    public DbSet<DailyReturn> DailyReturns => Set<DailyReturn>();

    /// <summary>Calculated backtest metrics.</summary>
    public DbSet<BacktestMetrics> BacktestMetrics => Set<BacktestMetrics>();

    /// <summary>Strategy definitions.</summary>
    public DbSet<Strategy> Strategies => Set<Strategy>();

    /// <summary>Strategy parameters.</summary>
    public DbSet<StrategyParameter> StrategyParameters => Set<StrategyParameter>();

    /// <summary>Strategy version history.</summary>
    public DbSet<StrategyVersion> StrategyVersions => Set<StrategyVersion>();

    /// <summary>Trading orders.</summary>
    public DbSet<Order> Orders => Set<Order>();

    /// <summary>Order execution fills.</summary>
    public DbSet<Fill> Fills => Set<Fill>();

    /// <summary>Portfolio performance snapshots.</summary>
    public DbSet<PerformanceSnapshot> PerformanceSnapshots => Set<PerformanceSnapshot>();

    /// <summary>AI analysis reports.</summary>
    public DbSet<AiAnalysisReport> AiAnalysisReports => Set<AiAnalysisReport>();

    /// <summary>Market regime classifications.</summary>
    public DbSet<RegimeClassification> RegimeClassifications => Set<RegimeClassification>();

    /// <summary>Walk-forward validation results.</summary>
    public DbSet<WalkForwardResult> WalkForwardResults => Set<WalkForwardResult>();

    /// <summary>Alert rules.</summary>
    public DbSet<AlertRule> AlertRules => Set<AlertRule>();

    /// <summary>Alert event history.</summary>
    public DbSet<AlertEvent> AlertEvents => Set<AlertEvent>();

    /// <summary>Alert delivery channels.</summary>
    public DbSet<AlertChannel> AlertChannels => Set<AlertChannel>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(RivrQuantDbContext).Assembly);
    }
}
