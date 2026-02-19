# RivrQuant

Quantitative trading platform — backtesting, AI analysis, live deployment for stocks (Alpaca) and crypto (Bybit).

## Architecture

```
QuantConnect (Backtesting) → Polling Service → AI Analysis Pipeline (Claude + Math.NET)
Alpaca API (Stocks)        → Trading Engine  → Alert Service (SendGrid + Twilio)
Bybit API (Crypto)         → Crypto Engine   → Next.js Dashboard (Real-time via SignalR)
```

### Tech Stack

| Layer | Technology |
|-------|-----------|
| Backend API | ASP.NET Core 8 (Web API) |
| Frontend | Next.js 14 (App Router, TypeScript, Tailwind CSS) |
| Backtesting | QuantConnect LEAN Engine (REST API) |
| Stocks Broker | Alpaca Markets API (C# SDK) |
| Crypto Exchange | Bybit API v5 (REST + WebSocket) |
| AI Analysis | Anthropic Claude API |
| Statistics | Math.NET Numerics |
| Background Jobs | Hangfire |
| Database | PostgreSQL (production) / SQLite (development) |
| ORM | Entity Framework Core 8 |
| Real-time | SignalR (WebSocket) |
| Containerization | Docker + Docker Compose |

## Quick Start

### Prerequisites

- .NET 8 SDK
- Node.js 20+
- Docker (optional, for containerized deployment)

### Development Setup

1. Clone the repository:
```bash
git clone https://github.com/k5tuck/RivrQuant.git
cd RivrQuant
```

2. Configure environment variables:
```bash
cp .env.example .env
# Edit .env with your API keys
```

3. Start the backend:
```bash
cd src
dotnet restore
dotnet run --project RivrQuant.Api
```

4. Start the frontend:
```bash
cd frontend
npm install
npm run dev
```

5. Open http://localhost:3000

### Docker Deployment

```bash
# Development (SQLite)
docker compose -f docker-compose.dev.yml up --build

# Production (PostgreSQL)
docker compose up --build
```

## Project Structure

```
RivrQuant/
├── src/
│   ├── RivrQuant.Domain/          # Domain models, interfaces, exceptions
│   ├── RivrQuant.Infrastructure/  # External service integrations
│   ├── RivrQuant.Application/     # Business logic, services, background jobs
│   ├── RivrQuant.Api/             # ASP.NET Core Web API
│   └── RivrQuant.Tests/           # Unit + integration tests
├── frontend/                       # Next.js dashboard
├── docs/                           # Documentation
├── docker-compose.yml              # Production Docker Compose
└── docker-compose.dev.yml          # Development Docker Compose
```

## API Endpoints

| Endpoint | Description |
|----------|-------------|
| `GET /api/dashboard` | Aggregated dashboard data |
| `GET /api/backtest` | List backtests |
| `GET /api/backtest/{id}` | Backtest details |
| `POST /api/backtest/{id}/analyze` | Trigger AI analysis |
| `GET /api/trading/positions` | Current positions |
| `POST /api/trading/orders` | Place order |
| `GET /api/analysis` | AI analysis reports |
| `GET /api/alert/rules` | Alert rules |
| `GET /api/strategy` | Strategies |
| `GET /health` | Health check |

## Documentation

- [Setup Guide](docs/SETUP.md)
- [Architecture](docs/ARCHITECTURE.md)
- [API Reference](docs/API.md)
- [Strategy Guide](docs/STRATEGIES.md)
- [Deployment](docs/DEPLOYMENT.md)

## License

Private repository. All rights reserved.
