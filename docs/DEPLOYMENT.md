# Deployment Guide

## Docker Compose (Recommended)

### Production

```bash
# Create environment file
cp .env.example .env
# Edit .env with production API keys and a strong DB_PASSWORD

# Build and start all services
docker compose up -d --build

# Check service health
curl http://localhost:5000/health
```

Services:
- **api** — ASP.NET Core backend on port 5000
- **frontend** — Next.js dashboard on port 3000
- **postgres** — PostgreSQL 16 on port 5432

### Development

```bash
docker compose -f docker-compose.dev.yml up --build
```

Uses SQLite instead of PostgreSQL for zero-config development.

## Manual Deployment

### Backend

```bash
cd src
dotnet publish RivrQuant.Api -c Release -o ./publish

# Run the published app
cd publish
dotnet RivrQuant.Api.dll
```

### Frontend

```bash
cd frontend
npm run build
npm start
```

## Environment Variables

See `.env.example` for the complete list. Critical variables:

| Variable | Required | Description |
|----------|----------|-------------|
| `DATABASE_PROVIDER` | Yes | `Sqlite` or `PostgreSQL` |
| `DATABASE_CONNECTION` | Yes | Connection string |
| `ANTHROPIC_API_KEY` | Yes | Claude API key for AI analysis |
| `ALPACA_API_KEY` | No | Alpaca API key (for stock trading) |
| `BYBIT_API_KEY` | No | Bybit API key (for crypto trading) |
| `SENDGRID_API_KEY` | No | SendGrid key (for email alerts) |
| `TWILIO_ACCOUNT_SID` | No | Twilio SID (for SMS alerts) |

## Health Checks

The API exposes a health endpoint at `/health` that checks:
- Database connectivity
- Service availability

## Monitoring

- **Hangfire Dashboard** — Available at `/hangfire` (development only)
- **Swagger UI** — Available at `/swagger` (development only)
- **SignalR** — Real-time connection status shown in the frontend sidebar

## Security Considerations

1. Never commit `.env` files or API keys to version control
2. Use strong passwords for the PostgreSQL database
3. Enable API key authentication in production (`X-Api-Key` header)
4. Run behind a reverse proxy (nginx/Caddy) with HTTPS in production
5. Restrict CORS origins to your frontend domain
6. Use .NET User Secrets for local development instead of `.env` files
