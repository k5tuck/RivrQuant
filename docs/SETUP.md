# Setup Guide

## Prerequisites

- **.NET 8 SDK** — [Download](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Node.js 20+** — [Download](https://nodejs.org/)
- **Docker** (optional) — [Download](https://www.docker.com/)

## External Service Accounts

### Required

1. **Anthropic (Claude AI)**
   - Create account at https://console.anthropic.com/
   - Generate API key under API Keys
   - Set `ANTHROPIC_API_KEY` in your environment

### Optional (enable as needed)

2. **QuantConnect**
   - Sign up at https://www.quantconnect.com/
   - Find User ID and API Token in Account Settings
   - Set `QC_USER_ID`, `QC_API_TOKEN`, `QC_PROJECT_IDS`

3. **Alpaca Markets (Stock Trading)**
   - Sign up at https://alpaca.markets/
   - Generate API keys (paper trading recommended for testing)
   - Set `ALPACA_API_KEY`, `ALPACA_API_SECRET`, `ALPACA_PAPER=true`

4. **Bybit (Crypto Trading)**
   - Sign up at https://www.bybit.com/
   - Generate API keys (testnet recommended for testing)
   - Set `BYBIT_API_KEY`, `BYBIT_API_SECRET`, `BYBIT_USE_TESTNET=true`

5. **SendGrid (Email Alerts)**
   - Sign up at https://sendgrid.com/
   - Generate API key with Mail Send permission
   - Set `SENDGRID_API_KEY`, `SENDGRID_FROM_EMAIL`, `ALERT_EMAIL_RECIPIENTS`

6. **Twilio (SMS Alerts)**
   - Sign up at https://www.twilio.com/
   - Get Account SID, Auth Token, and a phone number
   - Set `TWILIO_ACCOUNT_SID`, `TWILIO_AUTH_TOKEN`, `TWILIO_FROM_NUMBER`, `ALERT_SMS_RECIPIENTS`

## Configuration

1. Copy the example environment file:
```bash
cp .env.example .env
```

2. Edit `.env` with your API keys and preferences.

3. For the backend, configuration can also be set via .NET User Secrets:
```bash
cd src/RivrQuant.Api
dotnet user-secrets set "Anthropic:ApiKey" "sk-ant-your-key"
dotnet user-secrets set "Alpaca:ApiKey" "your-alpaca-key"
```

## Running Locally

### Backend

```bash
cd src
dotnet restore
dotnet run --project RivrQuant.Api
```

The API starts at http://localhost:5000. Swagger UI available at http://localhost:5000/swagger.

### Frontend

```bash
cd frontend
npm install
npm run dev
```

The dashboard starts at http://localhost:3000.

### Running Tests

```bash
cd src
dotnet test
```

## Database

### Development (SQLite)
No setup needed — the database file is created automatically on first run.

### Production (PostgreSQL)
```bash
# Via Docker
docker run -d --name rivrquant-db \
  -e POSTGRES_DB=rivrquant \
  -e POSTGRES_USER=rq \
  -e POSTGRES_PASSWORD=changeme \
  -p 5432:5432 \
  postgres:16-alpine
```

Set `DATABASE_PROVIDER=PostgreSQL` and update `DATABASE_CONNECTION` in your environment.
