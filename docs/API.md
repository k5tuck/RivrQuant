# API Reference

Base URL: `http://localhost:5000`

All endpoints return JSON. Error responses use ProblemDetails format.

## Authentication

In production, include `X-Api-Key` header with your configured API key. Authentication is bypassed in Development mode.

## Dashboard

### GET /api/dashboard
Returns aggregated dashboard data including portfolio, positions, metrics, recent trades, and alerts.

**Response:** `DashboardDto`

### GET /api/dashboard/portfolio
Returns current portfolio state.

**Response:** `PortfolioDto`

## Backtests

### GET /api/backtest
Lists all backtest results.

**Response:** `BacktestSummaryDto[]`

### GET /api/backtest/{id}
Returns full backtest details including trades, daily returns, and AI analysis.

**Response:** `BacktestDetailDto`

### POST /api/backtest/{id}/analyze
Triggers AI analysis for a specific backtest.

**Response:** `AnalysisReportDto`

### POST /api/backtest/compare
Compares multiple backtests side-by-side.

**Request Body:** `string[]` (backtest IDs)

## Trading

### GET /api/trading/positions
Returns all open positions across brokers.

**Response:** `PositionDto[]`

### GET /api/trading/orders
Returns order history.

**Response:** `OrderDto[]`

### POST /api/trading/orders
Places a new order.

**Request Body:**
```json
{
  "symbol": "AAPL",
  "side": "Buy",
  "type": "Market",
  "quantity": 10,
  "limitPrice": null,
  "stopPrice": null,
  "assetClass": "Stock"
}
```

**Response:** `OrderDto`

### DELETE /api/trading/orders/{id}
Cancels a pending order.

### POST /api/trading/close-all
Emergency close all positions across all brokers.

## Analysis

### GET /api/analysis
Lists all AI analysis reports.

**Response:** `AnalysisReportDto[]`

### GET /api/analysis/{id}
Returns a specific AI analysis report.

**Response:** `AnalysisReportDto`

### POST /api/analysis/{backtestId}/run
Runs AI analysis on a backtest.

**Response:** `AnalysisReportDto`

## Alerts

### GET /api/alert/rules
Lists all alert rules.

**Response:** `AlertRuleDto[]`

### POST /api/alert/rules
Creates a new alert rule.

**Request Body:**
```json
{
  "name": "Drawdown Limit",
  "conditionType": "DrawdownExceedsPercent",
  "threshold": 0.10,
  "severity": "Critical",
  "sendEmail": true,
  "sendSms": true
}
```

### DELETE /api/alert/rules/{id}
Deletes an alert rule.

### PUT /api/alert/rules/{id}/toggle
Toggles an alert rule on/off.

### GET /api/alert/history
Returns alert event history.

**Response:** `AlertEventDto[]`

### PUT /api/alert/history/{id}/acknowledge
Acknowledges a triggered alert.

## Strategies

### GET /api/strategy
Lists all strategies.

**Response:** `StrategyDto[]`

### GET /api/strategy/{id}
Returns strategy details.

**Response:** `StrategyDto`

## Health

### GET /health
Returns service health status.

## SignalR Hub

Connect to `/hubs/trading` for real-time updates.

**Events:**
- `PortfolioUpdate` — Portfolio value changed
- `PositionUpdate` — Position opened/closed/changed
- `OrderUpdate` — Order status changed
- `TradeExecuted` — Fill received
- `BacktestDetected` — New backtest found
- `AnalysisComplete` — AI analysis finished
- `AlertTriggered` — Alert rule fired
- `BrokerStatusChange` — Connection status changed
