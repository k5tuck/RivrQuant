using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RivrQuant.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AlertChannels",
                columns: table => new
                {
                    Id          = table.Column<Guid>(nullable: false),
                    ChannelType = table.Column<string>(maxLength: 32, nullable: false),
                    Destination = table.Column<string>(maxLength: 256, nullable: false),
                    IsActive    = table.Column<bool>(nullable: false)
                },
                constraints: t => t.PrimaryKey("PK_AlertChannels", x => x.Id));

            migrationBuilder.CreateTable(
                name: "AlertRules",
                columns: table => new
                {
                    Id                 = table.Column<Guid>(nullable: false),
                    Name               = table.Column<string>(maxLength: 256, nullable: false),
                    ConditionType      = table.Column<string>(maxLength: 128, nullable: false),
                    Threshold          = table.Column<decimal>(precision: 18, scale: 8, nullable: false),
                    ComparisonOperator = table.Column<string>(maxLength: 32, nullable: true),
                    Severity           = table.Column<string>(maxLength: 32, nullable: false),
                    IsActive           = table.Column<bool>(nullable: false),
                    CreatedAt          = table.Column<DateTimeOffset>(nullable: false)
                },
                constraints: t => t.PrimaryKey("PK_AlertRules", x => x.Id));

            migrationBuilder.CreateTable(
                name: "BacktestResults",
                columns: table => new
                {
                    Id                  = table.Column<Guid>(nullable: false),
                    ExternalBacktestId  = table.Column<string>(maxLength: 256, nullable: false),
                    ProjectId           = table.Column<string>(maxLength: 128, nullable: false),
                    ProjectName         = table.Column<string>(nullable: true),
                    StrategyName        = table.Column<string>(maxLength: 256, nullable: false),
                    StrategyDescription = table.Column<string>(maxLength: 2000, nullable: true),
                    StartDate           = table.Column<DateTimeOffset>(nullable: false),
                    EndDate             = table.Column<DateTimeOffset>(nullable: false),
                    CreatedAt           = table.Column<DateTimeOffset>(nullable: false),
                    InitialCapital      = table.Column<decimal>(precision: 18, scale: 2, nullable: false),
                    FinalEquity         = table.Column<decimal>(precision: 18, scale: 2, nullable: false),
                    TotalReturn         = table.Column<decimal>(precision: 18, scale: 8, nullable: false),
                    IsAnalyzed          = table.Column<bool>(nullable: false)
                },
                constraints: t => t.PrimaryKey("PK_BacktestResults", x => x.Id));

            migrationBuilder.CreateTable(
                name: "CorrelationSnapshots",
                columns: table => new
                {
                    Id          = table.Column<Guid>(nullable: false),
                    SymbolsJson = table.Column<string>(maxLength: 8000, nullable: false),
                    MatrixJson  = table.Column<string>(maxLength: 65535, nullable: false),
                    LookbackDays = table.Column<int>(nullable: false),
                    SnapshotAt  = table.Column<DateTimeOffset>(nullable: false)
                },
                constraints: t => t.PrimaryKey("PK_CorrelationSnapshots", x => x.Id));

            migrationBuilder.CreateTable(
                name: "DeleverageEvents",
                columns: table => new
                {
                    Id                       = table.Column<Guid>(nullable: false),
                    TriggeredAt              = table.Column<DateTimeOffset>(nullable: false),
                    PreviousLevel            = table.Column<string>(maxLength: 32, nullable: false),
                    NewLevel                 = table.Column<string>(maxLength: 32, nullable: false),
                    DrawdownPercentAtTrigger = table.Column<decimal>(precision: 18, scale: 8, nullable: false),
                    PortfolioValueAtTrigger  = table.Column<decimal>(precision: 18, scale: 2, nullable: false),
                    PeakEquity               = table.Column<decimal>(precision: 18, scale: 2, nullable: false),
                    ActionsTaken             = table.Column<string>(maxLength: 4000, nullable: false),
                    Reason                   = table.Column<string>(maxLength: 64, nullable: false),
                    PausedStrategiesJson     = table.Column<string>(maxLength: 8000, nullable: true)
                },
                constraints: t => t.PrimaryKey("PK_DeleverageEvents", x => x.Id));

            migrationBuilder.CreateTable(
                name: "FillAnalyses",
                columns: table => new
                {
                    Id                       = table.Column<Guid>(nullable: false),
                    OrderId                  = table.Column<string>(maxLength: 256, nullable: false),
                    Symbol                   = table.Column<string>(maxLength: 32, nullable: false),
                    ExpectedFillPrice        = table.Column<decimal>(precision: 18, scale: 8, nullable: false),
                    ActualFillPrice          = table.Column<decimal>(precision: 18, scale: 8, nullable: false),
                    SlippageBps              = table.Column<decimal>(precision: 18, scale: 4, nullable: false),
                    EstimatedSlippageBps     = table.Column<decimal>(precision: 18, scale: 4, nullable: false),
                    SlippageDeviationBps     = table.Column<decimal>(precision: 18, scale: 4, nullable: false),
                    ActualTotalCostDollars   = table.Column<decimal>(precision: 18, scale: 8, nullable: false),
                    EstimatedTotalCostDollars = table.Column<decimal>(precision: 18, scale: 8, nullable: false),
                    CostDeviationDollars     = table.Column<decimal>(precision: 18, scale: 8, nullable: false),
                    OrderToFillLatency       = table.Column<TimeSpan>(nullable: false),
                    Broker                   = table.Column<string>(maxLength: 32, nullable: false),
                    AnalyzedAt               = table.Column<DateTimeOffset>(nullable: false)
                },
                constraints: t => t.PrimaryKey("PK_FillAnalyses", x => x.Id));

            migrationBuilder.CreateTable(
                name: "PerformanceSnapshots",
                columns: table => new
                {
                    Id                 = table.Column<Guid>(nullable: false),
                    TotalEquity        = table.Column<decimal>(precision: 18, scale: 2, nullable: false),
                    DailyPnl           = table.Column<decimal>(precision: 18, scale: 2, nullable: false),
                    DailyReturnPercent = table.Column<decimal>(precision: 18, scale: 8, nullable: false),
                    CumulativeReturn   = table.Column<decimal>(precision: 18, scale: 8, nullable: false),
                    CurrentDrawdown    = table.Column<decimal>(precision: 18, scale: 8, nullable: false),
                    OpenPositionCount  = table.Column<int>(nullable: false),
                    Timestamp          = table.Column<DateTimeOffset>(nullable: false)
                },
                constraints: t => t.PrimaryKey("PK_PerformanceSnapshots", x => x.Id));

            migrationBuilder.CreateTable(
                name: "PortfolioExposures",
                columns: table => new
                {
                    Id                   = table.Column<Guid>(nullable: false),
                    TotalPortfolioValue  = table.Column<decimal>(precision: 18, scale: 2, nullable: false),
                    NetExposureDollars   = table.Column<decimal>(precision: 18, scale: 2, nullable: false),
                    NetExposurePercent   = table.Column<decimal>(precision: 18, scale: 8, nullable: false),
                    GrossExposureDollars = table.Column<decimal>(precision: 18, scale: 2, nullable: false),
                    GrossExposurePercent = table.Column<decimal>(precision: 18, scale: 8, nullable: false),
                    PortfolioBeta        = table.Column<decimal>(precision: 18, scale: 8, nullable: false),
                    StockExposurePercent = table.Column<decimal>(precision: 18, scale: 8, nullable: false),
                    CryptoExposurePercent = table.Column<decimal>(precision: 18, scale: 8, nullable: false),
                    CashPercent          = table.Column<decimal>(precision: 18, scale: 8, nullable: false),
                    AssetBreakdownJson   = table.Column<string>(maxLength: 65535, nullable: false),
                    SectorBreakdownJson  = table.Column<string>(maxLength: 65535, nullable: false),
                    SnapshotAt           = table.Column<DateTimeOffset>(nullable: false)
                },
                constraints: t => t.PrimaryKey("PK_PortfolioExposures", x => x.Id));

            migrationBuilder.CreateTable(
                name: "RiskBudgets",
                columns: table => new
                {
                    Id                  = table.Column<Guid>(nullable: false),
                    StrategyId          = table.Column<Guid>(nullable: false),
                    StrategyName        = table.Column<string>(maxLength: 256, nullable: false),
                    AllocatedRiskPercent = table.Column<decimal>(precision: 18, scale: 8, nullable: false),
                    UsedRiskPercent     = table.Column<decimal>(precision: 18, scale: 8, nullable: false),
                    RemainingRiskPercent = table.Column<decimal>(precision: 18, scale: 8, nullable: false),
                    CalculatedAt        = table.Column<DateTimeOffset>(nullable: false)
                },
                constraints: t => t.PrimaryKey("PK_RiskBudgets", x => x.Id));

            migrationBuilder.CreateTable(
                name: "SlippageRecords",
                columns: table => new
                {
                    Id                    = table.Column<Guid>(nullable: false),
                    OrderId               = table.Column<string>(maxLength: 256, nullable: false),
                    Symbol                = table.Column<string>(maxLength: 32, nullable: false),
                    Broker                = table.Column<string>(maxLength: 32, nullable: false),
                    ExpectedSlippageBps   = table.Column<decimal>(precision: 18, scale: 4, nullable: false),
                    ActualSlippageBps     = table.Column<decimal>(precision: 18, scale: 4, nullable: false),
                    DeviationBps          = table.Column<decimal>(precision: 18, scale: 4, nullable: false),
                    NotionalValue         = table.Column<decimal>(precision: 18, scale: 2, nullable: false),
                    RecordedAt            = table.Column<DateTimeOffset>(nullable: false)
                },
                constraints: t => t.PrimaryKey("PK_SlippageRecords", x => x.Id));

            migrationBuilder.CreateTable(
                name: "Strategies",
                columns: table => new
                {
                    Id             = table.Column<Guid>(nullable: false),
                    Name           = table.Column<string>(maxLength: 256, nullable: false),
                    Description    = table.Column<string>(maxLength: 2000, nullable: true),
                    AssetClass     = table.Column<string>(maxLength: 32, nullable: false),
                    Broker         = table.Column<string>(maxLength: 32, nullable: false),
                    IsActive       = table.Column<bool>(nullable: false),
                    CreatedAt      = table.Column<DateTimeOffset>(nullable: false),
                    LastModifiedAt = table.Column<DateTimeOffset>(nullable: true)
                },
                constraints: t => t.PrimaryKey("PK_Strategies", x => x.Id));

            migrationBuilder.CreateTable(
                name: "VolatilityTargets",
                columns: table => new
                {
                    Id                   = table.Column<Guid>(nullable: false),
                    TargetAnnualizedVol  = table.Column<decimal>(precision: 18, scale: 8, nullable: false),
                    RealizedAnnualizedVol = table.Column<decimal>(precision: 18, scale: 8, nullable: false),
                    VolMultiplier        = table.Column<decimal>(precision: 18, scale: 8, nullable: false),
                    PreviousRealizedVol  = table.Column<decimal>(precision: 18, scale: 8, nullable: false),
                    IsVolExpanding       = table.Column<bool>(nullable: false),
                    IsVolContracting     = table.Column<bool>(nullable: false),
                    VolRegime            = table.Column<int>(nullable: false),
                    CalculatedAt         = table.Column<DateTimeOffset>(nullable: false)
                },
                constraints: t => t.PrimaryKey("PK_VolatilityTargets", x => x.Id));

            // ── Dependent tables ──────────────────────────────────────────────────

            migrationBuilder.CreateTable(
                name: "AlertEvents",
                columns: table => new
                {
                    Id              = table.Column<Guid>(nullable: false),
                    AlertRuleId     = table.Column<Guid>(nullable: false),
                    RuleName        = table.Column<string>(maxLength: 256, nullable: true),
                    Severity        = table.Column<string>(maxLength: 32, nullable: false),
                    Message         = table.Column<string>(maxLength: 2000, nullable: true),
                    CurrentValue    = table.Column<decimal>(precision: 18, scale: 8, nullable: false),
                    ThresholdValue  = table.Column<decimal>(precision: 18, scale: 8, nullable: false),
                    TriggeredAt     = table.Column<DateTimeOffset>(nullable: false),
                    IsAcknowledged  = table.Column<bool>(nullable: false),
                    IsDelivered     = table.Column<bool>(nullable: false),
                    DeliveryError   = table.Column<string>(maxLength: 2000, nullable: true)
                },
                constraints: t =>
                {
                    t.PrimaryKey("PK_AlertEvents", x => x.Id);
                    t.ForeignKey("FK_AlertEvents_AlertRules_AlertRuleId",
                        x => x.AlertRuleId, "AlertRules", "Id", onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AiAnalysisReports",
                columns: table => new
                {
                    Id                    = table.Column<Guid>(nullable: false),
                    BacktestResultId      = table.Column<Guid>(nullable: false),
                    OverallAssessment     = table.Column<string>(maxLength: 4000, nullable: true),
                    OverfittingRisk       = table.Column<string>(maxLength: 32, nullable: true),
                    OverfittingExplanation = table.Column<string>(maxLength: 4000, nullable: true),
                    RegimeAnalysis        = table.Column<string>(maxLength: 8000, nullable: true),
                    PlainEnglishSummary   = table.Column<string>(maxLength: 8000, nullable: true),
                    DeploymentReadiness   = table.Column<double>(nullable: false),
                    EstimatedCost         = table.Column<decimal>(precision: 18, scale: 8, nullable: false),
                    Strengths             = table.Column<string>(maxLength: 8000, nullable: false),
                    Weaknesses            = table.Column<string>(maxLength: 8000, nullable: false),
                    ParameterSuggestions  = table.Column<string>(maxLength: 8000, nullable: false),
                    CriticalWarnings      = table.Column<string>(maxLength: 8000, nullable: false),
                    CreatedAt             = table.Column<DateTimeOffset>(nullable: false)
                },
                constraints: t =>
                {
                    t.PrimaryKey("PK_AiAnalysisReports", x => x.Id);
                    t.ForeignKey("FK_AiAnalysisReports_BacktestResults_BacktestResultId",
                        x => x.BacktestResultId, "BacktestResults", "Id", onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BacktestMetrics",
                columns: table => new
                {
                    Id                    = table.Column<Guid>(nullable: false),
                    BacktestResultId      = table.Column<Guid>(nullable: false),
                    SharpeRatio           = table.Column<double>(nullable: false),
                    SortinoRatio          = table.Column<double>(nullable: false),
                    MaxDrawdown           = table.Column<double>(nullable: false),
                    WinRate               = table.Column<double>(nullable: false),
                    ProfitFactor          = table.Column<double>(nullable: false),
                    CalmarRatio           = table.Column<double>(nullable: false),
                    ValueAtRisk95         = table.Column<double>(nullable: false),
                    ExpectedShortfall95   = table.Column<double>(nullable: false),
                    Beta                  = table.Column<double>(nullable: false),
                    AnnualizedReturn      = table.Column<double>(nullable: false),
                    AnnualizedVolatility  = table.Column<double>(nullable: false),
                    TotalTrades           = table.Column<int>(nullable: false),
                    WinningTrades         = table.Column<int>(nullable: false),
                    LosingTrades          = table.Column<int>(nullable: false),
                    AverageWin            = table.Column<decimal>(precision: 18, scale: 8, nullable: false),
                    AverageLoss           = table.Column<decimal>(precision: 18, scale: 8, nullable: false),
                    LargestWin            = table.Column<decimal>(precision: 18, scale: 8, nullable: false),
                    LargestLoss           = table.Column<decimal>(precision: 18, scale: 8, nullable: false),
                    AverageHoldingPeriod  = table.Column<TimeSpan>(nullable: false),
                    CalculatedAt          = table.Column<DateTimeOffset>(nullable: false)
                },
                constraints: t =>
                {
                    t.PrimaryKey("PK_BacktestMetrics", x => x.Id);
                    t.ForeignKey("FK_BacktestMetrics_BacktestResults_BacktestResultId",
                        x => x.BacktestResultId, "BacktestResults", "Id", onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BacktestTrades",
                columns: table => new
                {
                    Id               = table.Column<Guid>(nullable: false),
                    BacktestResultId = table.Column<Guid>(nullable: false),
                    Symbol           = table.Column<string>(maxLength: 32, nullable: false),
                    EntryTime        = table.Column<DateTimeOffset>(nullable: false),
                    ExitTime         = table.Column<DateTimeOffset>(nullable: false),
                    EntryPrice       = table.Column<decimal>(precision: 18, scale: 8, nullable: false),
                    ExitPrice        = table.Column<decimal>(precision: 18, scale: 8, nullable: false),
                    Quantity         = table.Column<decimal>(precision: 18, scale: 8, nullable: false),
                    Side             = table.Column<string>(maxLength: 16, nullable: false),
                    ProfitLoss       = table.Column<decimal>(precision: 18, scale: 8, nullable: false),
                    ProfitLossPercent = table.Column<decimal>(precision: 18, scale: 8, nullable: false)
                },
                constraints: t =>
                {
                    t.PrimaryKey("PK_BacktestTrades", x => x.Id);
                    t.ForeignKey("FK_BacktestTrades_BacktestResults_BacktestResultId",
                        x => x.BacktestResultId, "BacktestResults", "Id", onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DailyReturns",
                columns: table => new
                {
                    Id                 = table.Column<Guid>(nullable: false),
                    BacktestResultId   = table.Column<Guid>(nullable: false),
                    Date               = table.Column<DateTimeOffset>(nullable: false),
                    Equity             = table.Column<decimal>(precision: 18, scale: 2, nullable: false),
                    DailyPnl           = table.Column<decimal>(precision: 18, scale: 2, nullable: false),
                    DailyReturnPercent = table.Column<decimal>(precision: 18, scale: 8, nullable: false),
                    CumulativeReturn   = table.Column<decimal>(precision: 18, scale: 8, nullable: false),
                    Drawdown           = table.Column<decimal>(precision: 18, scale: 8, nullable: false),
                    BenchmarkEquity    = table.Column<decimal>(precision: 18, scale: 2, nullable: false)
                },
                constraints: t =>
                {
                    t.PrimaryKey("PK_DailyReturns", x => x.Id);
                    t.ForeignKey("FK_DailyReturns_BacktestResults_BacktestResultId",
                        x => x.BacktestResultId, "BacktestResults", "Id", onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RegimeClassifications",
                columns: table => new
                {
                    Id               = table.Column<Guid>(nullable: false),
                    BacktestResultId = table.Column<Guid>(nullable: false),
                    Regime           = table.Column<string>(maxLength: 32, nullable: false),
                    StartDate        = table.Column<DateTimeOffset>(nullable: false),
                    EndDate          = table.Column<DateTimeOffset>(nullable: false),
                    DurationDays     = table.Column<int>(nullable: false),
                    AnnualizedReturn = table.Column<double>(nullable: false),
                    SharpeRatio      = table.Column<double>(nullable: false),
                    MaxDrawdown      = table.Column<double>(nullable: false),
                    Volatility       = table.Column<double>(nullable: false),
                    TradeCount       = table.Column<int>(nullable: false),
                    WinRate          = table.Column<double>(nullable: false)
                },
                constraints: t =>
                {
                    t.PrimaryKey("PK_RegimeClassifications", x => x.Id);
                    t.ForeignKey("FK_RegimeClassifications_BacktestResults_BacktestResultId",
                        x => x.BacktestResultId, "BacktestResults", "Id", onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WalkForwardResults",
                columns: table => new
                {
                    Id               = table.Column<Guid>(nullable: false),
                    BacktestResultId = table.Column<Guid>(nullable: false),
                    WindowIndex      = table.Column<int>(nullable: false),
                    InSampleStart    = table.Column<DateTimeOffset>(nullable: false),
                    InSampleEnd      = table.Column<DateTimeOffset>(nullable: false),
                    OutOfSampleStart = table.Column<DateTimeOffset>(nullable: false),
                    OutOfSampleEnd   = table.Column<DateTimeOffset>(nullable: false),
                    InSampleSharpe   = table.Column<double>(nullable: false),
                    OutOfSampleSharpe = table.Column<double>(nullable: false),
                    InSampleReturn   = table.Column<double>(nullable: false),
                    OutOfSampleReturn = table.Column<double>(nullable: false),
                    InSampleDays     = table.Column<int>(nullable: false),
                    OutOfSampleDays  = table.Column<int>(nullable: false),
                    AnalyzedAt       = table.Column<DateTimeOffset>(nullable: false)
                },
                constraints: t =>
                {
                    t.PrimaryKey("PK_WalkForwardResults", x => x.Id);
                    t.ForeignKey("FK_WalkForwardResults_BacktestResults_BacktestResultId",
                        x => x.BacktestResultId, "BacktestResults", "Id", onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id                  = table.Column<Guid>(nullable: false),
                    ExternalOrderId     = table.Column<string>(maxLength: 256, nullable: false),
                    ClientOrderId       = table.Column<string>(maxLength: 256, nullable: false),
                    Symbol              = table.Column<string>(maxLength: 32, nullable: false),
                    Side                = table.Column<string>(maxLength: 16, nullable: false),
                    Type                = table.Column<string>(maxLength: 32, nullable: false),
                    Status              = table.Column<string>(maxLength: 32, nullable: false),
                    Quantity            = table.Column<decimal>(precision: 18, scale: 8, nullable: false),
                    LimitPrice          = table.Column<decimal>(precision: 18, scale: 8, nullable: true),
                    StopPrice           = table.Column<decimal>(precision: 18, scale: 8, nullable: true),
                    FilledQuantity      = table.Column<decimal>(precision: 18, scale: 8, nullable: false),
                    FilledAveragePrice  = table.Column<decimal>(precision: 18, scale: 8, nullable: true),
                    Broker              = table.Column<string>(maxLength: 32, nullable: false),
                    AssetClass          = table.Column<string>(maxLength: 32, nullable: false),
                    RejectReason        = table.Column<string>(maxLength: 1000, nullable: true),
                    CreatedAt           = table.Column<DateTimeOffset>(nullable: false),
                    FilledAt            = table.Column<DateTimeOffset>(nullable: true),
                    CancelledAt         = table.Column<DateTimeOffset>(nullable: true)
                },
                constraints: t => t.PrimaryKey("PK_Orders", x => x.Id));

            migrationBuilder.CreateTable(
                name: "StrategyParameters",
                columns: table => new
                {
                    Id          = table.Column<Guid>(nullable: false),
                    StrategyId  = table.Column<Guid>(nullable: false),
                    Name        = table.Column<string>(maxLength: 128, nullable: false),
                    Description = table.Column<string>(maxLength: 512, nullable: true),
                    Value       = table.Column<decimal>(precision: 18, scale: 8, nullable: false),
                    MinValue    = table.Column<decimal>(precision: 18, scale: 8, nullable: true),
                    MaxValue    = table.Column<decimal>(precision: 18, scale: 8, nullable: true),
                    StepSize    = table.Column<decimal>(precision: 18, scale: 8, nullable: true)
                },
                constraints: t =>
                {
                    t.PrimaryKey("PK_StrategyParameters", x => x.Id);
                    t.ForeignKey("FK_StrategyParameters_Strategies_StrategyId",
                        x => x.StrategyId, "Strategies", "Id", onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StrategyVersions",
                columns: table => new
                {
                    Id                 = table.Column<Guid>(nullable: false),
                    StrategyId         = table.Column<Guid>(nullable: false),
                    VersionNumber      = table.Column<int>(nullable: false),
                    ChangeNotes        = table.Column<string>(maxLength: 2000, nullable: true),
                    ParametersSnapshot = table.Column<string>(maxLength: 8000, nullable: true),
                    CreatedAt          = table.Column<DateTimeOffset>(nullable: false)
                },
                constraints: t =>
                {
                    t.PrimaryKey("PK_StrategyVersions", x => x.Id);
                    t.ForeignKey("FK_StrategyVersions_Strategies_StrategyId",
                        x => x.StrategyId, "Strategies", "Id", onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Fills",
                columns: table => new
                {
                    Id             = table.Column<Guid>(nullable: false),
                    OrderId        = table.Column<Guid>(nullable: false),
                    ExternalFillId = table.Column<string>(maxLength: 256, nullable: false),
                    Price          = table.Column<decimal>(precision: 18, scale: 8, nullable: false),
                    Quantity       = table.Column<decimal>(precision: 18, scale: 8, nullable: false),
                    Commission     = table.Column<decimal>(precision: 18, scale: 8, nullable: false),
                    FilledAt       = table.Column<DateTimeOffset>(nullable: false)
                },
                constraints: t =>
                {
                    t.PrimaryKey("PK_Fills", x => x.Id);
                    t.ForeignKey("FK_Fills_Orders_OrderId",
                        x => x.OrderId, "Orders", "Id", onDelete: ReferentialAction.Cascade);
                });

            // ── Indexes ───────────────────────────────────────────────────────────

            migrationBuilder.CreateIndex("IX_BacktestResults_ExternalBacktestId", "BacktestResults", "ExternalBacktestId");
            migrationBuilder.CreateIndex("IX_BacktestResults_ProjectId", "BacktestResults", "ProjectId");
            migrationBuilder.CreateIndex("IX_BacktestResults_CreatedAt", "BacktestResults", "CreatedAt");
            migrationBuilder.CreateIndex("IX_BacktestTrades_BacktestResultId", "BacktestTrades", "BacktestResultId");
            migrationBuilder.CreateIndex("IX_BacktestTrades_Symbol", "BacktestTrades", "Symbol");
            migrationBuilder.CreateIndex("IX_DailyReturns_BacktestResultId", "DailyReturns", "BacktestResultId");
            migrationBuilder.CreateIndex("IX_DailyReturns_Date", "DailyReturns", "Date");
            migrationBuilder.CreateIndex("IX_BacktestMetrics_BacktestResultId", "BacktestMetrics", "BacktestResultId", unique: true);
            migrationBuilder.CreateIndex("IX_Strategies_Name", "Strategies", "Name");
            migrationBuilder.CreateIndex("IX_Strategies_CreatedAt", "Strategies", "CreatedAt");
            migrationBuilder.CreateIndex("IX_StrategyParameters_StrategyId", "StrategyParameters", "StrategyId");
            migrationBuilder.CreateIndex("IX_StrategyVersions_StrategyId", "StrategyVersions", "StrategyId");
            migrationBuilder.CreateIndex("IX_Orders_ExternalOrderId", "Orders", "ExternalOrderId");
            migrationBuilder.CreateIndex("IX_Orders_ClientOrderId", "Orders", "ClientOrderId");
            migrationBuilder.CreateIndex("IX_Orders_Symbol", "Orders", "Symbol");
            migrationBuilder.CreateIndex("IX_Orders_Status", "Orders", "Status");
            migrationBuilder.CreateIndex("IX_Orders_CreatedAt", "Orders", "CreatedAt");
            migrationBuilder.CreateIndex("IX_Fills_OrderId", "Fills", "OrderId");
            migrationBuilder.CreateIndex("IX_PerformanceSnapshots_Timestamp", "PerformanceSnapshots", "Timestamp");
            migrationBuilder.CreateIndex("IX_AiAnalysisReports_BacktestResultId", "AiAnalysisReports", "BacktestResultId");
            migrationBuilder.CreateIndex("IX_AiAnalysisReports_CreatedAt", "AiAnalysisReports", "CreatedAt");
            migrationBuilder.CreateIndex("IX_RegimeClassifications_BacktestResultId", "RegimeClassifications", "BacktestResultId");
            migrationBuilder.CreateIndex("IX_RegimeClassifications_StartDate", "RegimeClassifications", "StartDate");
            migrationBuilder.CreateIndex("IX_WalkForwardResults_BacktestResultId", "WalkForwardResults", "BacktestResultId");
            migrationBuilder.CreateIndex("IX_AlertRules_IsActive", "AlertRules", "IsActive");
            migrationBuilder.CreateIndex("IX_AlertRules_CreatedAt", "AlertRules", "CreatedAt");
            migrationBuilder.CreateIndex("IX_AlertEvents_AlertRuleId", "AlertEvents", "AlertRuleId");
            migrationBuilder.CreateIndex("IX_AlertEvents_TriggeredAt", "AlertEvents", "TriggeredAt");
            migrationBuilder.CreateIndex("IX_AlertEvents_Severity", "AlertEvents", "Severity");
            migrationBuilder.CreateIndex("IX_AlertChannels_ChannelType", "AlertChannels", "ChannelType");
            migrationBuilder.CreateIndex("IX_VolatilityTargets_CalculatedAt", "VolatilityTargets", "CalculatedAt");
            migrationBuilder.CreateIndex("IX_DeleverageEvents_TriggeredAt", "DeleverageEvents", "TriggeredAt");
            migrationBuilder.CreateIndex("IX_DeleverageEvents_NewLevel", "DeleverageEvents", "NewLevel");
            migrationBuilder.CreateIndex("IX_RiskBudgets_StrategyId", "RiskBudgets", "StrategyId");
            migrationBuilder.CreateIndex("IX_RiskBudgets_CalculatedAt", "RiskBudgets", "CalculatedAt");
            migrationBuilder.CreateIndex("IX_PortfolioExposures_SnapshotAt", "PortfolioExposures", "SnapshotAt");
            migrationBuilder.CreateIndex("IX_CorrelationSnapshots_SnapshotAt", "CorrelationSnapshots", "SnapshotAt");
            migrationBuilder.CreateIndex("IX_FillAnalyses_OrderId", "FillAnalyses", "OrderId");
            migrationBuilder.CreateIndex("IX_FillAnalyses_Symbol", "FillAnalyses", "Symbol");
            migrationBuilder.CreateIndex("IX_FillAnalyses_AnalyzedAt", "FillAnalyses", "AnalyzedAt");
            migrationBuilder.CreateIndex("IX_SlippageRecords_OrderId", "SlippageRecords", "OrderId");
            migrationBuilder.CreateIndex("IX_SlippageRecords_Symbol", "SlippageRecords", "Symbol");
            migrationBuilder.CreateIndex("IX_SlippageRecords_RecordedAt", "SlippageRecords", "RecordedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("Fills");
            migrationBuilder.DropTable("Orders");
            migrationBuilder.DropTable("StrategyParameters");
            migrationBuilder.DropTable("StrategyVersions");
            migrationBuilder.DropTable("Strategies");
            migrationBuilder.DropTable("AlertEvents");
            migrationBuilder.DropTable("AlertChannels");
            migrationBuilder.DropTable("AlertRules");
            migrationBuilder.DropTable("AiAnalysisReports");
            migrationBuilder.DropTable("BacktestMetrics");
            migrationBuilder.DropTable("BacktestTrades");
            migrationBuilder.DropTable("DailyReturns");
            migrationBuilder.DropTable("RegimeClassifications");
            migrationBuilder.DropTable("WalkForwardResults");
            migrationBuilder.DropTable("BacktestResults");
            migrationBuilder.DropTable("PerformanceSnapshots");
            migrationBuilder.DropTable("VolatilityTargets");
            migrationBuilder.DropTable("DeleverageEvents");
            migrationBuilder.DropTable("RiskBudgets");
            migrationBuilder.DropTable("PortfolioExposures");
            migrationBuilder.DropTable("CorrelationSnapshots");
            migrationBuilder.DropTable("FillAnalyses");
            migrationBuilder.DropTable("SlippageRecords");
        }
    }
}
