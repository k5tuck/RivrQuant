# RivrQuant — Claude Code Project Prompt

## Pre-Requisite: Create the GitHub Repository

Run this before starting any code generation:

```bash
# Install GitHub CLI if not already installed
# macOS: brew install gh
# Windows: winget install GitHub.cli
# Linux: sudo apt install gh

# Authenticate (one-time)
gh auth login

# Create the private repo and clone it
gh repo create RivrQuant --private --clone --description "Quantitative trading platform — backtesting, AI analysis, live deployment for stocks (Alpaca) and crypto (Bybit)"
cd RivrQuant
git checkout -b main
```

## Anthropic API Key Setup

The AI analysis pipeline requires a Claude API key:

1. Go to https://console.anthropic.com/
2. Create an account or sign in
3. Navigate to **API Keys** in the left sidebar
4. Click **Create Key**, name it `rivrquant-analysis`
5. Copy the key immediately (it won't be shown again)
6. Store it in your environment or user secrets (instructions below in the configuration section)

**Pricing context:** Claude Sonnet 4 is the recommended model for the analysis pipeline. At current pricing, analyzing a typical backtest (trade log + daily returns + regime classification) costs approximately $0.01–$0.05 per analysis. Even running 50 analyses/day would cost ~$1–2.50/day.

---

## Project Overview

**RivrQuant** is a quantitative trading platform that connects existing trading infrastructure (QuantConnect LEAN, Alpaca, Bybit) into a unified system with AI-powered analysis and real-time monitoring.

### Core Architecture

```
┌─────────────────────────────────────────────────────────────────────┐
│                        RivrQuant Platform                          │
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│  ┌──────────────┐    ┌──────────────────┐    ┌──────────────────┐  │
│  │ QuantConnect  │───▶│  Polling Service  │───▶│  AI Analysis     │  │
│  │ (Backtesting) │    │  (Background Job) │    │  Pipeline        │  │
│  └──────────────┘    └──────────────────┘    │  - Claude API    │  │
│                                               │  - Math.NET      │  │
│  ┌──────────────┐    ┌──────────────────┐    │  - Regime Detect  │  │
│  │ Alpaca API   │───▶│  Trading Engine   │───▶│  - Walk-Forward  │  │
│  │ (Stocks)     │    │  (Order Mgmt)     │    └────────┬─────────┘  │
│  └──────────────┘    └──────────────────┘             │            │
│                                                        ▼            │
│  ┌──────────────┐    ┌──────────────────┐    ┌──────────────────┐  │
│  │ Bybit API    │───▶│  Crypto Engine    │───▶│  Alert Service   │  │
│  │ (Crypto)     │    │  (Testnet + Live) │    │  - SendGrid      │  │
│  └──────────────┘    └──────────────────┘    │  - Twilio SMS     │  │
│                                               └────────┬─────────┘  │
│                                                        ▼            │
│                      ┌──────────────────────────────────────────┐   │
│                      │         Next.js Dashboard (Frontend)      │   │
│                      │  - Real-time equity curves                │   │
│                      │  - Backtest comparison views               │   │
│                      │  - AI analysis reports                    │   │
│                      │  - Live position monitoring               │   │
│                      │  - Risk metrics & alerts config           │   │
│                      └──────────────────────────────────────────┘   │
│                                                                     │
└─────────────────────────────────────────────────────────────────────┘
```

### Tech Stack

| Layer | Technology |
|-------|-----------|
| **Backend API** | ASP.NET Core 8 (Web API) |
| **Frontend** | Next.js 14+ (App Router, TypeScript, Tailwind CSS, shadcn/ui) |
| **Backtesting** | QuantConnect LEAN Engine (C# SDK + REST API) |
| **Stocks Broker** | Alpaca Markets API (C# SDK) |
| **Crypto Exchange** | Bybit API v5 (REST + WebSocket) |
| **AI Analysis** | Anthropic Claude API (claude-sonnet-4-20250514) |
| **Statistics** | Math.NET Numerics |
| **Background Jobs** | Hangfire (with SQLite storage for dev) |
| **Database** | PostgreSQL (production) / SQLite (development) |
| **ORM** | Entity Framework Core 8 |
| **Alerts — Email** | SendGrid API |
| **Alerts — SMS** | Twilio API |
| **Real-time Comms** | SignalR (WebSocket from backend to frontend) |
| **Caching** | Redis (production) / In-memory (development) |
| **Containerization** | Docker + Docker Compose |

---

## Solution Structure

```
RivrQuant/
├── README.md
├── LICENSE
├── .gitignore
├── .env.example
├── docker-compose.yml
├── docker-compose.dev.yml
│
├── src/
│   ├── RivrQuant.sln
│   │
│   ├── RivrQuant.Domain/                    # Domain models, enums, interfaces
│   │   ├── RivrQuant.Domain.csproj
│   │   ├── Models/
│   │   │   ├── Backtests/
│   │   │   │   ├── BacktestResult.cs        # Backtest run metadata
│   │   │   │   ├── BacktestTrade.cs         # Individual trade record
│   │   │   │   ├── DailyReturn.cs           # Daily equity/return data
│   │   │   │   ├── BacktestMetrics.cs       # Sharpe, Sortino, MaxDD, etc.
│   │   │   │   └── BacktestComparison.cs    # Side-by-side comparison model
│   │   │   ├── Strategies/
│   │   │   │   ├── Strategy.cs              # Strategy definition
│   │   │   │   ├── StrategyParameter.cs     # Tunable parameter definition
│   │   │   │   └── StrategyVersion.cs       # Version tracking for iterations
│   │   │   ├── Trading/
│   │   │   │   ├── Position.cs              # Current open position
│   │   │   │   ├── Order.cs                 # Order (pending/filled/cancelled)
│   │   │   │   ├── Fill.cs                  # Execution fill details
│   │   │   │   ├── Portfolio.cs             # Aggregate portfolio state
│   │   │   │   └── PerformanceSnapshot.cs   # Point-in-time performance record
│   │   │   ├── Analysis/
│   │   │   │   ├── AiAnalysisReport.cs      # Claude AI analysis output
│   │   │   │   ├── RegimeClassification.cs  # Bull/bear/sideways/volatile
│   │   │   │   ├── ParameterSensitivity.cs  # Parameter sweep results
│   │   │   │   └── WalkForwardResult.cs     # Walk-forward validation result
│   │   │   ├── Alerts/
│   │   │   │   ├── AlertRule.cs             # User-defined alert condition
│   │   │   │   ├── AlertEvent.cs            # Triggered alert record
│   │   │   │   └── AlertChannel.cs          # Email/SMS delivery config
│   │   │   └── Market/
│   │   │       ├── MarketRegime.cs          # Market regime enum + metadata
│   │   │       ├── Asset.cs                 # Tradeable asset (stock or crypto)
│   │   │       └── PriceBar.cs              # OHLCV price data
│   │   ├── Enums/
│   │   │   ├── AssetClass.cs                # Stock, Crypto
│   │   │   ├── OrderSide.cs                 # Buy, Sell
│   │   │   ├── OrderType.cs                 # Market, Limit, StopLoss, etc.
│   │   │   ├── OrderStatus.cs               # Pending, Filled, Cancelled, Rejected
│   │   │   ├── BrokerType.cs                # Alpaca, Bybit
│   │   │   ├── RegimeType.cs                # Trending, MeanReverting, HighVol, LowVol
│   │   │   ├── AlertSeverity.cs             # Info, Warning, Critical
│   │   │   └── EnvironmentMode.cs           # Paper, LiveTestnet, LiveProduction
│   │   ├── Interfaces/
│   │   │   ├── IBrokerClient.cs             # Unified broker interface (Alpaca + Bybit)
│   │   │   ├── IBacktestProvider.cs         # QuantConnect integration interface
│   │   │   ├── IAiAnalyzer.cs               # Claude analysis interface
│   │   │   ├── IStatisticsEngine.cs         # Math.NET statistics interface
│   │   │   ├── IAlertService.cs             # Alert dispatch interface
│   │   │   ├── IMarketDataProvider.cs       # Real-time + historical data interface
│   │   │   └── IPortfolioTracker.cs         # Portfolio state tracking interface
│   │   └── Exceptions/
│   │       ├── RivrQuantException.cs        # Base exception
│   │       ├── BrokerConnectionException.cs # Broker API connection failures
│   │       ├── InsufficientFundsException.cs# Not enough capital for order
│   │       ├── BacktestRetrievalException.cs# QuantConnect API errors
│   │       ├── AiAnalysisException.cs       # Claude API errors
│   │       ├── AlertDeliveryException.cs    # SendGrid/Twilio failures
│   │       └── RiskLimitExceededException.cs# Position/drawdown limit hit
│   │
│   ├── RivrQuant.Infrastructure/            # External service integrations
│   │   ├── RivrQuant.Infrastructure.csproj
│   │   ├── Brokers/
│   │   │   ├── Alpaca/
│   │   │   │   ├── AlpacaBrokerClient.cs    # IBrokerClient implementation for stocks
│   │   │   │   ├── AlpacaStreamClient.cs    # Real-time WebSocket streaming
│   │   │   │   ├── AlpacaAccountMapper.cs   # Map Alpaca models to domain models
│   │   │   │   └── AlpacaConfiguration.cs   # API key, base URL, paper/live toggle
│   │   │   └── Bybit/
│   │   │       ├── BybitBrokerClient.cs      # IBrokerClient implementation for crypto
│   │   │       ├── BybitWebSocketClient.cs   # Real-time WebSocket streaming
│   │   │       ├── BybitAuthenticator.cs     # HMAC-SHA256 request signing
│   │   │       ├── BybitAccountMapper.cs     # Map Bybit models to domain models
│   │   │       └── BybitConfiguration.cs     # API key/secret, testnet/live toggle
│   │   ├── QuantConnect/
│   │   │   ├── QcApiClient.cs               # QuantConnect REST API client
│   │   │   ├── QcBacktestPoller.cs          # Background polling for new backtest results
│   │   │   ├── QcResultParser.cs            # Parse backtest JSON into domain models
│   │   │   └── QcConfiguration.cs           # User ID, API token, project IDs
│   │   ├── Analysis/
│   │   │   ├── ClaudeAiAnalyzer.cs          # IAiAnalyzer implementation using Claude API
│   │   │   ├── ClaudePromptBuilder.cs       # Builds structured analysis prompts
│   │   │   ├── MathNetStatisticsEngine.cs   # IStatisticsEngine using Math.NET Numerics
│   │   │   ├── RegimeDetector.cs            # Market regime classification logic
│   │   │   ├── WalkForwardAnalyzer.cs       # Walk-forward validation engine
│   │   │   └── ParameterSweepRunner.cs      # Parameter sensitivity analysis
│   │   ├── Alerts/
│   │   │   ├── SendGridEmailSender.cs       # Email alerts via SendGrid
│   │   │   ├── TwilioSmsSender.cs           # SMS alerts via Twilio
│   │   │   ├── AlertDispatcher.cs           # Routes alerts to configured channels
│   │   │   └── AlertRuleEvaluator.cs        # Evaluates alert conditions against portfolio state
│   │   ├── Persistence/
│   │   │   ├── RivrQuantDbContext.cs         # EF Core DbContext
│   │   │   ├── Configurations/              # EF Core entity configurations
│   │   │   └── Migrations/
│   │   └── DependencyInjection.cs           # Service registration for Infrastructure
│   │
│   ├── RivrQuant.Application/              # Business logic, CQRS handlers
│   │   ├── RivrQuant.Application.csproj
│   │   ├── Services/
│   │   │   ├── BacktestOrchestrator.cs      # Coordinates backtest retrieval → analysis → storage
│   │   │   ├── TradingOrchestrator.cs       # Coordinates order placement across brokers
│   │   │   ├── PortfolioService.cs          # Aggregate portfolio across Alpaca + Bybit
│   │   │   ├── AnalysisService.cs           # Coordinates AI + statistical analysis
│   │   │   ├── AlertService.cs              # Manages alert rules, evaluation, and dispatch
│   │   │   └── ComparisonService.cs         # Backtest vs live performance comparison
│   │   ├── BackgroundJobs/
│   │   │   ├── BacktestPollingJob.cs        # Hangfire recurring job — polls QuantConnect
│   │   │   ├── PortfolioSnapshotJob.cs      # Hangfire recurring job — snapshots portfolio state
│   │   │   ├── AlertEvaluationJob.cs        # Hangfire recurring job — checks alert conditions
│   │   │   └── LivePerformanceComparisonJob.cs # Compares live vs backtest expectations
│   │   ├── DTOs/                            # Data transfer objects for API responses
│   │   └── DependencyInjection.cs
│   │
│   ├── RivrQuant.Api/                      # ASP.NET Core Web API
│   │   ├── RivrQuant.Api.csproj
│   │   ├── Program.cs
│   │   ├── appsettings.json
│   │   ├── appsettings.Development.json
│   │   ├── Controllers/
│   │   │   ├── BacktestController.cs        # GET backtests, trigger analysis, compare
│   │   │   ├── StrategyController.cs        # CRUD strategies
│   │   │   ├── TradingController.cs         # Place orders, get positions, portfolio
│   │   │   ├── AnalysisController.cs        # Get AI reports, run analysis on-demand
│   │   │   ├── AlertController.cs           # CRUD alert rules, alert history
│   │   │   ├── MarketDataController.cs      # Price data, regime info
│   │   │   └── DashboardController.cs       # Aggregated dashboard data endpoints
│   │   ├── Hubs/
│   │   │   └── TradingHub.cs                # SignalR hub for real-time updates
│   │   ├── Middleware/
│   │   │   ├── ExceptionHandlingMiddleware.cs # Global exception handling
│   │   │   └── RequestLoggingMiddleware.cs
│   │   └── Filters/
│   │       └── ApiKeyAuthFilter.cs          # Simple API key auth for dashboard
│   │
│   └── RivrQuant.Tests/                    # Unit + integration tests
│       ├── RivrQuant.Tests.csproj
│       ├── Unit/
│       │   ├── Analysis/
│       │   │   ├── RegimeDetectorTests.cs
│       │   │   ├── StatisticsEngineTests.cs
│       │   │   └── WalkForwardAnalyzerTests.cs
│       │   ├── Brokers/
│       │   │   ├── AlpacaBrokerClientTests.cs
│       │   │   └── BybitBrokerClientTests.cs
│       │   └── Alerts/
│       │       └── AlertRuleEvaluatorTests.cs
│       └── Integration/
│           ├── QcApiClientTests.cs
│           └── ClaudeAiAnalyzerTests.cs
│
├── frontend/                               # Next.js Dashboard
│   ├── package.json
│   ├── tsconfig.json
│   ├── tailwind.config.ts
│   ├── next.config.ts
│   ├── .env.local.example
│   ├── app/
│   │   ├── layout.tsx                       # Root layout with sidebar navigation
│   │   ├── page.tsx                         # Dashboard home — portfolio overview
│   │   ├── globals.css
│   │   ├── backtests/
│   │   │   ├── page.tsx                     # Backtest list with status indicators
│   │   │   ├── [id]/
│   │   │   │   └── page.tsx                 # Individual backtest detail view
│   │   │   └── compare/
│   │   │       └── page.tsx                 # Side-by-side backtest comparison
│   │   ├── strategies/
│   │   │   ├── page.tsx                     # Strategy list with version history
│   │   │   └── [id]/
│   │   │       └── page.tsx                 # Strategy detail + linked backtests
│   │   ├── trading/
│   │   │   ├── page.tsx                     # Live trading dashboard
│   │   │   ├── positions/
│   │   │   │   └── page.tsx                 # Current open positions across brokers
│   │   │   ├── orders/
│   │   │   │   └── page.tsx                 # Order history + pending orders
│   │   │   └── performance/
│   │   │       └── page.tsx                 # Live vs backtest performance overlay
│   │   ├── analysis/
│   │   │   ├── page.tsx                     # AI analysis reports list
│   │   │   ├── [id]/
│   │   │   │   └── page.tsx                 # Full AI analysis report view
│   │   │   └── regimes/
│   │   │       └── page.tsx                 # Market regime timeline visualization
│   │   ├── alerts/
│   │   │   ├── page.tsx                     # Alert rules management
│   │   │   └── history/
│   │   │       └── page.tsx                 # Alert event history log
│   │   └── settings/
│   │       └── page.tsx                     # API keys, broker connections, preferences
│   ├── components/
│   │   ├── ui/                              # shadcn/ui components
│   │   ├── charts/
│   │   │   ├── EquityCurveChart.tsx         # Recharts equity curve with benchmark overlay
│   │   │   ├── DrawdownChart.tsx            # Underwater/drawdown visualization
│   │   │   ├── ReturnsDistribution.tsx      # Histogram of daily/monthly returns
│   │   │   ├── RegimeTimeline.tsx           # Color-coded market regime bands
│   │   │   ├── ParameterHeatmap.tsx         # Parameter sensitivity heatmap
│   │   │   ├── TradeScatterPlot.tsx         # Win/loss scatter by time/size
│   │   │   ├── PortfolioPieChart.tsx        # Current allocation breakdown
│   │   │   └── LivePnlTicker.tsx            # Real-time P&L with SignalR
│   │   ├── dashboard/
│   │   │   ├── PortfolioSummaryCard.tsx     # Total value, daily change, drawdown
│   │   │   ├── ActiveStrategiesCard.tsx     # Running strategies with status
│   │   │   ├── RecentTradesTable.tsx        # Last N trades across all brokers
│   │   │   ├── AlertsBanner.tsx             # Active/recent alerts strip
│   │   │   ├── MetricsGrid.tsx             # Sharpe, Sortino, Win Rate, etc.
│   │   │   └── BrokerStatusIndicator.tsx    # Alpaca/Bybit connection health
│   │   ├── analysis/
│   │   │   ├── AiReportRenderer.tsx         # Renders Claude's analysis as rich UI
│   │   │   ├── BacktestComparisonTable.tsx  # Side-by-side metrics comparison
│   │   │   └── WalkForwardChart.tsx         # In-sample vs out-of-sample overlay
│   │   ├── trading/
│   │   │   ├── PositionCard.tsx             # Individual position with P&L
│   │   │   ├── OrderForm.tsx                # Manual order entry (for testing)
│   │   │   └── KillSwitchButton.tsx         # Emergency: close all positions
│   │   └── layout/
│   │       ├── Sidebar.tsx                  # Navigation sidebar
│   │       ├── Header.tsx                   # Top bar with portfolio value
│   │       └── ConnectionStatus.tsx         # SignalR + broker connection status
│   ├── lib/
│   │   ├── api.ts                           # Typed API client for ASP.NET Core backend
│   │   ├── signalr.ts                       # SignalR connection manager
│   │   ├── formatters.ts                    # Currency, percentage, date formatters
│   │   └── types.ts                         # TypeScript types mirroring C# DTOs
│   └── hooks/
│       ├── usePortfolio.ts                  # Real-time portfolio state via SignalR
│       ├── useBacktestResults.ts            # Fetch + cache backtest data
│       ├── useAlerts.ts                     # Alert subscription hook
│       └── useTradingStatus.ts              # Broker connection + trading status
│
└── docs/
    ├── SETUP.md                             # Full setup guide
    ├── ARCHITECTURE.md                      # Detailed architecture documentation
    ├── API.md                               # API endpoint documentation
    ├── STRATEGIES.md                        # Guide to writing QuantConnect strategies
    └── DEPLOYMENT.md                        # Docker deployment guide
```

---

## Configuration

All secrets are stored via .NET User Secrets (development) and environment variables (production).

### .env.example

```env
# =============================================
# RivrQuant Configuration
# =============================================

# --- Environment ---
ASPNETCORE_ENVIRONMENT=Development
RIVRQUANT_MODE=Paper                         # Paper | LiveTestnet | LiveProduction

# --- Database ---
DATABASE_PROVIDER=Sqlite                     # Sqlite | PostgreSQL
DATABASE_CONNECTION=Data Source=rivrquant.db
# DATABASE_CONNECTION=Host=localhost;Database=rivrquant;Username=rq;Password=changeme

# --- QuantConnect ---
QC_USER_ID=your_quantconnect_user_id
QC_API_TOKEN=your_quantconnect_api_token
QC_PROJECT_IDS=12345,67890                   # Comma-separated project IDs to poll
QC_POLL_INTERVAL_SECONDS=300                 # Poll every 5 minutes

# --- Alpaca (Stocks) ---
ALPACA_API_KEY=your_alpaca_api_key
ALPACA_API_SECRET=your_alpaca_secret
ALPACA_PAPER=true                            # true = paper trading, false = live
ALPACA_BASE_URL=https://paper-api.alpaca.markets   # Change to live URL for production

# --- Bybit (Crypto) ---
BYBIT_API_KEY=your_bybit_api_key
BYBIT_API_SECRET=your_bybit_api_secret
BYBIT_USE_TESTNET=true                       # true = testnet, false = live mainnet
BYBIT_TESTNET_URL=https://api-testnet.bybit.com
BYBIT_LIVE_URL=https://api.bybit.com

# --- Anthropic (AI Analysis) ---
ANTHROPIC_API_KEY=your_anthropic_api_key
ANTHROPIC_MODEL=claude-sonnet-4-20250514
ANTHROPIC_MAX_TOKENS=4096

# --- SendGrid (Email Alerts) ---
SENDGRID_API_KEY=your_sendgrid_api_key
SENDGRID_FROM_EMAIL=alerts@rivrquant.com
SENDGRID_FROM_NAME=RivrQuant Alerts
ALERT_EMAIL_RECIPIENTS=kevin@example.com     # Comma-separated

# --- Twilio (SMS Alerts) ---
TWILIO_ACCOUNT_SID=your_twilio_sid
TWILIO_AUTH_TOKEN=your_twilio_token
TWILIO_FROM_NUMBER=+1234567890
ALERT_SMS_RECIPIENTS=+1234567890             # Comma-separated

# --- SignalR ---
SIGNALR_ENABLE=true

# --- Frontend ---
NEXT_PUBLIC_API_URL=http://localhost:5000
NEXT_PUBLIC_SIGNALR_URL=http://localhost:5000/hubs/trading
```

---

## Implementation Requirements

### CRITICAL RULES (Apply to ALL code)

1. **No placeholders, TODOs, or ellipses.** Every file must be complete, production-ready code.
2. **Custom exception classes only.** Never throw generic `Exception`. Use the domain exceptions defined in `RivrQuant.Domain/Exceptions/`.
3. **Fail fast with clear, contextual error messages.** Example: `throw new BrokerConnectionException($"Failed to connect to Alpaca paper trading API at {baseUrl}. Status: {response.StatusCode}. Verify ALPACA_API_KEY and ALPACA_API_SECRET are set correctly.", innerException);`
4. **Structured logging everywhere.** Use `ILogger<T>` with structured log templates: `_logger.LogError(ex, "Backtest {BacktestId} analysis failed after {ElapsedMs}ms. Regime: {Regime}", backtestId, elapsed, regime);`
5. **SOLID principles throughout.** Depend on interfaces. Single responsibility per class. Open for extension.
6. **Type safety.** No `dynamic`, no `object` passing, no stringly-typed anything. Use strongly-typed DTOs, enums, and domain models.
7. **Async all the way.** Every I/O operation must be async. No `.Result` or `.Wait()` calls.
8. **Cancellation token propagation.** Every async method accepts and forwards `CancellationToken`.
9. **Null safety.** Enable nullable reference types project-wide. Handle nulls explicitly.
10. **Configuration validation.** All config classes must validate on startup. Missing API keys = immediate startup failure with clear message.

---

## Detailed Component Specifications

### 1. QuantConnect Polling Service (`QcBacktestPoller`)

**Purpose:** Automatically detect when new backtest results are available on QuantConnect and trigger the AI analysis pipeline.

**Behavior:**
- Runs as a Hangfire recurring job every N seconds (configurable via `QC_POLL_INTERVAL_SECONDS`)
- Calls QuantConnect REST API: `GET /api/v2/backtests/read` for each project in `QC_PROJECT_IDS`
- Compares returned backtest IDs against stored IDs in the local database
- When a new backtest is detected:
  1. Fetch full backtest results (trade log, daily equity, statistics) via QC API
  2. Parse into domain models (`BacktestResult`, `BacktestTrade`, `DailyReturn`)
  3. Persist to database
  4. Enqueue an AI analysis job for the new backtest (fire-and-forget via Hangfire)
  5. Push a SignalR notification to the frontend: "New backtest detected: {strategyName} — analysis in progress"

**QuantConnect API endpoints to implement:**
- `GET /api/v2/projects/read` — List projects
- `GET /api/v2/backtests/read` — List backtests for a project
- `GET /api/v2/backtests/read` (with backtestId) — Get full backtest results
- Authentication: Basic Auth with userId:apiToken

**Error handling:**
- QC API rate limit (30 req/min) — implement exponential backoff with jitter
- QC API downtime — log warning, skip cycle, retry next interval
- Malformed response — log error with response body, skip backtest, continue polling

### 2. AI Analysis Pipeline (`ClaudeAiAnalyzer` + `MathNetStatisticsEngine`)

**Purpose:** When a new backtest result arrives (triggered by the polling service), automatically run comprehensive quantitative and AI analysis.

**Pipeline flow:**

```
New Backtest Detected
        │
        ▼
┌─────────────────────────┐
│  MathNetStatisticsEngine │  ◄── Step 1: Calculate raw statistics
│  - Sharpe Ratio          │
│  - Sortino Ratio         │
│  - Max Drawdown          │
│  - Win Rate / Loss Rate  │
│  - Profit Factor         │
│  - Calmar Ratio          │
│  - Value at Risk (95%)   │
│  - Expected Shortfall    │
│  - Monthly Returns       │
│  - Rolling Sharpe (60d)  │
│  - Beta vs SPY benchmark │
│  - Correlation matrix    │
└────────────┬────────────┘
             ▼
┌─────────────────────────┐
│  RegimeDetector          │  ◄── Step 2: Classify market regimes
│  - Split backtest into   │
│    regime windows using  │
│    VIX levels + trend    │
│    detection             │
│  - Label each window:    │
│    Trending, MeanRevert, │
│    HighVol, LowVol,      │
│    Crisis                │
│  - Calculate per-regime  │
│    statistics             │
└────────────┬────────────┘
             ▼
┌─────────────────────────┐
│  ClaudeAiAnalyzer        │  ◄── Step 3: AI interpretation
│  Send to Claude:         │
│  - Full statistics       │
│  - Per-regime breakdown  │
│  - Trade log summary     │
│  - Strategy parameters   │
│                          │
│  Ask Claude to:          │
│  1. Identify strengths   │
│     and weaknesses       │
│  2. Flag overfitting     │
│     risks                │
│  3. Suggest parameter    │
│     adjustments          │
│  4. Compare to known     │
│     strategy archetypes  │
│  5. Rate deployment      │
│     readiness (1-10)     │
│  6. Provide plain-       │
│     English summary      │
└────────────┬────────────┘
             ▼
┌─────────────────────────┐
│  Store AiAnalysisReport  │  ◄── Step 4: Persist + notify
│  Push SignalR update     │
│  Send alert if critical  │
│  findings detected       │
└─────────────────────────┘
```

**Claude prompt structure (`ClaudePromptBuilder`):**

Build a system prompt that establishes Claude as a quantitative analyst, then a user prompt containing:
- Strategy name, description, and parameters
- Full statistics table (from Math.NET)
- Per-regime performance breakdown
- Trade summary (total trades, avg win, avg loss, largest win, largest loss, avg holding period)
- Rolling Sharpe chart data (as a simplified table)
- Request structured JSON output with fields: `overallAssessment`, `strengths[]`, `weaknesses[]`, `overfittingRisk` (low/medium/high with explanation), `parameterSuggestions[]`, `regimeAnalysis`, `deploymentReadiness` (1-10), `plainEnglishSummary`, `criticalWarnings[]`

**Math.NET calculations to implement (all in `MathNetStatisticsEngine`):**
- `CalculateSharpeRatio(dailyReturns[], riskFreeRate)` — annualized
- `CalculateSortinoRatio(dailyReturns[], riskFreeRate)` — downside deviation only
- `CalculateMaxDrawdown(equityCurve[])` — peak-to-trough percentage
- `CalculateValueAtRisk(dailyReturns[], confidence)` — historical VaR
- `CalculateExpectedShortfall(dailyReturns[], confidence)` — CVaR / tail risk
- `CalculateRollingStatistic(dailyReturns[], windowSize, statisticFn)` — generic rolling window
- `CalculateMonteCarloDrawdownProbability(dailyReturns[], threshold, simulations, horizon)` — probability of exceeding drawdown threshold
- `RunParameterSensitivity(backtestResults[], parameterName)` — correlation between parameter values and key metrics
- `CalculateCalmarRatio(annualizedReturn, maxDrawdown)`
- `CalculateProfitFactor(wins[], losses[])`
- `CalculateBeta(strategyReturns[], benchmarkReturns[])`

### 3. Broker Integrations

#### Alpaca (`AlpacaBrokerClient`)

**Implements `IBrokerClient` for stocks.**

**Capabilities:**
- Get account info (buying power, equity, P&L)
- Place orders (market, limit, stop, stop-limit, trailing stop)
- Cancel orders
- Get open positions
- Close positions (individual or all)
- Stream real-time trade updates via Alpaca WebSocket
- Stream real-time quotes for watched symbols

**Alpaca-specific:**
- Use Alpaca.Markets NuGet package (official C# SDK)
- Paper trading URL: `https://paper-api.alpaca.markets`
- Live trading URL: `https://api.alpaca.markets`
- Toggle via `ALPACA_PAPER` env var
- Extended hours trading support (configurable)

#### Bybit (`BybitBrokerClient`)

**Implements `IBrokerClient` for crypto.**

**Capabilities:**
- Get account info (wallet balance, available margin, unrealized P&L)
- Place orders (market, limit) on spot and linear perpetual
- Cancel orders
- Get open positions
- Close positions
- Stream real-time execution updates via Bybit WebSocket v5
- Stream real-time orderbook/ticker for watched pairs

**Bybit-specific:**
- Use Bybit API v5 (REST + WebSocket)
- Build raw HTTP client with HMAC-SHA256 authentication (no official C# SDK — implement `BybitAuthenticator` to sign requests)
- Testnet URL: `https://api-testnet.bybit.com`
- Live URL: `https://api.bybit.com`
- Toggle via `BYBIT_USE_TESTNET` env var
- Implement recv_window parameter (5000ms default) for request freshness
- Handle Bybit-specific error codes (10001 = parameter error, 10003 = invalid API key, 110007 = insufficient balance, etc.)

**Unified `IBrokerClient` interface:**

```csharp
public interface IBrokerClient
{
    BrokerType BrokerType { get; }
    Task<Portfolio> GetPortfolioAsync(CancellationToken ct);
    Task<IReadOnlyList<Position>> GetPositionsAsync(CancellationToken ct);
    Task<Order> PlaceOrderAsync(OrderRequest request, CancellationToken ct);
    Task<Order> CancelOrderAsync(string orderId, CancellationToken ct);
    Task ClosePositionAsync(string symbol, CancellationToken ct);
    Task CloseAllPositionsAsync(CancellationToken ct);
    Task<IReadOnlyList<Order>> GetOrderHistoryAsync(DateTimeOffset from, DateTimeOffset to, CancellationToken ct);
    Task SubscribeToUpdatesAsync(Action<PerformanceSnapshot> onUpdate, CancellationToken ct);
}
```

### 4. Alert System

**Alert Rules (`AlertRule` model):**
Each rule has a condition type, threshold, comparison operator, and delivery channels.

**Supported conditions:**
- `DrawdownExceedsPercent` — current drawdown from peak exceeds threshold (e.g., -10%)
- `DailyLossExceedsAmount` — today's P&L below threshold (e.g., -$500)
- `PositionSizeExceedsPercent` — single position exceeds % of portfolio (e.g., >25%)
- `LiveDeviatesFromBacktest` — live Sharpe deviates from backtest Sharpe by more than N standard deviations
- `BrokerDisconnected` — WebSocket connection lost for more than N seconds
- `NewBacktestComplete` — notification when polling detects new result
- `AiAnalysisComplete` — notification when AI analysis finishes
- `AiCriticalWarning` — AI analysis flagged a critical issue

**Alert evaluation (`AlertEvaluationJob`):**
- Runs as Hangfire recurring job every 30 seconds
- Evaluates all active rules against current portfolio state
- Deduplicates: don't re-fire same alert within cooldown period (configurable, default 1 hour)
- Dispatches via `AlertDispatcher` to configured channels (email, SMS, or both)

**SendGrid integration (`SendGridEmailSender`):**
- Uses SendGrid v3 API via official NuGet package
- HTML email template with: alert title, severity badge, current value vs threshold, timestamp, link to dashboard
- Rate limit: respect SendGrid's 100 emails/second limit

**Twilio integration (`TwilioSmsSender`):**
- Uses Twilio REST API via official NuGet package
- Concise SMS: "[RivrQuant CRITICAL] Drawdown -12.3% exceeds -10% limit. Portfolio: $8,450. Open dashboard: {url}"
- Rate limit: max 1 SMS per alert rule per cooldown period

### 5. Next.js Dashboard — Full Functionality Specification

#### Home Page (`/`) — Portfolio Overview

**Layout:** Full-width dashboard with responsive grid.

**Components:**
- **PortfolioSummaryCard** — Total portfolio value (Alpaca + Bybit combined), daily $ change, daily % change, all-time return. Color-coded green/red.
- **MetricsGrid** — 6-card grid showing: Sharpe (live rolling 30d), Win Rate (last 30 trades), Max Drawdown (current), Open Positions count, Today's P&L, Account Cash Available.
- **EquityCurveChart** — Combined equity curve from all brokers. Toggle overlays: SPY benchmark, BTC benchmark, backtest prediction line. Time range selector: 1D, 1W, 1M, 3M, 6M, 1Y, ALL.
- **RecentTradesTable** — Last 20 trades across all brokers. Columns: Time, Symbol, Side, Quantity, Price, P&L, Broker. Clicking a row opens trade detail.
- **ActiveStrategiesCard** — List of currently deployed strategies with status (Running, Paused, Error), last signal time, and a quick-toggle to pause/resume.
- **AlertsBanner** — Horizontal strip showing most recent 3 alerts. Click to expand to full alert history.
- **BrokerStatusIndicator** — Green/yellow/red dot for Alpaca and Bybit connections. Shows latency.

#### Backtests Section (`/backtests`)

**List Page:**
- Sortable/filterable table of all backtests. Columns: Strategy Name, Date Run, Sharpe, Max DD, Total Return, Win Rate, AI Score (1-10), Status (Analyzed/Pending).
- Quick filters: by strategy, by date range, by AI score range.
- Bulk select for comparison.

**Detail Page (`/backtests/[id]`):**
- Hero section with key metrics (Sharpe, Sortino, Max DD, Total Return, Profit Factor)
- EquityCurveChart with benchmark overlay
- DrawdownChart (underwater plot)
- ReturnsDistribution histogram
- Trade log table (sortable, filterable)
- AI Analysis section (rendered from `AiAnalysisReport`):
  - Overall assessment card
  - Strengths/weaknesses as green/red tagged lists
  - Overfitting risk gauge
  - Deployment readiness score (1-10 with visual meter)
  - Plain English summary
  - Critical warnings highlighted in red cards
- RegimeTimeline showing which regimes the backtest period covered and performance in each

**Comparison Page (`/backtests/compare`):**
- Select 2-4 backtests to compare
- Side-by-side metrics table
- Overlaid equity curves (different colors per backtest)
- Overlaid drawdown charts
- Regime-by-regime comparison table
- AI comparison summary (send all backtests to Claude for comparative analysis)

#### Trading Section (`/trading`)

**Live Dashboard:**
- Real-time P&L ticker (updates via SignalR)
- Current positions grid (PositionCards) — one per position showing symbol, side, qty, entry price, current price, unrealized P&L, % of portfolio
- Separate sections for Stocks (Alpaca) and Crypto (Bybit)
- KillSwitchButton — prominent red button, requires confirmation dialog, calls `CloseAllPositionsAsync` on all brokers
- Manual OrderForm (for testing) — symbol input, side toggle, quantity, order type dropdown, optional limit price

**Performance Page (`/trading/performance`):**
- Live equity curve overlaid with backtest prediction curve
- Deviation tracker: how far is live from expected?
- Rolling Sharpe comparison (live vs backtest, 30-day window)
- WalkForwardChart showing in-sample vs out-of-sample results

#### Analysis Section (`/analysis`)

**Reports List:**
- All AI analysis reports with date, strategy, overall score, critical warnings count
- Filter by strategy, date, score range

**Report Detail (`/analysis/[id]`):**
- Full `AiReportRenderer` component that renders the structured AI output:
  - Executive summary card
  - Strengths and weaknesses in expandable panels
  - Parameter suggestions as actionable cards
  - Risk assessment with visual indicators
  - Regime analysis with color-coded performance table

**Regime Timeline (`/analysis/regimes`):**
- Full-width timeline visualization showing market regimes over time
- Color-coded bands: green (trending up), blue (trending down), yellow (mean reverting), orange (high volatility), gray (low volatility), red (crisis)
- Click a regime band to see which backtests covered that period and their performance

#### Alerts Section (`/alerts`)

**Rules Management:**
- Card-based UI for each alert rule
- Create new rule: condition type dropdown, threshold input, comparison operator, channel selection (email, SMS, both)
- Toggle rules on/off
- Edit/delete rules

**Alert History:**
- Chronological log of all triggered alerts
- Columns: Timestamp, Rule Name, Severity, Message, Delivery Status (sent/failed), Acknowledged (checkbox)
- Filter by severity, date range, acknowledged status

#### Settings Page (`/settings`)

- **Broker Connections:** Show connected status for Alpaca and Bybit. Show which mode (paper/testnet/live). Input fields for API keys (masked). Test connection button.
- **QuantConnect:** User ID, API token (masked), project IDs. Test connection button. Manual "Poll Now" button.
- **AI Analysis:** Anthropic API key (masked). Model selection dropdown. Test button that runs a mini analysis.
- **Alert Channels:** SendGrid API key, from email, recipients list. Twilio SID, token, from number, recipients list. Send test email/SMS buttons.
- **Preferences:** Default time zone, currency display, chart theme (light/dark — use next-themes).

### 6. SignalR Real-Time Updates

**`TradingHub` pushes these event types to the frontend:**

- `PortfolioUpdate` — portfolio value changed (from broker WebSocket)
- `PositionUpdate` — position opened/closed/changed
- `OrderUpdate` — order status changed
- `TradeExecuted` — fill received
- `BacktestDetected` — new backtest found by polling service
- `AnalysisComplete` — AI analysis finished for a backtest
- `AlertTriggered` — alert rule fired
- `BrokerStatusChange` — connection status changed

**Frontend SignalR client (`lib/signalr.ts`):**
- Auto-reconnect with exponential backoff
- Connection state management (connected, reconnecting, disconnected)
- Typed event handlers matching backend event types
- React hooks that subscribe to specific event types

---

## Implementation Order

Build in this order to ensure each layer has its dependencies ready:

1. **Domain models + interfaces + exceptions** — the foundation everything depends on
2. **Database + EF Core setup** — persistence layer
3. **QuantConnect integration + polling service** — data source
4. **Math.NET statistics engine** — quantitative analysis
5. **Claude AI analyzer** — interpretation layer
6. **Backtest orchestrator** — wires polling → stats → AI → storage
7. **Alpaca broker client** — stock trading
8. **Bybit broker client** — crypto trading
9. **Trading orchestrator + portfolio service** — unified trading
10. **Alert system** (rules, evaluation, SendGrid, Twilio)
11. **ASP.NET Core API controllers + SignalR hub**
12. **Next.js frontend** — dashboard pages and components
13. **Docker Compose** — containerized deployment
14. **Tests** — unit and integration tests throughout

---

## Post-Implementation Self-Critique Checklist

After completing each component, evaluate against:

- [ ] Are all exceptions custom domain exceptions with context?
- [ ] Is every async method accepting and forwarding CancellationToken?
- [ ] Are all external API calls wrapped in try-catch with specific exception handling?
- [ ] Is structured logging present at Info (operations), Warning (recoverable), and Error (failures) levels?
- [ ] Are all configuration values validated at startup?
- [ ] Are there no hardcoded secrets, URLs, or magic numbers?
- [ ] Does every public method have XML doc comments?
- [ ] Are all DTOs immutable (record types or init-only)?
- [ ] Is the class doing only one thing (SRP)?
- [ ] Could this be unit tested without mocking 10 dependencies?

---

## Final Notes

- **Security:** Never log API keys or secrets. Use `[LoggerMessage]` attributes for high-performance logging. Sanitize all user inputs.
- **Rate limiting:** Respect all external API rate limits. Implement circuit breakers for external services using Polly.
- **Idempotency:** Order placement must be idempotent. Use client-generated order IDs to prevent duplicate orders on retry.
- **Graceful shutdown:** Handle SIGTERM properly. Close WebSocket connections, flush pending alerts, complete in-flight analyses before shutdown.
- **Health checks:** Implement ASP.NET Core health checks for database, Alpaca, Bybit, QuantConnect, and SignalR. Expose at `/health`.
