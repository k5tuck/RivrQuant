# RivrQuant — Risk & Execution Engine Enhancement Prompt

## Context

This prompt enhances the existing RivrQuant platform with critical risk management, execution cost modeling, position sizing intelligence, and exposure tracking capabilities. These additions are required before any live trading deployment.

**This prompt assumes the base RivrQuant system already exists** (QuantConnect integration, Alpaca/Bybit brokers, AI analysis pipeline, Math.NET statistics, Next.js dashboard, SignalR, alerts). The components below integrate into and extend that existing architecture.

**Priority Classification:**
- **TIER 1 (Critical):** Blocks safe live trading. Must be fully implemented and tested.
- **TIER 2 (Important):** Should be operational within 30 days. Can run in parallel with paper trading.
- **TIER 3 (Architecture Only):** Define interfaces and stub implementations. Full logic deferred.

---

## New Solution Structure (Additions Only)

These files and folders are ADDED to the existing RivrQuant solution. Do not modify existing files unless explicitly stated in the "Integration Points" sections.

```
src/
├── RivrQuant.Domain/
│   ├── Models/
│   │   ├── Execution/                          # NEW — Tier 1
│   │   │   ├── ExecutionCostEstimate.cs         # Estimated slippage + commission + spread
│   │   │   ├── FillAnalysis.cs                  # Post-trade fill quality assessment
│   │   │   ├── SlippageRecord.cs                # Actual vs expected fill price
│   │   │   └── ExecutionReport.cs               # Aggregated execution quality over time
│   │   ├── Risk/                                # NEW — Tier 1 + 2
│   │   │   ├── PositionSizeRecommendation.cs    # Output of position sizing engine
│   │   │   ├── VolatilityTarget.cs              # Current vol target + realized vol
│   │   │   ├── DrawdownState.cs                 # Current drawdown tracking state
│   │   │   ├── DeleverageEvent.cs               # Record of automatic risk reduction
│   │   │   ├── RiskBudget.cs                    # Per-strategy risk allocation
│   │   │   └── RuinProbability.cs               # Monte Carlo ruin analysis result
│   │   ├── Exposure/                            # NEW — Tier 2
│   │   │   ├── PortfolioExposure.cs             # Net, gross, beta exposure snapshot
│   │   │   ├── AssetExposure.cs                 # Per-asset contribution to exposure
│   │   │   ├── SectorExposure.cs                # Sector-level aggregation
│   │   │   ├── CorrelationSnapshot.cs           # Cross-asset correlation matrix
│   │   │   └── FactorExposure.cs                # TIER 3 STUB — factor decomposition
│   │   ├── Allocation/                          # NEW — Tier 2 (interface) + Tier 3 (logic)
│   │   │   ├── StrategyAllocation.cs            # Capital allocated per strategy
│   │   │   ├── AllocationDecision.cs            # Allocator output with reasoning
│   │   │   └── StrategyPerformanceRank.cs       # Rolling performance ranking
│   │   └── Stress/                              # NEW — Tier 3 stubs
│   │       ├── StressScenario.cs                # Defined stress test scenario
│   │       ├── StressTestResult.cs              # Result of running a stress scenario
│   │       └── LiquidityProfile.cs              # Asset liquidity characteristics
│   ├── Enums/
│   │   ├── PositionSizingMethod.cs              # NEW — Kelly, VolTarget, FixedFractional, RiskParity
│   │   ├── DeleverageReason.cs                  # NEW — MaxDrawdown, VolSpike, CorrelationSpike, Manual
│   │   ├── ExposureType.cs                      # NEW — Net, Gross, Beta, Factor
│   │   └── StressScenarioType.cs                # NEW — TIER 3 STUB
│   ├── Interfaces/
│   │   ├── IExecutionCostModel.cs               # NEW — Tier 1
│   │   ├── IPositionSizer.cs                    # NEW — Tier 1
│   │   ├── IDrawdownManager.cs                  # NEW — Tier 1
│   │   ├── IVolatilityTargetEngine.cs           # NEW — Tier 1
│   │   ├── IExposureTracker.cs                  # NEW — Tier 2
│   │   ├── ICapitalAllocator.cs                 # NEW — Tier 2 interface, Tier 3 implementation
│   │   ├── IFillAnalyzer.cs                     # NEW — Tier 1
│   │   └── IStressTester.cs                     # NEW — Tier 3 stub
│   └── Exceptions/
│       ├── PositionSizingException.cs            # NEW
│       ├── ExecutionCostException.cs             # NEW
│       ├── ExposureLimitException.cs             # NEW
│       └── DeleverageException.cs                # NEW
│
├── RivrQuant.Infrastructure/
│   ├── Execution/                               # NEW — Tier 1
│   │   ├── SimpleSlippageModel.cs               # Fixed bps + volume-adjusted impact
│   │   ├── SpreadEstimator.cs                   # Bid-ask spread estimation from recent data
│   │   ├── CommissionCalculator.cs              # Per-broker commission calculation
│   │   ├── ExecutionCostAggregator.cs           # Combines slippage + spread + commission
│   │   ├── PostTradeFillAnalyzer.cs             # Compares expected vs actual fill price
│   │   └── ExecutionReportGenerator.cs          # Aggregated execution quality reporting
│   ├── Risk/                                    # NEW — Tier 1
│   │   ├── PositionSizing/
│   │   │   ├── KellyPositionSizer.cs            # Kelly criterion with configurable fraction
│   │   │   ├── VolatilityTargetSizer.cs         # Size positions to target portfolio vol
│   │   │   ├── FixedFractionalSizer.cs          # Fixed % of equity per trade
│   │   │   ├── RiskParitySizer.cs               # TIER 2 — Equal risk contribution
│   │   │   └── CompositePositionSizer.cs        # Combines multiple methods, takes minimum
│   │   ├── DrawdownManager.cs                   # Monitors drawdown, triggers deleveraging
│   │   ├── VolatilityTargetEngine.cs            # Calculates vol-adjusted position multiplier
│   │   ├── RuinProbabilityCalculator.cs         # Monte Carlo probability of ruin
│   │   └── OutOfSampleDecayTracker.cs           # TIER 2 — Track backtest-to-live decay
│   ├── Exposure/                                # NEW — Tier 2
│   │   ├── ExposureTracker.cs                   # Real-time net/gross/beta tracking
│   │   ├── CorrelationEngine.cs                 # Rolling cross-asset correlations
│   │   ├── SectorMapper.cs                      # Maps symbols to sectors (stocks)
│   │   └── FactorDecomposer.cs                  # TIER 3 STUB — factor model placeholder
│   ├── Allocation/                              # NEW — Tier 2 (framework) + Tier 3 (smart logic)
│   │   ├── EqualWeightAllocator.cs              # Simple equal allocation (baseline)
│   │   ├── SharpeWeightedAllocator.cs           # TIER 3 — Allocate proportional to rolling Sharpe
│   │   ├── MeanVarianceAllocator.cs             # TIER 3 — Markowitz optimization
│   │   └── AllocationOrchestrator.cs            # Tier 2 — Coordinates allocation decisions
│   └── Stress/                                  # NEW — Tier 3 stubs
│       ├── HistoricalStressTester.cs            # STUB — replay historical crises
│       ├── SyntheticStressTester.cs             # STUB — generate synthetic shocks
│       └── LiquidityStressModel.cs              # STUB — model liquidity evaporation
│
├── RivrQuant.Application/
│   ├── Services/
│   │   ├── ExecutionService.cs                  # NEW — Coordinates cost estimation + fill analysis
│   │   ├── RiskManagementService.cs             # NEW — Coordinates position sizing + drawdown + vol targeting
│   │   ├── ExposureService.cs                   # NEW — Coordinates exposure tracking + limits
│   │   └── AllocationService.cs                 # NEW — Coordinates capital allocation across strategies
│   ├── BackgroundJobs/
│   │   ├── DrawdownMonitorJob.cs                # NEW — Hangfire job, checks drawdown every 15 seconds
│   │   ├── VolatilityUpdateJob.cs               # NEW — Hangfire job, recalculates realized vol every 5 minutes
│   │   ├── ExposureSnapshotJob.cs               # NEW — Hangfire job, snapshots exposure every minute
│   │   ├── CorrelationUpdateJob.cs              # NEW — Hangfire job, recalculates correlations every hour
│   │   └── DecayTrackingJob.cs                  # NEW — Hangfire job, daily out-of-sample decay check
│
├── RivrQuant.Api/
│   ├── Controllers/
│   │   ├── RiskController.cs                    # NEW — Position sizing, drawdown state, vol targets
│   │   ├── ExecutionController.cs               # NEW — Execution cost estimates, fill quality reports
│   │   ├── ExposureController.cs                # NEW — Current exposure, correlation matrix
│   │   └── AllocationController.cs              # NEW — Strategy allocations, rebalance triggers
│
└── frontend/
    ├── app/
    │   ├── risk/
    │   │   ├── page.tsx                         # NEW — Risk management dashboard
    │   │   ├── position-sizing/
    │   │   │   └── page.tsx                     # NEW — Position sizing calculator + history
    │   │   ├── drawdown/
    │   │   │   └── page.tsx                     # NEW — Drawdown monitoring + deleverage history
    │   │   └── volatility/
    │   │       └── page.tsx                     # NEW — Volatility targeting dashboard
    │   ├── execution/
    │   │   ├── page.tsx                         # NEW — Execution quality dashboard
    │   │   └── fill-analysis/
    │   │       └── page.tsx                     # NEW — Fill quality analysis detail
    │   ├── exposure/
    │   │   ├── page.tsx                         # NEW — Exposure dashboard
    │   │   └── correlations/
    │   │       └── page.tsx                     # NEW — Correlation matrix + timeline
    │   └── allocation/
    │       └── page.tsx                         # NEW — Capital allocation dashboard
    ├── components/
    │   ├── risk/
    │   │   ├── PositionSizeCalculator.tsx        # NEW — Interactive position size calculator
    │   │   ├── DrawdownGauge.tsx                 # NEW — Visual drawdown meter with thresholds
    │   │   ├── DeleverageTimeline.tsx            # NEW — Timeline of deleverage events
    │   │   ├── VolatilityTargetChart.tsx         # NEW — Target vs realized vol over time
    │   │   ├── KellyFractionDisplay.tsx         # NEW — Current Kelly recommendation
    │   │   ├── RuinProbabilityCard.tsx           # NEW — Monte Carlo ruin probability
    │   │   └── RiskBudgetBar.tsx                # NEW — Per-strategy risk budget utilization
    │   ├── execution/
    │   │   ├── SlippageChart.tsx                 # NEW — Expected vs actual slippage over time
    │   │   ├── FillQualityTable.tsx              # NEW — Per-trade fill quality breakdown
    │   │   ├── ExecutionCostBreakdown.tsx        # NEW — Commission + spread + slippage pie chart
    │   │   └── CostImpactOnReturns.tsx          # NEW — Backtest return vs cost-adjusted return
    │   ├── exposure/
    │   │   ├── ExposureBarChart.tsx              # NEW — Net/gross exposure stacked bar
    │   │   ├── CorrelationHeatmap.tsx            # NEW — Cross-asset correlation matrix
    │   │   ├── BetaExposureGauge.tsx             # NEW — Portfolio beta vs target
    │   │   └── SectorDonutChart.tsx              # NEW — Sector allocation donut
    │   └── allocation/
    │       ├── AllocationSankey.tsx              # NEW — Capital flow from total → strategies → assets
    │       ├── StrategyRankingTable.tsx          # NEW — Rolling Sharpe ranking with allocation %
    │       └── RebalanceHistoryChart.tsx         # NEW — Allocation changes over time
```

---

## TIER 1: Execution Cost Modeling (CRITICAL)

### Purpose

Every backtest result and every live trade must account for real-world execution costs. Without this, backtested returns are fiction.

### IExecutionCostModel Interface

```csharp
public interface IExecutionCostModel
{
    /// <summary>
    /// Estimate total execution cost for a hypothetical order BEFORE placement.
    /// Used by position sizer to adjust expected returns and by backtest analyzer
    /// to produce cost-adjusted performance metrics.
    /// </summary>
    Task<ExecutionCostEstimate> EstimateCostAsync(
        string symbol,
        OrderSide side,
        decimal quantity,
        decimal currentPrice,
        BrokerType broker,
        CancellationToken ct);

    /// <summary>
    /// Calculate cost-adjusted backtest metrics by applying realistic execution
    /// costs to every trade in the backtest log.
    /// </summary>
    Task<BacktestMetrics> AdjustBacktestForCostsAsync(
        BacktestResult backtest,
        BrokerType broker,
        CancellationToken ct);
}
```

### ExecutionCostEstimate Model

```csharp
public sealed record ExecutionCostEstimate
{
    public required string Symbol { get; init; }
    public required decimal Quantity { get; init; }
    public required decimal CurrentPrice { get; init; }
    public required decimal NotionalValue { get; init; }           // Quantity * Price
    public required decimal EstimatedSlippageBps { get; init; }    // Basis points of slippage
    public required decimal EstimatedSlippageDollars { get; init; }
    public required decimal EstimatedSpreadCostBps { get; init; }  // Half-spread cost
    public required decimal EstimatedSpreadCostDollars { get; init; }
    public required decimal CommissionDollars { get; init; }        // Broker commission
    public required decimal TotalCostDollars { get; init; }        // Sum of all costs
    public required decimal TotalCostBps { get; init; }            // Total as basis points of notional
    public required decimal CostAsPercentOfPrice { get; init; }    // Total / NotionalValue * 100
    public required BrokerType Broker { get; init; }
    public required DateTimeOffset EstimatedAt { get; init; }
}
```

### SimpleSlippageModel Implementation

This is a practical slippage model suitable for retail-scale trading on Alpaca and Bybit. It does NOT attempt to model order book microstructure (that's premature at your scale). Instead it uses empirically-grounded fixed + variable cost components.

**Slippage formula:**

```
TotalSlippage(bps) = BaseSlippage(bps) + VolumeImpact(bps)

BaseSlippage:
  - Alpaca (stocks, market order):     2.0 bps
  - Alpaca (stocks, limit order):      0.5 bps
  - Bybit  (crypto, market order):     5.0 bps
  - Bybit  (crypto, limit order):      1.0 bps

VolumeImpact:
  - ParticipationRate = OrderQuantity / AvgDailyVolume
  - If ParticipationRate < 0.01 (less than 1% of daily volume): 0 bps
  - If ParticipationRate 0.01–0.05: ParticipationRate * 100 bps
  - If ParticipationRate > 0.05: ParticipationRate * 200 bps  (significant market impact)
  - If no volume data available: assume 3.0 bps additional (conservative)
```

**Spread estimation:**

```
SpreadCost(bps) = EstimatedHalfSpread

For stocks (Alpaca):
  - Large cap (SPY, AAPL, MSFT, etc.): 0.5 bps
  - Mid cap: 2.0 bps
  - Small cap: 5.0 bps
  - Default (unknown): 2.0 bps
  - Use Alpaca's latest quote endpoint to get real bid-ask when available

For crypto (Bybit):
  - BTC/USDT: 1.0 bps
  - ETH/USDT: 1.5 bps
  - Major alts (SOL, AVAX, etc.): 3.0 bps
  - Other pairs: 8.0 bps
  - Use Bybit's orderbook endpoint to get real spread when available
```

**Commission calculation:**

```
Alpaca:
  - Commission: $0.00 (commission-free)
  - SEC fee (sells only): $0.00278 per $1,000 notional (as of 2024, check current rate)
  - TAF fee (sells only): $0.000166 per share, max $8.30

Bybit:
  - Spot maker: 0.10% of notional
  - Spot taker: 0.10% of notional
  - Linear perpetual maker: 0.02% of notional
  - Linear perpetual taker: 0.055% of notional
  - Use maker rate for limit orders, taker rate for market orders
```

**Implementation requirements:**
- Cache average daily volume data (refresh every 24 hours) from Alpaca/Bybit market data endpoints
- Cache real-time bid-ask spread (refresh every 60 seconds for actively traded symbols)
- Log every cost estimate with full breakdown for audit trail
- Expose a method to bulk-adjust an entire backtest trade log with realistic costs

### PostTradeFillAnalyzer (IFillAnalyzer)

**Purpose:** After every live trade executes, compare the actual fill against what the execution cost model predicted. This builds a feedback loop that improves your cost estimates over time.

```csharp
public interface IFillAnalyzer
{
    /// <summary>
    /// Analyze a completed fill against the pre-trade cost estimate.
    /// Records the deviation and updates rolling accuracy metrics.
    /// </summary>
    Task<FillAnalysis> AnalyzeFillAsync(
        Order order,
        Fill fill,
        ExecutionCostEstimate preTradeEstimate,
        CancellationToken ct);

    /// <summary>
    /// Generate an execution quality report over a time period.
    /// Shows systematic biases in cost estimation.
    /// </summary>
    Task<ExecutionReport> GenerateReportAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        BrokerType? brokerFilter,
        CancellationToken ct);
}
```

**FillAnalysis model:**

```csharp
public sealed record FillAnalysis
{
    public required string OrderId { get; init; }
    public required string Symbol { get; init; }
    public required decimal ExpectedFillPrice { get; init; }      // Price at order creation time
    public required decimal ActualFillPrice { get; init; }         // Actual fill price from broker
    public required decimal SlippageBps { get; init; }             // Actual slippage experienced
    public required decimal EstimatedSlippageBps { get; init; }    // What model predicted
    public required decimal SlippageDeviationBps { get; init; }    // Actual - Estimated
    public required decimal ActualTotalCostDollars { get; init; }  // Real total cost
    public required decimal EstimatedTotalCostDollars { get; init; }
    public required decimal CostDeviationDollars { get; init; }
    public required TimeSpan OrderToFillLatency { get; init; }     // Time from order sent to fill received
    public required BrokerType Broker { get; init; }
    public required DateTimeOffset AnalyzedAt { get; init; }
}
```

**Behavior:**
- Every fill triggers automatic analysis
- Store all FillAnalysis records in database
- Calculate rolling metrics: mean slippage deviation, standard deviation, worst fill, best fill
- If rolling mean deviation exceeds 3 bps: trigger an alert ("Execution cost model may be inaccurate — actual slippage averaging {X}bps higher than estimated")
- Push FillAnalysis results via SignalR to the frontend

### Integration Points (Existing Code Changes)

**Modify `TradingOrchestrator`:**
- Before placing any order, call `IExecutionCostModel.EstimateCostAsync()` to get pre-trade cost estimate
- Store the estimate alongside the order
- After fill received, call `IFillAnalyzer.AnalyzeFillAsync()` with the stored estimate and actual fill
- If `CostAsPercentOfPrice > 1.0%`: log a warning (execution costs eating significant return)

**Modify `BacktestOrchestrator`:**
- After fetching any new backtest result from QuantConnect, run `IExecutionCostModel.AdjustBacktestForCostsAsync()` to produce cost-adjusted metrics
- Store BOTH raw and cost-adjusted metrics in the database
- Pass cost-adjusted metrics (not raw) to the AI analysis pipeline
- The AI analysis prompt should include: "Note: These metrics have been adjusted for estimated execution costs ({totalCostBps}bps average per trade). Raw backtest Sharpe was {rawSharpe}, cost-adjusted Sharpe is {adjustedSharpe}."

**Modify `ClaudePromptBuilder`:**
- Add a new section to the analysis prompt: "Execution Cost Impact"
- Include: average cost per trade, total cost drag on returns, cost-adjusted vs raw Sharpe delta
- Ask Claude: "Given the execution cost drag of {X}bps per trade, is this strategy's edge large enough to survive real-world trading costs? If the edge is less than 2x the estimated execution costs, flag this as high risk."

---

## TIER 1: Position Sizing Engine (CRITICAL)

### Purpose

Determine how many shares/contracts/coins to buy for each signal. This is the single most impactful risk management component. Wrong position sizing kills more strategies than bad signals.

### IPositionSizer Interface

```csharp
public interface IPositionSizer
{
    PositionSizingMethod Method { get; }

    /// <summary>
    /// Calculate recommended position size for a trade signal.
    /// Takes into account: portfolio state, volatility, drawdown state,
    /// execution costs, and the specific sizing method's logic.
    /// </summary>
    Task<PositionSizeRecommendation> CalculateSizeAsync(
        PositionSizeRequest request,
        CancellationToken ct);
}
```

**PositionSizeRequest:**

```csharp
public sealed record PositionSizeRequest
{
    public required string Symbol { get; init; }
    public required OrderSide Side { get; init; }
    public required decimal CurrentPrice { get; init; }
    public required decimal SignalConfidence { get; init; }          // 0.0–1.0 from strategy
    public required Portfolio CurrentPortfolio { get; init; }
    public required DrawdownState CurrentDrawdownState { get; init; }
    public required VolatilityTarget CurrentVolTarget { get; init; }
    public required ExecutionCostEstimate EstimatedCosts { get; init; }
    public required decimal? HistoricalWinRate { get; init; }        // For Kelly
    public required decimal? HistoricalAvgWinLossRatio { get; init; }// For Kelly
    public required decimal? AssetAnnualizedVol { get; init; }       // For vol targeting
}
```

**PositionSizeRecommendation:**

```csharp
public sealed record PositionSizeRecommendation
{
    public required string Symbol { get; init; }
    public required decimal RecommendedQuantity { get; init; }
    public required decimal RecommendedNotional { get; init; }      // Quantity * Price
    public required decimal PercentOfPortfolio { get; init; }       // Notional / PortfolioValue
    public required decimal RiskPerTrade { get; init; }             // Expected $ at risk
    public required PositionSizingMethod MethodUsed { get; init; }
    public required string Reasoning { get; init; }                  // Human-readable explanation
    public required bool WasReducedByDrawdown { get; init; }        // True if drawdown manager reduced size
    public required bool WasReducedByVolTarget { get; init; }       // True if vol target reduced size
    public required decimal PreAdjustmentQuantity { get; init; }    // Size before risk adjustments
    public required decimal DrawdownMultiplier { get; init; }       // 1.0 = no reduction, 0.5 = halved
    public required decimal VolatilityMultiplier { get; init; }     // 1.0 = no reduction
    public required DateTimeOffset CalculatedAt { get; init; }
}
```

### KellyPositionSizer

**The Kelly Criterion** determines the mathematically optimal bet size to maximize long-term growth rate.

**Formula:**

```
FullKelly = WinRate - ((1 - WinRate) / AvgWinLossRatio)

Where:
  WinRate = historical probability of a winning trade (0.0–1.0)
  AvgWinLossRatio = average winning trade $ / average losing trade $ (positive number)

FractionalKelly = FullKelly * KellyFraction

Where:
  KellyFraction = configurable safety multiplier (default: 0.25 = quarter Kelly)
  Quarter Kelly is standard practice — full Kelly is extremely aggressive and
  assumes your win rate and payoff ratio estimates are perfectly accurate (they never are)
```

**Implementation rules:**
- If `FullKelly <= 0`: return quantity 0 with reasoning "Kelly criterion negative — strategy has negative expected value at current parameters. Do not trade."
- If `FullKelly > 0.5`: cap at 0.5 and log warning "Full Kelly exceeds 50% — likely due to small sample size or overfit parameters"
- `KellyFraction` is configurable per-strategy (default 0.25, range 0.1–0.5)
- Requires minimum 30 historical trades to calculate win rate and avg win/loss ratio. Below 30 trades: return error with reasoning "Insufficient trade history for Kelly calculation. Need minimum 30 trades, have {N}."
- Convert Kelly fraction to position size: `Quantity = (PortfolioValue * FractionalKelly) / CurrentPrice`

### VolatilityTargetSizer

**Purpose:** Size positions so that the PORTFOLIO maintains a target annualized volatility, regardless of how volatile individual assets are.

**Formula:**

```
TargetPortfolioVol = configurable (default: 10% annualized for conservative, 15% for moderate, 20% for aggressive)
RealizedPortfolioVol = rolling 20-day annualized volatility of portfolio returns
AssetVol = rolling 20-day annualized volatility of the specific asset

VolMultiplier = TargetPortfolioVol / RealizedPortfolioVol
  - Capped at 2.0 (never more than 2x normal sizing even in very low vol)
  - Floored at 0.25 (never less than quarter size even in very high vol)

BasePositionSize = PortfolioValue * MaxSinglePositionPercent / CurrentPrice
AdjustedSize = BasePositionSize * VolMultiplier

Where:
  MaxSinglePositionPercent = configurable (default: 10% for stocks, 5% for crypto)
```

**Implementation rules:**
- Recalculate `RealizedPortfolioVol` every 5 minutes via `VolatilityUpdateJob`
- If vol data unavailable (new portfolio, < 5 days of returns): use conservative default multiplier of 0.5
- If `RealizedPortfolioVol > 2 * TargetPortfolioVol`: trigger alert "Portfolio volatility ({realized}%) exceeds 2x target ({target}%). Positions will be significantly reduced."
- Store `VolatilityTarget` snapshots in database for historical tracking
- Crypto assets use a separate (typically higher) vol target since crypto is inherently more volatile

### FixedFractionalSizer

**The simplest method — risk a fixed percentage of portfolio per trade.**

```
RiskPerTrade = PortfolioValue * RiskFraction
Quantity = RiskPerTrade / (CurrentPrice * StopLossPercent)

Where:
  RiskFraction = configurable (default: 1% of portfolio per trade)
  StopLossPercent = expected max loss on this trade (default: 5% if no stop loss defined)
```

**Implementation rules:**
- `RiskFraction` configurable per strategy (range 0.5%–3.0%, default 1.0%)
- If no stop loss is defined for the strategy: assume 5% stop and log warning
- This is the fallback method when there isn't enough data for Kelly or vol targeting

### CompositePositionSizer

**Purpose:** Runs ALL applicable position sizing methods and takes the MINIMUM recommended size. This is the primary sizer that `RiskManagementService` calls.

**Behavior:**
1. Run `KellyPositionSizer` (if sufficient trade history exists)
2. Run `VolatilityTargetSizer` (if vol data exists)
3. Run `FixedFractionalSizer` (always available)
4. Take the MINIMUM quantity across all methods that returned valid results
5. Apply drawdown multiplier (from `DrawdownManager`)
6. Apply execution cost sanity check: if estimated round-trip cost > 2% of position value, reduce size or warn
7. Apply hard caps:
   - No single position > 20% of portfolio (stocks) or 10% of portfolio (crypto)
   - No single order > 5% of average daily volume for that symbol
   - Minimum order size: $10 (below this, don't trade — costs dominate)
8. Return `PositionSizeRecommendation` with `Reasoning` explaining which method was binding and why

---

## TIER 1: Drawdown-Based Deleveraging (CRITICAL)

### Purpose

Automatically reduce position sizes and pause strategies when the portfolio is losing money. This is the circuit breaker that prevents catastrophic losses.

### IDrawdownManager Interface

```csharp
public interface IDrawdownManager
{
    /// <summary>
    /// Get current drawdown state: peak equity, current equity, drawdown %,
    /// active deleverage level, and which strategies are paused.
    /// </summary>
    Task<DrawdownState> GetCurrentStateAsync(CancellationToken ct);

    /// <summary>
    /// Calculate the position size multiplier based on current drawdown.
    /// Returns 1.0 when no drawdown, scales down as drawdown deepens.
    /// </summary>
    Task<decimal> GetDrawdownMultiplierAsync(CancellationToken ct);

    /// <summary>
    /// Called by DrawdownMonitorJob every 15 seconds. Updates drawdown state,
    /// triggers deleveraging actions if thresholds are breached.
    /// </summary>
    Task EvaluateAndActAsync(CancellationToken ct);
}
```

### DrawdownState Model

```csharp
public sealed record DrawdownState
{
    public required decimal PeakEquity { get; init; }
    public required decimal CurrentEquity { get; init; }
    public required decimal DrawdownPercent { get; init; }          // Negative number (e.g., -8.5)
    public required decimal DrawdownDollars { get; init; }
    public required DateTimeOffset PeakDate { get; init; }
    public required int DaysInDrawdown { get; init; }
    public required DeleverageLevel ActiveLevel { get; init; }
    public required decimal CurrentMultiplier { get; init; }        // 1.0 = full size, 0.5 = half, etc.
    public required IReadOnlyList<string> PausedStrategies { get; init; }
    public required DateTimeOffset LastUpdated { get; init; }
}
```

### Deleverage Levels (Configurable)

```
Level 0 — Normal (drawdown 0% to -5%)
  Multiplier: 1.0
  Action: None

Level 1 — Caution (drawdown -5% to -10%)
  Multiplier: 0.75
  Action: Reduce all new position sizes by 25%. Log warning.

Level 2 — Defensive (drawdown -10% to -15%)
  Multiplier: 0.50
  Action: Reduce all new position sizes by 50%.
          Pause lowest-performing strategy (by rolling 30-day Sharpe).
          Send email + SMS alert.

Level 3 — Critical (drawdown -15% to -20%)
  Multiplier: 0.25
  Action: Reduce all new position sizes by 75%.
          Pause all strategies except the single best performer.
          Send CRITICAL email + SMS alert.
          Close positions in paused strategies (market orders).

Level 4 — Emergency (drawdown > -20%)
  Multiplier: 0.0
  Action: HALT ALL TRADING.
          Close ALL positions across ALL brokers (market orders).
          Send EMERGENCY email + SMS alert.
          Require manual override to resume (set via API or dashboard).
          Log full portfolio state at time of emergency halt.
```

**Implementation rules:**
- Deleverage thresholds are configurable per-user in settings (the defaults above are conservative for < $1K capital)
- `DrawdownMonitorJob` runs every 15 seconds via Hangfire
- When transitioning between levels: record a `DeleverageEvent` in database with timestamp, previous level, new level, actions taken, portfolio state
- Recovery: when drawdown improves past a threshold, do NOT immediately restore full sizing. Require drawdown to improve by half the threshold gap before stepping back up. Example: if Level 2 triggered at -10%, don't restore to Level 1 until drawdown recovers to -7.5%. This prevents whipsawing.
- Peak equity is updated only when equity reaches a new all-time high. It never decreases.
- On system startup: recalculate peak equity from historical portfolio snapshots

### Integration Points (Existing Code Changes)

**Modify `TradingOrchestrator`:**
- Before placing any order, check `IDrawdownManager.GetDrawdownMultiplierAsync()`
- If multiplier is 0.0: reject the order immediately with reason "Trading halted — emergency drawdown level"
- If multiplier < 1.0: the `CompositePositionSizer` already applies this, but `TradingOrchestrator` should also verify and log
- Before placing any order, check if the strategy is in the `PausedStrategies` list: reject if paused

**Modify `PortfolioSnapshotJob`:**
- After each snapshot, update peak equity if current equity is a new high
- Persist peak equity to database (survives restarts)

---

## TIER 1: Volatility Targeting Engine (CRITICAL)

### IVolatilityTargetEngine Interface

```csharp
public interface IVolatilityTargetEngine
{
    /// <summary>
    /// Get current volatility target state: target vol, realized vol,
    /// current multiplier, and trend.
    /// </summary>
    Task<VolatilityTarget> GetCurrentTargetAsync(CancellationToken ct);

    /// <summary>
    /// Recalculate realized volatility from recent portfolio returns.
    /// Called by VolatilityUpdateJob every 5 minutes.
    /// </summary>
    Task<VolatilityTarget> RecalculateAsync(CancellationToken ct);
}
```

### VolatilityTarget Model

```csharp
public sealed record VolatilityTarget
{
    public required decimal TargetAnnualizedVol { get; init; }       // User's target (e.g., 0.10 = 10%)
    public required decimal RealizedAnnualizedVol { get; init; }     // Current rolling 20-day vol
    public required decimal VolMultiplier { get; init; }              // Target / Realized, capped
    public required decimal PreviousRealizedVol { get; init; }       // For trend detection
    public required bool IsVolExpanding { get; init; }                // Realized > Previous
    public required bool IsVolContracting { get; init; }             // Realized < Previous
    public required int VolRegime { get; init; }                      // 1=Low, 2=Normal, 3=High, 4=Extreme
    public required DateTimeOffset CalculatedAt { get; init; }
}
```

**Vol regime classification:**

```
Low:     RealizedVol < 0.5 * TargetVol
Normal:  RealizedVol between 0.5 * TargetVol and 1.5 * TargetVol
High:    RealizedVol between 1.5 * TargetVol and 2.5 * TargetVol
Extreme: RealizedVol > 2.5 * TargetVol
```

**Implementation:**
- Use Math.NET to calculate rolling 20-day standard deviation of daily portfolio returns, annualized (* sqrt(252) for stocks, * sqrt(365) for crypto-heavy portfolios)
- When `VolRegime` transitions to High or Extreme: trigger alert
- When `VolRegime` transitions from Extreme back to High: log recovery but keep reduced sizing for one additional update cycle (avoid premature scaling up)
- Persist every `VolatilityTarget` snapshot for the frontend chart

---

## TIER 2: Exposure Tracking

### IExposureTracker Interface

```csharp
public interface IExposureTracker
{
    /// <summary>
    /// Get current portfolio exposure breakdown.
    /// </summary>
    Task<PortfolioExposure> GetCurrentExposureAsync(CancellationToken ct);

    /// <summary>
    /// Get rolling cross-asset correlation matrix.
    /// </summary>
    Task<CorrelationSnapshot> GetCorrelationMatrixAsync(
        int lookbackDays,
        CancellationToken ct);

    /// <summary>
    /// Snapshot current exposure to database. Called by ExposureSnapshotJob.
    /// </summary>
    Task<PortfolioExposure> SnapshotAsync(CancellationToken ct);
}
```

### PortfolioExposure Model

```csharp
public sealed record PortfolioExposure
{
    public required decimal TotalPortfolioValue { get; init; }
    public required decimal NetExposureDollars { get; init; }       // Long - Short (absolute $)
    public required decimal NetExposurePercent { get; init; }       // Net / Portfolio
    public required decimal GrossExposureDollars { get; init; }     // Long + Short (absolute $)
    public required decimal GrossExposurePercent { get; init; }     // Gross / Portfolio
    public required decimal PortfolioBeta { get; init; }            // Weighted beta vs SPY
    public required decimal StockExposurePercent { get; init; }     // % of portfolio in stocks
    public required decimal CryptoExposurePercent { get; init; }    // % of portfolio in crypto
    public required decimal CashPercent { get; init; }              // Cash / Portfolio
    public required IReadOnlyList<AssetExposure> AssetBreakdown { get; init; }
    public required IReadOnlyList<SectorExposure> SectorBreakdown { get; init; }
    public required DateTimeOffset SnapshotAt { get; init; }
}
```

**Implementation:**
- Calculate `PortfolioBeta` as the weighted sum of individual asset betas vs SPY. For crypto assets, use 90-day rolling correlation with SPY as a proxy beta.
- `SectorMapper` uses a hardcoded dictionary of the top 500 stocks → GICS sector. For unknown symbols, query Alpaca's asset endpoint for sector info.
- Sector breakdown only applies to stock positions. Crypto is reported as its own category.
- `CorrelationEngine` uses Math.NET to calculate Pearson correlation matrix from daily returns over the lookback period. Default lookback: 60 days.
- Store correlation snapshots hourly (via `CorrelationUpdateJob`) for the frontend timeline.
- Alert if correlation between any two held assets exceeds 0.85 ("High correlation between {A} and {B} ({corr}) — diversification reduced")

### Exposure Limits (Configurable, Checked by RiskManagementService)

```
MaxNetExposurePercent: 150%    (default — can be > 100% with leverage on Bybit)
MaxGrossExposurePercent: 200%
MaxSingleAssetPercent: 20% (stocks), 10% (crypto)
MaxSectorPercent: 40%
MaxCryptoPercent: 30%          (of total portfolio)
MaxPortfolioBeta: 1.5
```

If any limit is breached: log warning, trigger alert, and block new orders that would increase exposure in the breached dimension.

---

## TIER 2: Capital Allocation Framework

### ICapitalAllocator Interface

```csharp
public interface ICapitalAllocator
{
    /// <summary>
    /// Calculate how much capital each strategy should receive.
    /// </summary>
    Task<IReadOnlyList<StrategyAllocation>> AllocateAsync(
        decimal totalCapital,
        IReadOnlyList<Strategy> activeStrategies,
        CancellationToken ct);

    /// <summary>
    /// Determine if rebalancing is needed based on drift from target allocations.
    /// </summary>
    Task<AllocationDecision> EvaluateRebalanceAsync(CancellationToken ct);
}
```

### EqualWeightAllocator (Tier 2 — Fully Implemented)

Simple baseline: divide capital equally among all active strategies.

```
AllocationPerStrategy = TotalCapital / NumberOfActiveStrategies
```

**Rules:**
- Minimum allocation per strategy: $100 (below this, exclude the strategy and redistribute)
- If a strategy is paused by the drawdown manager, its allocation is redistributed to remaining active strategies
- Log allocation decisions with reasoning

### SharpeWeightedAllocator (Tier 3 — STUB ONLY)

```csharp
/// <summary>
/// TIER 3 — NOT YET IMPLEMENTED.
/// Will allocate capital proportional to each strategy's rolling 60-day Sharpe ratio.
/// Strategies with negative Sharpe receive zero allocation.
/// Requires minimum 60 days of live or paper trading data per strategy.
/// </summary>
public class SharpeWeightedAllocator : ICapitalAllocator
{
    public Task<IReadOnlyList<StrategyAllocation>> AllocateAsync(
        decimal totalCapital,
        IReadOnlyList<Strategy> activeStrategies,
        CancellationToken ct)
    {
        throw new NotImplementedException(
            "SharpeWeightedAllocator is a Tier 3 feature. " +
            "Use EqualWeightAllocator until sufficient live trading data is available. " +
            "Target implementation: when 3+ strategies have 60+ days of paper/live data.");
    }

    public Task<AllocationDecision> EvaluateRebalanceAsync(CancellationToken ct)
    {
        throw new NotImplementedException("SharpeWeightedAllocator is a Tier 3 feature.");
    }
}
```

### MeanVarianceAllocator (Tier 3 — STUB ONLY)

Same pattern as above. Stub with clear `NotImplementedException` explaining prerequisites.

---

## TIER 2: Enhanced Monte Carlo & Decay Tracking

### Additions to MathNetStatisticsEngine

Add these methods to the existing `MathNetStatisticsEngine` class:

**`CalculateRuinProbability`:**

```
Input: dailyReturns[], initialCapital, ruinThreshold (e.g., lose 50% of capital), simulations (default 10,000), horizonDays (default 252)
Process:
  1. Fit daily returns to a distribution (use empirical distribution, not normal — fat tails matter)
  2. For each simulation:
     a. Start with initialCapital
     b. For each day in horizon:
        - Sample a return from the empirical distribution
        - Apply to capital
        - If capital < ruinThreshold: record ruin, break
  3. RuinProbability = simulations where ruin occurred / total simulations
Output: RuinProbability { Probability, MedianTimeToRuin, WorstCaseDrawdown, ConfidenceInterval95 }
```

**`CalculateMaxDrawdownProbability`:**

```
Input: dailyReturns[], threshold (e.g., -15%), simulations (default 10,000), horizonDays
Process: Similar to ruin probability but tracks max drawdown in each simulation
Output: Probability of experiencing drawdown worse than threshold within horizon
```

**`TrackOutOfSampleDecay`:**

```
Input: backtestMetrics (Sharpe, return, etc.), liveMetrics (same), rollingWindow (default 30 days)
Process:
  1. Calculate ratio: liveMetric / backtestMetric for each key metric
  2. Track this ratio over time (daily snapshots)
  3. If ratio < 0.5 for Sharpe (live Sharpe is less than half of backtest Sharpe): flag as "significant decay"
  4. If ratio < 0 for Sharpe (live Sharpe is negative while backtest was positive): flag as "strategy failure"
Output: OutOfSampleDecayReport { MetricRatios, TrendDirection, DaysSinceBacktest, DecayRate, IsDecaying, DecaySeverity }
```

### DecayTrackingJob (Background Job)

- Runs daily at market close (4:30 PM ET for stocks, midnight UTC for 24/7 crypto)
- For each deployed strategy: compare last 30 days of live performance against the backtest that justified deployment
- If `DecaySeverity` is "significant": trigger alert "Strategy {name} showing significant out-of-sample decay. Live Sharpe: {X}, Backtest Sharpe: {Y}. Consider pausing."
- If `DecaySeverity` is "failure": trigger CRITICAL alert AND automatically pause the strategy via the drawdown manager

---

## TIER 3: Stubs (Architecture Only)

### IStressTester

```csharp
/// <summary>
/// TIER 3 — Interface only. Implementation deferred.
///
/// When implemented, will provide:
/// 1. Historical replay: run current portfolio through 2008, 2020, 2022 market conditions
/// 2. Synthetic shocks: apply configurable % drops, vol spikes, correlation spikes
/// 3. Liquidity stress: model what happens when bid-ask spreads widen 10x
///
/// Prerequisites for implementation:
/// - Minimum 6 months of live trading data
/// - Historical market data cache (daily OHLCV for major indices back to 2007)
/// - Completed Tier 1 and Tier 2 risk infrastructure
/// </summary>
public interface IStressTester
{
    Task<StressTestResult> RunHistoricalScenarioAsync(
        StressScenario scenario,
        Portfolio currentPortfolio,
        CancellationToken ct);

    Task<StressTestResult> RunSyntheticShockAsync(
        decimal marketDropPercent,
        decimal volMultiplier,
        decimal spreadMultiplier,
        Portfolio currentPortfolio,
        CancellationToken ct);

    Task<IReadOnlyList<StressTestResult>> RunAllScenariosAsync(
        Portfolio currentPortfolio,
        CancellationToken ct);
}
```

### FactorDecomposer (Tier 3 Stub)

```csharp
/// <summary>
/// TIER 3 — Stub only. Factor exposure decomposition.
///
/// When implemented, will decompose portfolio returns into:
/// - Market (beta), Size (SMB), Value (HML), Momentum, Quality
/// - Crypto-specific: BTC beta, ETH beta, DeFi exposure
///
/// Prerequisites:
/// - Factor return data source (Kenneth French data library or similar)
/// - Minimum 60 days of portfolio return data
/// - Regression analysis infrastructure
/// </summary>
public class FactorDecomposer
{
    public Task<FactorExposure> DecomposeAsync(
        IReadOnlyList<DailyReturn> portfolioReturns,
        CancellationToken ct)
    {
        throw new NotImplementedException(
            "FactorDecomposer is a Tier 3 feature. " +
            "Requires factor return data source and 60+ days of portfolio returns. " +
            "Use PortfolioBeta from ExposureTracker as a simplified proxy.");
    }
}
```

---

## Frontend Specifications (New Pages)

### Risk Dashboard (`/risk`) — Main Page

**Layout:** Three-column grid at desktop, stacked on mobile.

**Top row:**
- **DrawdownGauge** — Circular gauge showing current drawdown %. Color coded: green (0 to -5%), yellow (-5% to -10%), orange (-10% to -15%), red (> -15%). Shows active deleverage level as a label. Includes "Days in Drawdown" counter.
- **VolatilityTargetChart** — Line chart (Recharts) showing target vol (flat line) vs realized vol (rolling line) over the last 90 days. Color-coded background bands for vol regimes (Low=blue, Normal=green, High=yellow, Extreme=red). Current multiplier displayed as large number.
- **RuinProbabilityCard** — Card showing Monte Carlo ruin probability as a percentage with confidence interval. Large number (e.g., "2.3%"), subtitle "probability of losing 50% within 1 year", small text "based on 10,000 simulations of current strategy performance". Refresh button to re-run Monte Carlo.

**Middle row:**
- **KellyFractionDisplay** — For each active strategy: show current full Kelly, fractional Kelly, and the resulting max position size. Bar chart comparing Kelly recommendation vs actual position sizes taken.
- **RiskBudgetBar** — Horizontal stacked bar for each strategy showing: allocated risk budget (from capital allocator) and used risk budget (current exposure). If used > allocated: bar turns red.

**Bottom row:**
- **DeleverageTimeline** — Timeline visualization showing all deleverage events. Each event shows: timestamp, trigger (drawdown/vol/manual), level change (e.g., "Level 0 → Level 2"), actions taken, portfolio value at time of event. Click to see full detail modal.

### Position Sizing Calculator (`/risk/position-sizing`)

**Interactive calculator form:**
- Symbol input (autocomplete from Alpaca/Bybit symbols)
- Side toggle (Buy/Sell)
- Signal confidence slider (0–100%)
- Override fields for: risk per trade %, Kelly fraction, vol target
- "Calculate" button

**Output panel:**
- Shows recommendation from each sizing method (Kelly, Vol Target, Fixed Fractional)
- Highlights which method is binding (the minimum)
- Shows all adjustments: drawdown multiplier, vol multiplier, execution cost impact
- Final recommended quantity + notional + % of portfolio
- Risk metrics for this trade: max loss at stop, impact on portfolio drawdown if max loss hit

### Execution Quality Dashboard (`/execution`)

**Top row:**
- **CostImpactOnReturns** — Bar chart comparing: Raw backtest return, Cost-adjusted backtest return, Live return. For each active strategy. Shows how much execution costs are eating.
- **ExecutionCostBreakdown** — Pie chart: Slippage vs Spread vs Commission as % of total execution costs. Toggle between strategies and time periods.

**Bottom row:**
- **SlippageChart** — Scatter plot: X = trade number, Y = actual slippage bps. Overlay: estimated slippage line. Shows model accuracy over time.
- **FillQualityTable** — Sortable table of recent fills: Timestamp, Symbol, Side, Expected Price, Actual Price, Slippage bps, Estimated bps, Deviation, Latency ms. Color-code rows where deviation > 2 bps.

### Exposure Dashboard (`/exposure`)

**Top row:**
- **ExposureBarChart** — Stacked bar: Long exposure (green), Short exposure (red), Net exposure (line overlay). Over last 30 days.
- **BetaExposureGauge** — Semicircular gauge showing portfolio beta. Target beta line. Color zones.

**Bottom row:**
- **SectorDonutChart** — Donut chart of sector allocation (stocks only). Inner ring: stock vs crypto split.
- **CorrelationHeatmap** — Interactive heatmap of cross-asset correlations. Click a cell to see rolling correlation chart for that pair over 90 days.

### Allocation Dashboard (`/allocation`)

- **AllocationSankey** — Sankey/flow diagram: Total Capital → Strategy A, Strategy B, etc. → Individual positions. Shows capital flow through the system.
- **StrategyRankingTable** — Table: Strategy Name, Rolling 30d Sharpe, Rolling 60d Sharpe, Current Allocation %, Target Allocation %, Drift %. Sort by any column. Strategies with negative Sharpe highlighted red.
- **RebalanceHistoryChart** — Stacked area chart showing allocation % per strategy over time. Vertical lines mark rebalance events.

---

## Enhanced AI Analysis Prompt Additions

### Modify ClaudePromptBuilder to Include New Data

When building the analysis prompt for Claude, add these additional sections:

**Section: Execution Cost Impact**
```
## Execution Cost Analysis
- Average estimated cost per trade: {avgCostBps} bps ({avgCostDollars} per trade)
- Total cost drag over backtest period: {totalCostDollars} ({totalCostBps} bps cumulative)
- Raw Sharpe Ratio: {rawSharpe}
- Cost-Adjusted Sharpe Ratio: {adjustedSharpe}
- Sharpe degradation from costs: {sharpeDelta}
- Cost as % of gross return: {costAsPercentOfReturn}%

Question: Is this strategy's edge large enough to survive real-world execution costs?
If the cost-adjusted Sharpe is below 0.5, flag this as likely unprofitable after costs.
If execution costs consume more than 40% of gross returns, flag as "edge too thin for reliable deployment."
```

**Section: Position Sizing Recommendations**
```
## Position Sizing Context
- Recommended sizing method: {method} (based on available data)
- Kelly fraction (if available): Full Kelly = {fullKelly}, Recommended = {fractionalKelly}
- Max recommended position size: {maxPositionPercent}% of portfolio
- Current portfolio volatility: {realizedVol}% annualized
- Volatility target: {targetVol}% annualized
- Vol multiplier: {volMultiplier}

Question: Based on this strategy's characteristics, what position sizing approach do you recommend?
Consider: win rate consistency, tail risk, drawdown profile, and regime sensitivity.
```

**Section: Ruin & Drawdown Risk**
```
## Risk Assessment
- Monte Carlo ruin probability (50% loss in 1 year): {ruinProb}%
- Probability of {maxDdThreshold}% drawdown in next 6 months: {ddProb}%
- Maximum observed drawdown in backtest: {maxDD}%
- Average drawdown duration: {avgDdDuration} days
- Longest drawdown: {longestDd} days

Question: Given these risk metrics, should this strategy be deployed with current parameters?
If ruin probability exceeds 5%, recommend parameter adjustments to reduce risk.
If max drawdown exceeds the user's configured Level 4 threshold, flag as "incompatible with current risk settings."
```

---

## Implementation Order

1. **Domain models + interfaces + exceptions** for all new types (Execution, Risk, Exposure, Allocation, Stress stubs)
2. **Enums** (PositionSizingMethod, DeleverageReason, ExposureType, StressScenarioType)
3. **SimpleSlippageModel + SpreadEstimator + CommissionCalculator + ExecutionCostAggregator** — Tier 1 execution cost modeling
4. **PostTradeFillAnalyzer** — Tier 1 fill quality tracking
5. **KellyPositionSizer + VolatilityTargetSizer + FixedFractionalSizer + CompositePositionSizer** — Tier 1 position sizing
6. **DrawdownManager + DeleverageEvent persistence** — Tier 1 drawdown protection
7. **VolatilityTargetEngine** — Tier 1 vol targeting
8. **Background jobs** (DrawdownMonitorJob, VolatilityUpdateJob) — Tier 1 real-time monitoring
9. **Integration into TradingOrchestrator and BacktestOrchestrator** — wire Tier 1 into existing flow
10. **Enhanced ClaudePromptBuilder** — add execution cost and risk sections to AI analysis
11. **ExposureTracker + CorrelationEngine + SectorMapper** — Tier 2 exposure
12. **EqualWeightAllocator + AllocationOrchestrator** — Tier 2 allocation baseline
13. **RuinProbabilityCalculator + OutOfSampleDecayTracker** — Tier 2 enhanced Monte Carlo
14. **Remaining background jobs** (ExposureSnapshotJob, CorrelationUpdateJob, DecayTrackingJob)
15. **API controllers** (RiskController, ExecutionController, ExposureController, AllocationController)
16. **Frontend pages and components** — all new dashboard sections
17. **Tier 3 stubs** — IStressTester, FactorDecomposer, SharpeWeightedAllocator, MeanVarianceAllocator
18. **Tests** — unit tests for all Tier 1 components (especially position sizing and drawdown logic)

---

## Post-Implementation Self-Critique Checklist (Risk-Specific)

- [ ] Can the system EVER place an order without checking drawdown state? (Must be impossible)
- [ ] Can the system EVER place an order without estimating execution costs? (Must be impossible)
- [ ] Can the system EVER place an order without running it through the CompositePositionSizer? (Must be impossible)
- [ ] If all external APIs fail (Alpaca, Bybit, QuantConnect), does the system default to SAFETY (halt trading) rather than CONTINUATION?
- [ ] Is the DrawdownMonitorJob resilient to its own failures? (If the job crashes, trading must be halted, not continued without monitoring)
- [ ] Are all deleverage events recorded with full context for post-mortem analysis?
- [ ] Can Level 4 (emergency halt) be triggered automatically AND manually from the dashboard?
- [ ] Does the KillSwitch button on the dashboard call BOTH Alpaca CloseAll AND Bybit CloseAll?
- [ ] Are execution cost estimates using REAL bid-ask data when available, not just defaults?
- [ ] Is the position sizing Reasoning field always populated with a human-readable explanation?
- [ ] Are all Tier 3 stubs throwing NotImplementedException with clear explanations of prerequisites?
- [ ] Can a user accidentally deploy to Bybit live (mainnet) without explicitly changing the BYBIT_USE_TESTNET flag?
