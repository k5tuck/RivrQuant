# Architecture

## Overview

RivrQuant follows clean architecture principles with four distinct layers:

```
┌─────────────────────────────────────┐
│         RivrQuant.Api               │  ← Controllers, SignalR Hub, Middleware
├─────────────────────────────────────┤
│       RivrQuant.Application         │  ← Services, Background Jobs, DTOs
├─────────────────────────────────────┤
│     RivrQuant.Infrastructure        │  ← External integrations, EF Core, APIs
├─────────────────────────────────────┤
│         RivrQuant.Domain            │  ← Models, Interfaces, Enums, Exceptions
└─────────────────────────────────────┘
```

Dependencies flow inward — Domain has zero dependencies, Infrastructure depends on Domain, Application depends on both, and Api depends on all.

## Domain Layer

Pure C# classes with no external dependencies. Contains:

- **Models** — Strongly-typed domain entities (BacktestResult, Strategy, Position, Order, etc.)
- **Interfaces** — Contracts for external services (IBrokerClient, IBacktestProvider, IAiAnalyzer, etc.)
- **Enums** — Type-safe enumerations (AssetClass, OrderSide, RegimeType, etc.)
- **Exceptions** — Custom domain exceptions with context (BrokerConnectionException, AiAnalysisException, etc.)

## Infrastructure Layer

Implements all external service integrations:

- **Persistence** — EF Core DbContext with 16 entity configurations, SQLite/PostgreSQL dual support
- **QuantConnect** — REST API client with Basic Auth, backtest polling, JSON result parsing
- **Analysis** — Math.NET statistics engine, Claude AI analyzer, regime detection, walk-forward analysis
- **Brokers/Alpaca** — IBrokerClient implementation using Alpaca.Markets SDK, WebSocket streaming
- **Brokers/Bybit** — IBrokerClient implementation with custom HMAC-SHA256 signing, Bybit v5 API
- **Alerts** — SendGrid email sender, Twilio SMS sender, rule evaluator, alert dispatcher

## Application Layer

Business logic orchestration:

- **Services** — BacktestService, TradingService, DashboardService, AnalysisService, AlertAppService, StrategyService
- **Background Jobs** — Hangfire recurring jobs for backtest polling, portfolio snapshots, alert evaluation
- **DTOs** — Data transfer objects for API responses

## API Layer

ASP.NET Core 8 Web API:

- **Controllers** — REST endpoints for dashboard, backtests, trading, analysis, alerts, strategies, market data
- **SignalR Hub** — TradingHub pushes real-time updates (portfolio, positions, orders, alerts)
- **Middleware** — Global exception handling (maps domain exceptions to HTTP status codes), request logging
- **Filters** — API key authentication (development mode bypass)

## Data Flow

### Backtest Analysis Pipeline

```
QC API → QcBacktestPoller → BacktestService → MathNetStatisticsEngine
                                             → RegimeDetector
                                             → ClaudeAiAnalyzer
                                             → Database + SignalR notification
```

### Live Trading Flow

```
OrderForm → TradingController → TradingService → IBrokerClient (Alpaca/Bybit)
                                                → SignalR (PositionUpdate, OrderUpdate)
                                                → AlertRuleEvaluator
```

### Alert Pipeline

```
AlertEvaluationJob (every 30s) → AlertRuleEvaluator → AlertDispatcher
                                                     → SendGridEmailSender
                                                     → TwilioSmsSender
                                                     → SignalR (AlertTriggered)
```

## Key Design Decisions

1. **Unified IBrokerClient** — Both Alpaca and Bybit implement the same interface, enabling broker-agnostic trading logic
2. **Background job polling** — Hangfire recurring jobs for QC polling, snapshots, and alerts rather than long-running hosted services
3. **SignalR for real-time** — WebSocket push from backend to frontend for all state changes
4. **Domain exceptions** — Custom exception hierarchy mapped to HTTP status codes in middleware
5. **SQLite for dev** — Zero-config development, PostgreSQL for production via EF Core provider switching
