# RivrQuant — Addendum: Liquidation Audit & Queue Separation

## Context

This addendum adds two targeted enhancements to the existing RivrQuant platform. Apply after the base system and Risk & Execution Engine prompts are implemented.

---

## 1. Post-Liquidation Audit & Smart Exit Upgrade Path

### Purpose

When Level 4 (Emergency Halt) triggers and closes all positions via market orders, the system must record exactly what happened during liquidation. This data serves two purposes: immediate accountability (did the emergency exit work?) and future upgrade justification (do you need smarter liquidation logic as capital grows?).

### New Models

Add to `RivrQuant.Domain/Models/Execution/`:

```
LiquidationAudit.cs
LiquidationFillRecord.cs
```

**LiquidationAudit:**

```csharp
public sealed record LiquidationAudit
{
    public required Guid Id { get; init; }
    public required DateTimeOffset TriggeredAt { get; init; }
    public required DeleverageReason Reason { get; init; }
    public required decimal PortfolioValueAtTrigger { get; init; }
    public required decimal DrawdownPercentAtTrigger { get; init; }
    public required int TotalPositionsToClose { get; init; }
    public required int PositionsSuccessfullyClosed { get; init; }
    public required int PositionsFailedToClose { get; init; }
    public required decimal TotalSlippageDollars { get; init; }
    public required decimal TotalSlippageBps { get; init; }
    public required decimal WorstSingleFillSlippageBps { get; init; }
    public required string WorstSingleFillSymbol { get; init; }
    public required TimeSpan TotalLiquidationDuration { get; init; }
    public required TimeSpan SlowestFillDuration { get; init; }
    public required decimal PortfolioValueAfterLiquidation { get; init; }
    public required decimal LiquidationCostDollars { get; init; }     // Value lost to slippage + costs during exit
    public required decimal LiquidationCostPercent { get; init; }     // As % of pre-liquidation portfolio
    public required bool AllPositionsClosed { get; init; }
    public required IReadOnlyList<LiquidationFillRecord> Fills { get; init; }
    public required IReadOnlyList<string> Errors { get; init; }       // Any errors encountered during liquidation
    public required bool SmartExitRecommended { get; init; }          // True if slippage suggests upgrade needed
    public required string SmartExitReasoning { get; init; }          // Explanation of recommendation
}
```

**LiquidationFillRecord:**

```csharp
public sealed record LiquidationFillRecord
{
    public required string Symbol { get; init; }
    public required BrokerType Broker { get; init; }
    public required OrderSide Side { get; init; }
    public required decimal Quantity { get; init; }
    public required decimal PriceAtTrigger { get; init; }           // Market price when Level 4 fired
    public required decimal ActualFillPrice { get; init; }           // What we actually got
    public required decimal SlippageBps { get; init; }
    public required decimal SlippageDollars { get; init; }
    public required TimeSpan TimeToFill { get; init; }               // Order sent → fill received
    public required bool Filled { get; init; }                       // False if order failed/timed out
    public required string? ErrorMessage { get; init; }              // Null if successful
}
```

### Implementation: Modify DrawdownManager.EvaluateAndActAsync()

When Level 4 triggers, wrap the existing close-all-positions logic in an audit context:

```
1. Record TriggeredAt timestamp and portfolio state
2. Snapshot current market prices for all open positions (PriceAtTrigger)
3. Execute close-all on Alpaca (CloseAllPositionsAsync)
4. Execute close-all on Bybit (CloseAllPositionsAsync)
5. For each fill received:
   a. Record actual fill price
   b. Calculate slippage: (ActualFillPrice - PriceAtTrigger) / PriceAtTrigger * 10000 bps
   c. Record time to fill
6. Wait for all fills (timeout: 30 seconds per position)
7. For any position that didn't fill within timeout:
   a. Record as failed with error message
   b. Retry once with a new market order
   c. If still unfilled: record error, send CRITICAL alert "Position {symbol} failed to close during emergency liquidation. MANUAL INTERVENTION REQUIRED."
8. Calculate aggregate metrics (total slippage, worst fill, total duration)
9. Evaluate SmartExitRecommended:
   - If WorstSingleFillSlippageBps > 50: SmartExitRecommended = true, reasoning = "Single fill experienced {X}bps slippage. At current capital this cost ${Y}. As capital scales, consider IOC limit orders with safety bands."
   - If TotalLiquidationDuration > 10 seconds: SmartExitRecommended = true, reasoning = "Liquidation took {X}s. Staggered exits may reduce market impact at larger position sizes."
   - If any fill failed: SmartExitRecommended = true, reasoning = "One or more positions failed to close. Implement fallback order types (IOC limit → market with delay)."
   - Otherwise: SmartExitRecommended = false, reasoning = "Market exits executed within acceptable parameters at current capital level."
10. Persist LiquidationAudit to database
11. Send full audit summary via email alert
12. Push audit to frontend via SignalR
```

### Smart Exit Engine — Tier 3 Stub

Add to `RivrQuant.Domain/Interfaces/`:

```csharp
/// <summary>
/// TIER 3 — Interface only. Implement when capital exceeds $10K per position
/// or when LiquidationAudit data shows SmartExitRecommended = true on 3+ occasions.
///
/// When implemented, replaces raw market orders in Level 3-4 liquidation with:
///
/// 1. IOC (Immediate-Or-Cancel) limit orders with safety band:
///    - Place limit order at current price - SafetyBandBps (e.g., 25 bps below mid)
///    - If not filled immediately (IOC), escalate to market order
///    - Prevents catastrophic fills during flash crashes
///
/// 2. Staggered exits for large positions:
///    - If position > $5K notional: split into 3 tranches, 2 seconds apart
///    - Reduces instantaneous market impact
///    - Monitor slippage between tranches; if worsening, accelerate remaining
///
/// 3. Venue-aware routing:
///    - For crypto: check orderbook depth before exit
///    - If bid depth < 2x position size within 50bps of mid: use staggered exit
///    - If bid depth > 10x position size: market order is fine
///
/// Prerequisites:
/// - LiquidationAudit history showing slippage > 50bps on 3+ events
/// - OR single position sizes exceeding $10K notional
/// - Real-time orderbook depth data from Bybit WebSocket
/// </summary>
public interface ISmartExitEngine
{
    Task<IReadOnlyList<LiquidationFillRecord>> ExecuteEmergencyExitAsync(
        IReadOnlyList<Position> positionsToClose,
        CancellationToken ct);
}
```

Add stub implementation to `RivrQuant.Infrastructure/Execution/`:

```
SmartExitEngine.cs
```

```csharp
public class SmartExitEngine : ISmartExitEngine
{
    public Task<IReadOnlyList<LiquidationFillRecord>> ExecuteEmergencyExitAsync(
        IReadOnlyList<Position> positionsToClose,
        CancellationToken ct)
    {
        throw new NotImplementedException(
            "SmartExitEngine is a Tier 3 feature. " +
            "Current market-order liquidation is appropriate for positions under $10K. " +
            "Review LiquidationAudit history at /execution/liquidations to determine if upgrade is needed. " +
            "Implement when: (a) single position sizes exceed $10K, or " +
            "(b) 3+ liquidation audits show SmartExitRecommended = true.");
    }
}
```

### Frontend Addition

Add to the execution section of the dashboard:

**New page: `/execution/liquidations`**

- Chronological list of all `LiquidationAudit` records
- Each row: Timestamp, Trigger Reason, Positions Closed, Total Slippage, Duration, Smart Exit Recommended?
- Click to expand: full fill-by-fill breakdown table
- Summary card at top: "Lifetime liquidation cost: ${X} across {N} events. Average slippage: {Y}bps."
- If `SmartExitRecommended` is true on 3+ audits: show a persistent banner — "Your liquidation data suggests upgrading to smart exit logic. Review the pattern below."

**New component: `LiquidationAuditDetail.tsx`**
- Fill-by-fill table: Symbol, Broker, Side, Qty, Trigger Price, Fill Price, Slippage bps, Slippage $, Time to Fill
- Color-code rows: green (< 10 bps), yellow (10–50 bps), red (> 50 bps)
- Failed fills highlighted with error message

### New API Endpoint

Add to `ExecutionController`:

```
GET  /api/execution/liquidations              — List all LiquidationAudit records (paginated)
GET  /api/execution/liquidations/{id}         — Get single audit with full fill detail
GET  /api/execution/liquidations/summary      — Aggregate stats (total cost, avg slippage, count, smart exit recommendation)
```

---

## 2. Hangfire Queue Separation — Critical vs Non-Critical Jobs

### Purpose

Ensure that trading-critical background jobs (drawdown monitoring, volatility updates) NEVER compete for thread pool resources with non-critical jobs (AI analysis, reporting, correlation updates). If Claude's API takes 30 seconds to respond, it must not delay a drawdown check.

### Queue Definitions

Define three Hangfire queues with strict priority ordering:

```
Queue: "critical"    — Priority: HIGHEST
  Jobs: DrawdownMonitorJob, VolatilityUpdateJob
  Concurrency: Dedicated — 2 workers reserved exclusively for this queue
  Behavior: These jobs run on schedule NO MATTER WHAT. If the critical queue has work, it executes before anything else.

Queue: "trading"     — Priority: HIGH
  Jobs: PortfolioSnapshotJob, AlertEvaluationJob, ExposureSnapshotJob, LivePerformanceComparisonJob
  Concurrency: 3 workers
  Behavior: Important operational jobs that support trading decisions but aren't on the execution hot path.

Queue: "analysis"    — Priority: NORMAL
  Jobs: BacktestPollingJob, AI analysis jobs (BacktestOrchestrator triggered), CorrelationUpdateJob, DecayTrackingJob
  Concurrency: 2 workers
  Behavior: Research and analysis work. Can be delayed without operational impact. If Claude API is slow or QuantConnect is rate-limited, these jobs wait without affecting trading operations.
```

### Implementation

**Modify `Program.cs` (or wherever Hangfire is configured):**

```csharp
// Configure Hangfire with queue-specific worker counts
builder.Services.AddHangfireServer(options =>
{
    options.Queues = new[] { "critical", "trading", "analysis" };
    options.WorkerCount = 7; // 2 critical + 3 trading + 2 analysis
});
```

**However**, the above gives Hangfire 7 total workers that pull from queues in priority order — it does NOT guarantee dedicated workers per queue. To truly isolate critical jobs, configure multiple Hangfire servers in the same process:

```csharp
// Server 1: Critical jobs ONLY — guaranteed 2 dedicated workers
builder.Services.AddHangfireServer(options =>
{
    options.ServerName = "rivrquant-critical";
    options.Queues = new[] { "critical" };
    options.WorkerCount = 2;
});

// Server 2: Trading jobs — 3 workers, will also pick up critical overflow
builder.Services.AddHangfireServer(options =>
{
    options.ServerName = "rivrquant-trading";
    options.Queues = new[] { "trading" };
    options.WorkerCount = 3;
});

// Server 3: Analysis jobs — 2 workers, lowest priority
builder.Services.AddHangfireServer(options =>
{
    options.ServerName = "rivrquant-analysis";
    options.Queues = new[] { "analysis" };
    options.WorkerCount = 2;
});
```

This ensures that even if all 2 analysis workers are blocked waiting on Claude's API, the critical workers are completely unaffected.

**Modify each job registration to specify its queue:**

```csharp
// Critical queue — runs every 15 seconds, must never be delayed
RecurringJob.AddOrUpdate<DrawdownMonitorJob>(
    "drawdown-monitor",
    job => job.ExecuteAsync(CancellationToken.None),
    "*/15 * * * * *",  // every 15 seconds (cron with seconds)
    new RecurringJobOptions { Queue = "critical" });

RecurringJob.AddOrUpdate<VolatilityUpdateJob>(
    "volatility-update",
    job => job.ExecuteAsync(CancellationToken.None),
    "*/5 * * * *",     // every 5 minutes
    new RecurringJobOptions { Queue = "critical" });

// Trading queue
RecurringJob.AddOrUpdate<PortfolioSnapshotJob>(
    "portfolio-snapshot",
    job => job.ExecuteAsync(CancellationToken.None),
    "* * * * *",       // every minute
    new RecurringJobOptions { Queue = "trading" });

RecurringJob.AddOrUpdate<AlertEvaluationJob>(
    "alert-evaluation",
    job => job.ExecuteAsync(CancellationToken.None),
    "*/30 * * * * *",  // every 30 seconds
    new RecurringJobOptions { Queue = "trading" });

RecurringJob.AddOrUpdate<ExposureSnapshotJob>(
    "exposure-snapshot",
    job => job.ExecuteAsync(CancellationToken.None),
    "* * * * *",       // every minute
    new RecurringJobOptions { Queue = "trading" });

RecurringJob.AddOrUpdate<LivePerformanceComparisonJob>(
    "live-performance-comparison",
    job => job.ExecuteAsync(CancellationToken.None),
    "*/5 * * * *",     // every 5 minutes
    new RecurringJobOptions { Queue = "trading" });

// Analysis queue — can be delayed without operational impact
RecurringJob.AddOrUpdate<BacktestPollingJob>(
    "backtest-polling",
    job => job.ExecuteAsync(CancellationToken.None),
    "*/5 * * * *",     // every 5 minutes
    new RecurringJobOptions { Queue = "analysis" });

RecurringJob.AddOrUpdate<CorrelationUpdateJob>(
    "correlation-update",
    job => job.ExecuteAsync(CancellationToken.None),
    "0 * * * *",       // every hour
    new RecurringJobOptions { Queue = "analysis" });

RecurringJob.AddOrUpdate<DecayTrackingJob>(
    "decay-tracking",
    job => job.ExecuteAsync(CancellationToken.None),
    "30 16 * * 1-5",   // weekdays at 4:30 PM ET (after market close)
    new RecurringJobOptions { Queue = "analysis" });
```

**For fire-and-forget AI analysis jobs** (triggered when a new backtest is detected):

```csharp
// In BacktestPollingJob, when a new backtest is found:
BackgroundJob.Enqueue<BacktestOrchestrator>(
    x => x.AnalyzeBacktestAsync(backtestId, CancellationToken.None),
    new EnqueuedState("analysis"));  // Explicitly route to analysis queue
```

### Resilience: What Happens When Critical Jobs Fail

Add a watchdog mechanism. If `DrawdownMonitorJob` fails to execute for more than 60 seconds (4 missed cycles), trigger an automatic safety response:

**New file: `RivrQuant.Application/BackgroundJobs/CriticalJobWatchdog.cs`**

```
Purpose: Monitors that critical jobs are actually running on schedule.
Implementation:
  - DrawdownMonitorJob writes a heartbeat timestamp to an in-memory cache (IMemoryCache)
    at the START of every execution: cache.Set("drawdown-monitor-heartbeat", DateTimeOffset.UtcNow)
  - VolatilityUpdateJob does the same: cache.Set("volatility-update-heartbeat", DateTimeOffset.UtcNow)
  - CriticalJobWatchdog is a separate recurring job on the "trading" queue (NOT the critical queue,
    so it can detect critical queue failures) that runs every 30 seconds:
    1. Read "drawdown-monitor-heartbeat" from cache
    2. If heartbeat is older than 60 seconds (4 missed cycles):
       a. Log CRITICAL error: "DrawdownMonitorJob has not executed in {elapsed}s. Safety halt triggered."
       b. Call IDrawdownManager to force Level 4 (emergency halt)
       c. Send EMERGENCY alert: "Risk monitoring failure detected. All trading halted as safety precaution. Manual restart required after investigation."
    3. Same check for "volatility-update-heartbeat" with a 10-minute tolerance (since it only runs every 5 minutes)
  - If the watchdog itself fails: the next layer of defense is the AlertEvaluationJob which independently checks drawdown thresholds.
    Three independent systems must ALL fail simultaneously before an unmonitored drawdown can occur.
```

Register the watchdog:

```csharp
RecurringJob.AddOrUpdate<CriticalJobWatchdog>(
    "critical-job-watchdog",
    job => job.ExecuteAsync(CancellationToken.None),
    "*/30 * * * * *",  // every 30 seconds
    new RecurringJobOptions { Queue = "trading" });  // Runs on trading queue, monitors critical queue
```

### Frontend Addition

Add to the Settings page (`/settings`):

**New section: "System Health"**

- Show each Hangfire queue: name, worker count, jobs in queue, last execution time
- Show each critical job's heartbeat: DrawdownMonitor (last heartbeat: 3s ago ✅), VolatilityUpdate (last heartbeat: 2m ago ✅)
- If any heartbeat is stale: show red warning banner
- Show watchdog status: last check, any triggers in the last 24 hours

**New API endpoints in DashboardController:**

```
GET /api/dashboard/system-health    — Queue stats, heartbeat timestamps, watchdog status
```

---

## Implementation Order

1. LiquidationAudit + LiquidationFillRecord domain models
2. Modify DrawdownManager Level 4 logic to produce audit records
3. ISmartExitEngine interface + stub implementation
4. LiquidationAudit persistence (EF Core configuration + migration)
5. Execution controller liquidation endpoints
6. Frontend liquidation audit page + components
7. Hangfire multi-server queue configuration in Program.cs
8. Update ALL existing job registrations with explicit queue assignments
9. Add heartbeat writes to DrawdownMonitorJob and VolatilityUpdateJob
10. Implement CriticalJobWatchdog
11. Frontend system health section in settings
12. Dashboard system-health API endpoint

## Post-Implementation Checklist

- [ ] Can the DrawdownMonitorJob EVER be starved of a worker by AI analysis jobs? (Must be impossible with dedicated server)
- [ ] If Claude's API hangs for 5 minutes, does drawdown monitoring continue uninterrupted? (Must be yes)
- [ ] If a Level 4 liquidation partially fails (some fills, some errors), is every outcome recorded in the audit? (Must be yes)
- [ ] If a position fails to close after retry, does a MANUAL INTERVENTION alert fire? (Must be yes)
- [ ] Does the CriticalJobWatchdog run on a DIFFERENT queue than the jobs it monitors? (Must be yes — trading queue monitors critical queue)
- [ ] If both the critical queue AND the trading queue fail, does AlertEvaluationJob provide a third layer of defense? (Verify independence)
- [ ] Is every LiquidationAudit record persisted BEFORE the system enters halted state? (Must survive restart)
- [ ] Can you view the full liquidation fill history from the dashboard without touching the database? (Must be yes)
