# CLAUDE.md — RivrQuant: Backend Execution Engine for Binelek Risk OS

## Step 0 — Repository Context Retrieval

Before writing ANY code, you need access to existing codebases for context.

**Ask the user for a temporary GitHub personal access token (classic) with `repo` scope.**

Say exactly: "I need a temporary GitHub personal access token to read your repositories
for context. Generate one at https://github.com/settings/tokens with `repo` scope and
paste it here. You can revoke it immediately after this session."

Once you have the token:

```bash
export GITHUB_TOKEN="<paste_token_here>"

# Clone this repo for self-inspection
git clone https://${GITHUB_TOKEN}@github.com/k5tuck/RivrQuant.git /tmp/rq-ref 2>/dev/null || true

# Clone binelek-backend WITH SUBMODULES (it has 11 git submodules — won't work without --recursive)
git clone --recursive https://${GITHUB_TOKEN}@github.com/k5tuck/binelek-backend.git /tmp/bl-ref 2>/dev/null || true
cd /tmp/bl-ref && git submodule update --init --recursive 2>/dev/null; cd -

# ── RIVRQUANT: Read everything ──
echo "=== RQ: Solution ==="
cat $(find /tmp/rq-ref -name "*.sln" | head -1) 2>/dev/null

echo "=== RQ: Domain ==="
for f in $(find /tmp/rq-ref/src/RivrQuant.Domain -name "*.cs" 2>/dev/null | grep -v "/bin/" | grep -v "/obj/"); do
  echo "--- $f ---"; cat "$f"; done

echo "=== RQ: Infrastructure ==="
for f in $(find /tmp/rq-ref/src/RivrQuant.Infrastructure -name "*.cs" 2>/dev/null | grep -v "/bin/" | grep -v "/obj/"); do
  echo "--- $f ---"; cat "$f"; done

echo "=== RQ: Application ==="
for f in $(find /tmp/rq-ref/src/RivrQuant.Application -name "*.cs" 2>/dev/null | grep -v "/bin/" | grep -v "/obj/"); do
  echo "--- $f ---"; cat "$f"; done

echo "=== RQ: API ==="
for f in $(find /tmp/rq-ref/src/RivrQuant.Api -name "*.cs" 2>/dev/null | grep -v "/bin/" | grep -v "/obj/"); do
  echo "--- $f ---"; cat "$f"; done

echo "=== RQ: Existing Prompts ==="
for f in $(find /tmp/rq-ref/prompts -type f 2>/dev/null); do
  echo "--- $f ---"; cat "$f"; done

echo "=== RQ: Frontend ==="
find /tmp/rq-ref/frontend -name "*.tsx" -o -name "*.ts" 2>/dev/null | grep -v "/node_modules/" | sort

echo "=== RQ: Docker ==="
cat /tmp/rq-ref/docker-compose.yml 2>/dev/null
cat /tmp/rq-ref/docker-compose.dev.yml 2>/dev/null

# ── BINELEK: Extract patterns from submodules ──
echo "=== BL: Claude.md ==="
cat /tmp/bl-ref/Claude.md 2>/dev/null | head -300

echo "=== BL: Shared libraries (binelek-shared) ==="
for f in $(find /tmp/bl-ref/shared -name "*.cs" 2>/dev/null | grep -v "/bin/" | grep -v "/obj/"); do
  echo "--- $f ---"; cat "$f"; done

echo "=== BL: Core — auth service ==="
for f in $(find /tmp/bl-ref/core -path "*binah-auth*" -name "*.cs" 2>/dev/null | grep -v "/bin/" | grep -v "/obj/"); do
  echo "--- $f ---"; cat "$f"; done

echo "=== BL: Core — API gateway ==="
for f in $(find /tmp/bl-ref/core -path "*binah-api*" -name "*.cs" 2>/dev/null | grep -v "/bin/" | grep -v "/obj/"); do
  echo "--- $f ---"; cat "$f"; done

echo "=== BL: Core — ontology (Neo4j + Kafka) ==="
for f in $(find /tmp/bl-ref/core -path "*binah-ontology*" -name "*.cs" 2>/dev/null | grep -v "/bin/" | grep -v "/obj/"); do
  echo "--- $f ---"; cat "$f"; done

echo "=== BL: Data — pipeline (Hangfire + ETL) ==="
for f in $(find /tmp/bl-ref/data -name "*.cs" 2>/dev/null | grep -v "/bin/" | grep -v "/obj/" | head -20); do
  echo "--- $f ---"; cat "$f"; done

echo "=== BL: Codegen (Roslyn generators) ==="
for f in $(find /tmp/bl-ref/codegen -name "*Generator*" -name "*.cs" 2>/dev/null | grep -v "/bin/" | grep -v "/obj/"); do
  echo "--- $f ---"; cat "$f"; done

echo "=== BL: Database init scripts ==="
for f in $(find /tmp/bl-ref/database -name "*.sql" -o -name "*.cypher" 2>/dev/null | head -10); do
  echo "--- $f ---"; head -80 "$f"; done

echo "=== BL: Docker Compose ==="
head -120 /tmp/bl-ref/docker-compose.yml 2>/dev/null

unset GITHUB_TOKEN
echo "=== DISCOVERY COMPLETE ==="
```

**After discovery, determine:**
1. What already exists in RivrQuant — DO NOT rebuild what's built
2. What Binelek patterns to absorb (Neo4j, Kafka, exceptions, auth, EF Core base)
3. What to skip from Binelek (Python services, multi-tenant, billing, Qdrant, Elasticsearch, GIS, image analysis)

---

## Identity

RivrQuant is the **backend execution engine** that powers the Binelek Risk OS (product name: FRAP).

RivrQuant handles ALL server-side concerns:
- Market data ingestion from broker WebSocket feeds
- Trade execution via Alpaca (stocks) and Bybit (crypto)
- Backtesting via QuantConnect LEAN
- AI analysis via Claude API + Math.NET Numerics
- Portfolio tracking and snapshots
- Alert dispatch (SendGrid email, Twilio SMS)
- Kafka event streaming to the FRAP Risk Engine microservice

RivrQuant does NOT own risk rule evaluation. That belongs to the **FRAP Risk Engine** — a separate microservice that subscribes to RivrQuant's Kafka events.

RivrQuant does NOT own the trader-facing UI. That belongs to **FRAP Web** (Next.js BFF at binelek.io).

## Architecture — Three Services

```
                       binelek.io
                          │
                    FRAP Web (Next.js BFF)
                    ├── Server-side auth token management
                    ├── SSR for dashboard pages
                    ├── API route proxying
                    └── Redis response caching
                          │
              ┌───────────┴───────────┐
              ▼                       ▼
      RivrQuant API             FRAP Risk Engine
      (this repo)               (separate microservice)
      ├── Broker connections    ├── Rule evaluation (<50ms)
      ├── Trade execution       ├── Drawdown monitoring
      ├── Market data stream    ├── Kelly position sizing
      ├── Backtesting (QC)      ├── Behavioral detection
      ├── AI analysis           ├── Risk event logging
      ├── Portfolio tracking    ├── Risk override commands
      ├── Alert dispatch        └── Trading Copilot (NL)
      ├── SignalR hubs                    │
      └── Kafka producers ────────► Kafka ◄──── Kafka consumers
                                          │
                                    Risk overrides
                                    back to RivrQuant
```

## Tech Stack

| Layer | Technology |
|-------|-----------|
| API Server | ASP.NET Core 8 (Web API + SignalR) |
| Database | PostgreSQL 16 (EF Core 8) |
| Graph DB | Neo4j 5 — **absorb from Binelek** (asset correlations, strategy lineage) |
| Internal Events | Apache Kafka + Protobuf — **absorb from Binelek** |
| Cache | Redis 7 |
| Job Queue | Hangfire (3 isolated queues: critical, trading, analysis) |
| Backtesting | QuantConnect LEAN (C# SDK + REST API) |
| Stocks Broker | Alpaca Markets API (paper + live) |
| Crypto Broker | Bybit API v5 (testnet default, live toggle) |
| AI Analysis | Anthropic Claude API (Sonnet) |
| Statistics | Math.NET Numerics |
| Email Alerts | SendGrid |
| SMS Alerts | Twilio |
| Auth | JWT Bearer — **absorb from Binelek binah-auth** |
| Validation | FluentValidation — **absorb from Binelek** |
| Containerization | Docker + Docker Compose |

## Before You Begin

**Read these skill files in order:**

1. `/mnt/skills/user/kafka-dotnet-protobuf-integration/SKILL.md`
2. `/mnt/skills/user/csharp-neo4j-driver-advanced-patterns/SKILL.md`
3. `/mnt/skills/user/dotnet-fluent-validation-patterns/SKILL.md`
4. `/mnt/skills/user/dotnet-project-structure-refactoring/SKILL.md`
5. `/mnt/skills/user/duplicate-functionality-detection/SKILL.md`

## Critical Rules

1. **No placeholders, TODOs, or ellipses.** Every file is complete, production-ready code.
2. **Custom exceptions only.** Base: `RivrQuantException`. Never throw raw `Exception`.
3. **Fail fast with context.** Include entity IDs, operation names, and state in error messages.
4. **Structured logging everywhere.** `_logger.LogError(ex, "Backtest {BacktestId} analysis failed after {Ms}ms", id, elapsed);`
5. **SOLID principles.** Depend on interfaces. Single responsibility. Open for extension.
6. **Bybit testnet is the default.** Live mainnet requires explicit `BYBIT_USE_TESTNET=false`.
7. **Hangfire critical queue is sacred.** DrawdownMonitor + EmergencyHalt can NEVER be blocked by analysis jobs.
8. **RivrQuant never says "buy this."** It executes strategies. Risk rules live in the Risk Engine.
9. **All REST endpoints documented with XML comments** for OpenAPI/Swagger generation.
10. **Neo4j queries use parameterized Cypher only.** No string interpolation. Ever.

## Binelek Patterns to Absorb

After Step 0 discovery, extract and adapt from binelek-backend submodules:

| Source | What to Extract | How to Adapt |
|---|---|---|
| `shared/` (binelek-shared) | BaseEntity, Result<T>, Money value object | Absorb into RivrQuant.Domain. Remove TenantId. |
| `core/binah-auth` | JWT generation, refresh rotation, claims | Add to RivrQuant.Api/Auth/. Simplify: single-user, not multi-tenant. |
| `core/binah-api` | GlobalExceptionMiddleware (RFC 7807), health checks | Enhance RivrQuant.Api existing middleware. |
| `core/binah-ontology` | Neo4j driver patterns, parameterized Cypher, Kafka producer + Protobuf | NEW: RivrQuant.Infrastructure/Graph/ and enhance Kafka support. |
| `data/binah-pipeline` | Hangfire job patterns, ETL patterns | Reference for organizing background jobs (already has Hangfire). |
| `codegen/` | Roslyn code generation | Phase 3+ — evaluate for trading ontology. |
| `database/` | SQL + Cypher init scripts | Reference for migration design. |

**Skip entirely:** Python AI services (binah-search, binah-aip, binah-ml), billing, webhooks, Qdrant, Elasticsearch, VS Code extension, MCP server, multi-tenant complexity, GIS architecture, image analysis docs.

## Kafka Topics (RivrQuant Produces)

The FRAP Risk Engine subscribes to these:

| Topic | Schema | Trigger |
|-------|--------|---------|
| `rivrquant.trade-executions` | TradeExecutionEvent.proto | Every filled order |
| `rivrquant.position-updates` | PositionUpdateEvent.proto | Position state change |
| `rivrquant.market-ticks` | MarketTickEvent.proto | Broker WebSocket feed |
| `rivrquant.backtest-results` | BacktestResultEvent.proto | Backtest completion |
| `rivrquant.ai-analysis` | AiAnalysisEvent.proto | AI pipeline completion |
| `rivrquant.portfolio-snapshots` | PortfolioSnapshot.proto | Periodic (every 30s) |

## Kafka Topics (RivrQuant Consumes)

| Topic | Producer | Required Behavior |
|-------|----------|-------------------|
| `frap.risk-overrides` | FRAP Risk Engine | **MUST execute immediately.** HALT = close all positions. DELEVERAGE = reduce by %. PAUSE_STRATEGY = stop specific strategy. |

## API Contract (What FRAP Web BFF Calls)

```
Auth:
  POST   /api/v1/auth/login              → JWT token
  POST   /api/v1/auth/refresh            → Refresh token

Accounts:
  GET    /api/v1/accounts                → List broker accounts
  GET    /api/v1/accounts/{id}           → Account detail + balance
  POST   /api/v1/accounts/connect        → Connect broker (Alpaca/Bybit)
  DELETE /api/v1/accounts/{id}           → Disconnect broker

Positions:
  GET    /api/v1/accounts/{id}/positions → Current positions
  GET    /api/v1/positions/aggregate     → Cross-account summary

Strategies:
  GET    /api/v1/strategies              → All strategies
  POST   /api/v1/strategies              → Create strategy
  GET    /api/v1/strategies/{id}         → Detail + linked backtests

Backtests:
  GET    /api/v1/backtests               → All results
  GET    /api/v1/backtests/{id}          → Detail + metrics
  GET    /api/v1/backtests/compare       → Side-by-side (query: ids[])
  POST   /api/v1/backtests/trigger       → Trigger via QuantConnect

AI Analysis:
  GET    /api/v1/analysis                → All reports
  GET    /api/v1/analysis/{id}           → Individual report
  POST   /api/v1/analysis/trigger        → Manually trigger for backtest

Performance:
  GET    /api/v1/performance/snapshot    → Current metrics
  GET    /api/v1/performance/history     → Historical P&L
  GET    /api/v1/performance/equity-curve → Equity curve data
  GET    /api/v1/performance/drawdown    → Drawdown time series

Neo4j Graph:
  GET    /api/v1/graph/correlations      → Asset correlation network
  GET    /api/v1/graph/sector-exposure   → Sector concentration
  GET    /api/v1/graph/strategy-lineage/{id} → Strategy graph

Emergency:
  POST   /api/v1/emergency/halt          → Kill switch: close ALL positions
  POST   /api/v1/emergency/resume        → Resume after halt
  GET    /api/v1/emergency/status        → Current halt state

Alerts:
  GET    /api/v1/alerts/config           → Alert configuration
  PUT    /api/v1/alerts/config           → Update channels

Health:
  GET    /health                         → Service health
```

## SignalR Hubs

```
/hubs/risk
  → Broadcasts: RiskSnapshotUpdated, RuleTriggered, EmergencyHaltActivated
  → Client: JoinAccountGroup(accountId), LeaveAccountGroup(accountId)

/hubs/portfolio
  → Broadcasts: PositionUpdated, PnLUpdated, TradeExecuted
  → Client: SubscribeToAccount(accountId)

/hubs/market
  → Broadcasts: TickReceived (filtered by subscribed symbols)
  → Client: SubscribeSymbols(symbols[]), UnsubscribeSymbols(symbols[])
```

## Hangfire Queue Isolation

| Queue | Workers | Jobs |
|-------|---------|------|
| `critical` | 2 | DrawdownMonitorJob, RiskOverrideConsumer |
| `trading` | 3 | PortfolioSnapshotJob, AlertDispatchJob, ExposureUpdateJob |
| `analysis` | 2 | AiAnalysisPipelineJob, BacktestPollingJob, CorrelationUpdateJob |

CriticalJobWatchdog: if DrawdownMonitorJob misses 4 consecutive cycles (60s), force emergency halt.

## Implementation Order

**Phase 1 — Absorb Binelek Patterns (extend existing codebase):**
1. Read all existing RivrQuant code (Step 0) — understand what's built
2. Absorb exception patterns from binelek-shared → extend RivrQuantException hierarchy
3. Absorb Neo4j graph repository from binelek-ontology → RivrQuant.Infrastructure/Graph/
4. Absorb Kafka producer/consumer with Protobuf from binelek-ontology → enhance existing
5. Absorb JWT auth from binah-auth → RivrQuant.Api/Auth/ (simplify: single-user)
6. Absorb FluentValidation patterns → apply to all request DTOs
7. Enhance GlobalExceptionMiddleware with RFC 7807 ProblemDetails

**Phase 2 — Kafka Event Publishing:**
8. Add Kafka producer for `rivrquant.trade-executions` (publish on every fill)
9. Add Kafka producer for `rivrquant.position-updates` (publish on position change)
10. Add Kafka producer for `rivrquant.market-ticks` (publish normalized WebSocket ticks)
11. Add Kafka producer for `rivrquant.backtest-results` (publish on completion)
12. Add Kafka producer for `rivrquant.ai-analysis` (publish on analysis completion)
13. Add Kafka producer for `rivrquant.portfolio-snapshots` (periodic)
14. Add Kafka consumer for `frap.risk-overrides` on critical Hangfire queue

**Phase 3 — Extended API Surface for FRAP:**
15. Add missing controllers: StrategiesController, PerformanceController, GraphController, EmergencyController
16. Add SignalR hubs: RiskHub, PortfolioHub, MarketHub
17. Generate OpenAPI spec (Swashbuckle) — FRAP BFF generates client from this
18. Add Neo4j trading ontology: asset correlations, sector exposure, strategy lineage

**Phase 4 — Enhanced Infrastructure:**
19. docker-compose.yml: add Neo4j, Kafka + Zookeeper, Redis
20. PostgreSQL migrations for any new tables
21. Neo4j Cypher init scripts for trading graph schema

## Self-Critique Checklist

- [ ] Can the DrawdownMonitor EVER be blocked by an analysis job? (Must be impossible)
- [ ] Is the `frap.risk-overrides` consumer on the critical Hangfire queue?
- [ ] Does every filled order publish to `rivrquant.trade-executions`?
- [ ] Is every REST endpoint documented with XML comments for OpenAPI?
- [ ] Are all Neo4j queries parameterized? (No string interpolation)
- [ ] Are all broker tokens encrypted at rest?
- [ ] Can Bybit accidentally connect to mainnet without explicit config change?
- [ ] Does the emergency halt close positions on BOTH Alpaca AND Bybit?
- [ ] Are Binelek patterns adapted (namespaces renamed, tenant logic removed)?
- [ ] Is there a `/health` endpoint checking all dependencies?
