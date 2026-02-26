# RivrQuant Runbook

Your single reference for running locally, connecting QuantConnect algorithms,
and deploying to AWS for paper trading.

---

## Table of Contents

1. [Running Locally](#1-running-locally)
2. [Connecting QuantConnect Algorithms](#2-connecting-quantconnect-algorithms)
3. [Tracking Algorithms in the Dashboard](#3-tracking-algorithms-in-the-dashboard)
4. [Deploying to AWS](#4-deploying-to-aws)
5. [Environment Variable Reference](#5-environment-variable-reference)

---

## 1. Running Locally

### Prerequisites

| Tool | Min version | Install |
|------|-------------|---------|
| .NET SDK | 8.0 | https://dotnet.microsoft.com/download |
| Node.js | 20 | https://nodejs.org |
| Docker (optional) | any | https://docker.com |

---

### Option A — Native (fastest for formula iteration)

This runs the API and frontend as native processes. No Docker required.
The API uses SQLite by default so there is no database to set up.

```bash
# 1. Clone
git clone https://github.com/k5tuck/RivrQuant.git
cd RivrQuant

# 2. Create your local env file
cp .env.example .env
# Open .env and fill in at minimum:
#   ALPACA_API_KEY, ALPACA_API_SECRET  (paper trading keys)
#   QC_USER_ID, QC_API_TOKEN, QC_PROJECT_IDS  (QuantConnect)

# 3. Start the API (Terminal 1)
cd src
dotnet restore
dotnet run --project RivrQuant.Api
# → API listening on http://localhost:5000
# → Hangfire dashboard at http://localhost:5000/hangfire
# → Health check at http://localhost:5000/health

# 4. Start the frontend (Terminal 2)
cd frontend
npm install
npm run dev
# → Dashboard at http://localhost:3000
```

The .env file is loaded automatically from the project root by the API via
ASP.NET Core's configuration system. The frontend reads NEXT_PUBLIC_* vars
from `frontend/.env.local` — copy the example if you need to override:
```bash
cp frontend/.env.local.example frontend/.env.local
```

**What works without API keys:**
The app loads and shows empty states. You can navigate all pages, place simulated
orders, and see the Hangfire job dashboard. Live data requires real keys.

---

### Option B — Docker (closer to prod)

Uses SQLite and in-memory Hangfire — no Postgres needed.

```bash
cp .env.example .env
# Fill in your API keys

docker compose -f docker-compose.dev.yml up --build

# → Frontend:  http://localhost:3000
# → API:       http://localhost:5000
# → Hangfire:  http://localhost:5000/hangfire
```

Stop with `Ctrl+C` then `docker compose -f docker-compose.dev.yml down`.
Data persists in a Docker volume between restarts.

---

## 2. Connecting QuantConnect Algorithms

### Step 1 — Get your QuantConnect credentials

1. Log in at https://www.quantconnect.com
2. Click your avatar → **My Account**
3. Note your **User ID** (a number, e.g. `123456`)
4. Go to **API Access** → generate or copy your **API Token**

### Step 2 — Find your Project IDs

For each backtest project you want to monitor:
1. Open the project in QuantConnect
2. Look at the URL: `quantconnect.com/project/789012` — `789012` is the Project ID
3. Collect all the IDs you want to import (comma-separated)

### Step 3 — Set the env vars

In your `.env` file:
```env
QC_USER_ID=123456
QC_API_TOKEN=your_api_token_here
QC_PROJECT_IDS=789012,345678,901234     # all projects you want polled
QC_POLL_INTERVAL_SECONDS=300            # how often to check (default: 5 min)
```

### Step 4 — What happens automatically

Once the API starts, the `BacktestPollingJob` Hangfire job runs every
`QC_POLL_INTERVAL_SECONDS` and does the following for each project ID:

```
QC API → list all backtests for project
       → for each backtest not yet in the database:
           → fetch full backtest detail (trades, equity curve, stats)
           → parse and store in database
           → compute risk metrics (Sharpe, Sortino, drawdown, win rate…)
           → detect market regimes (trending, mean-reverting, crisis)
           → run Claude AI analysis → deployment readiness score 0–10
           → save analysis report
```

You can also trigger a single backtest analysis manually:
```bash
curl -X POST http://localhost:5000/api/backtest/{backtest-id}/analyze
```

Or watch jobs run live in the Hangfire dashboard at `/hangfire`.

---

## 3. Tracking Algorithms in the Dashboard

### Backtests tab
Shows every imported backtest with:
- **Sharpe ratio**, Max Drawdown, Total Return, Win Rate
- **AI Score** (0–10): the Claude analysis deployment readiness score
- **Status**: Analyzed vs Pending (awaiting first AI run)

Click any row to see full detail: equity curve, trade log, regime breakdown,
AI strengths/weaknesses, critical warnings.

### Analysis tab
Filtered view of completed AI analysis reports with:
- Assessment classification (Strong/Moderate/Weak)
- Overfitting risk rating
- Warning count

### Strategies tab
Once a strategy name is associated with backtests in QuantConnect, all
backtests from that strategy group together here. Use the detail view to
compare multiple runs side by side.

### Trading tab (live)
Shows real-time positions and P&L from Alpaca (stocks) and Bybit (crypto).
The metrics at the top — Sharpe, Win Rate, Drawdown — pull from the last
30 days of live trading data.

### Watching a new algorithm get imported

1. Run a backtest in QuantConnect and wait for it to complete
2. Within `QC_POLL_INTERVAL_SECONDS` (default 5 min), the job fires
3. Refresh the Backtests tab — the new backtest appears as **Pending**
4. Within the same poll cycle, the AI analysis runs automatically
5. Status changes to **Analyzed** with a deployment score

You can force an immediate poll by going to the Hangfire dashboard
(`/hangfire`), finding the `backtest-polling` recurring job, and clicking
**Trigger now**.

---

## 4. Deploying to AWS

The simplest production path is a single EC2 instance running `docker compose`.
No ECS/Fargate complexity until you need horizontal scaling.

### 4.1 Launch an EC2 instance

1. Open the EC2 console and click **Launch Instance**
2. Settings:
   - **AMI**: Amazon Linux 2023 (or Ubuntu 24.04 LTS)
   - **Instance type**: `t3.small` (2 vCPU, 2 GB RAM) minimum; `t3.medium` recommended
   - **Storage**: 20 GB gp3
   - **Security group** — open these ports:
     | Port | Source | Purpose |
     |------|--------|---------|
     | 22 | Your IP only | SSH |
     | 80 | 0.0.0.0/0 | HTTP (for Let's Encrypt challenge) |
     | 443 | 0.0.0.0/0 | HTTPS |
3. Create or select a key pair, download the `.pem` file

> Do NOT open ports 3000 or 5000 publicly. Nginx will proxy them.

### 4.2 Point a domain at your EC2

In Route 53 (or your DNS provider), create two A records:
```
app.yourdomain.com  →  <EC2 Elastic IP>
api.yourdomain.com  →  <EC2 Elastic IP>
```

Use an **Elastic IP** (free when attached) so the address doesn't change on restart.

### 4.3 Install Docker and Nginx on the instance

```bash
ssh -i your-key.pem ec2-user@<EC2-IP>

# Amazon Linux 2023
sudo dnf install -y docker git nginx certbot python3-certbot-nginx
sudo systemctl enable --now docker
sudo systemctl enable --now nginx
sudo usermod -aG docker ec2-user
# Log out and back in for the group change to take effect
exit
ssh -i your-key.pem ec2-user@<EC2-IP>

# Install Docker Compose plugin
sudo mkdir -p /usr/local/lib/docker/cli-plugins
sudo curl -SL https://github.com/docker/compose/releases/latest/download/docker-compose-linux-x86_64 \
  -o /usr/local/lib/docker/cli-plugins/docker-compose
sudo chmod +x /usr/local/lib/docker/cli-plugins/docker-compose
docker compose version   # should print a version
```

### 4.4 Configure Nginx as a reverse proxy

```bash
sudo tee /etc/nginx/conf.d/rivrquant.conf > /dev/null << 'EOF'
server {
    listen 80;
    server_name app.yourdomain.com api.yourdomain.com;
    location / { return 301 https://$host$request_uri; }
}

server {
    listen 443 ssl http2;
    server_name app.yourdomain.com;

    ssl_certificate     /etc/letsencrypt/live/app.yourdomain.com/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/app.yourdomain.com/privkey.pem;

    location / {
        proxy_pass         http://localhost:3000;
        proxy_http_version 1.1;
        proxy_set_header   Upgrade $http_upgrade;
        proxy_set_header   Connection "upgrade";
        proxy_set_header   Host $host;
    }
}

server {
    listen 443 ssl http2;
    server_name api.yourdomain.com;

    ssl_certificate     /etc/letsencrypt/live/api.yourdomain.com/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/api.yourdomain.com/privkey.pem;

    location / {
        proxy_pass       http://localhost:5000;
        proxy_set_header Host $host;
    }
}
EOF

sudo nginx -t && sudo systemctl reload nginx
```

Get free TLS certificates:
```bash
sudo certbot --nginx -d app.yourdomain.com -d api.yourdomain.com
# Follow prompts — enter your email, agree to TOS
# Certbot auto-edits the nginx config with cert paths
sudo systemctl reload nginx
```

Certbot auto-renews certs via a systemd timer. No action needed.

### 4.5 Clone and configure the app

```bash
cd ~
git clone https://github.com/k5tuck/RivrQuant.git
cd RivrQuant

cat > .env << 'EOF'
# Database
DB_PASSWORD=choose_a_strong_random_password

# Alpaca — paper trading (ALPACA_PAPER=true means no real money)
ALPACA_API_KEY=PKxxxxxxxxxxxxxxxxxxxx
ALPACA_API_SECRET=xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
ALPACA_PAPER=true

# QuantConnect
QC_USER_ID=123456
QC_API_TOKEN=your_qc_api_token
QC_PROJECT_IDS=789012,345678
QC_POLL_INTERVAL_SECONDS=300

# Anthropic (AI analysis)
ANTHROPIC_API_KEY=sk-ant-xxxxxxxxxxxxxxxx

# CORS — your frontend domain (comma-separated for multiple)
CORS_ORIGINS=https://app.yourdomain.com

# These are baked into the frontend image at build time
NEXT_PUBLIC_API_URL=https://api.yourdomain.com
NEXT_PUBLIC_SIGNALR_URL=https://api.yourdomain.com/hubs/trading

# Optional — leave blank to skip email/SMS alerts
SENDGRID_API_KEY=
ALERT_EMAIL_RECIPIENTS=
TWILIO_ACCOUNT_SID=
TWILIO_AUTH_TOKEN=
TWILIO_FROM_NUMBER=
ALERT_SMS_RECIPIENTS=
EOF

chmod 600 .env   # only you can read it
```

### 4.6 Build and start

```bash
docker compose up --build -d

# Watch startup logs
docker compose logs -f

# Verify everything is healthy
curl https://api.yourdomain.com/health
# Should return: Healthy
```

On first start, the API runs `db.Database.Migrate()` which creates the
Postgres schema automatically. Subsequent deploys apply any new migrations.

### 4.7 Verify and test

```bash
# Check all containers are running
docker compose ps

# API health
curl https://api.yourdomain.com/health

# Hangfire dashboard (only accessible on the server itself for security)
curl http://localhost:5000/hangfire

# Open in browser
# https://app.yourdomain.com  →  dashboard
# https://api.yourdomain.com/swagger  →  API explorer (Production shows this if you uncomment in Program.cs)
```

### 4.8 Redeploying after code changes

```bash
cd ~/RivrQuant
git pull origin main          # or your branch
docker compose up --build -d  # rebuilds changed images, rolls containers
docker compose logs -f        # watch for errors
```

Database migrations run automatically on restart.

### 4.9 Making it survive reboots

```bash
# Enable docker to start on boot (already done if you used systemctl enable)
sudo systemctl enable docker

# Add the app as a systemd service
sudo tee /etc/systemd/system/rivrquant.service > /dev/null << 'EOF'
[Unit]
Description=RivrQuant Trading Platform
Requires=docker.service
After=docker.service

[Service]
Type=oneshot
RemainAfterExit=yes
WorkingDirectory=/home/ec2-user/RivrQuant
ExecStart=/usr/local/lib/docker/cli-plugins/docker-compose up -d
ExecStop=/usr/local/lib/docker/cli-plugins/docker-compose down
User=ec2-user

[Install]
WantedBy=multi-user.target
EOF

sudo systemctl enable rivrquant
```

---

## 5. Environment Variable Reference

### Required for paper trading

| Variable | Where to get it | Example |
|----------|----------------|---------|
| `ALPACA_API_KEY` | alpaca.markets → API Keys | `PKxxxxxxxx` |
| `ALPACA_API_SECRET` | alpaca.markets → API Keys | `xxxxxxxx` |
| `ALPACA_PAPER` | set to `true` for paper | `true` |
| `QC_USER_ID` | quantconnect.com → Account | `123456` |
| `QC_API_TOKEN` | quantconnect.com → API Access | `xxxxxxxx` |
| `QC_PROJECT_IDS` | URL of each QC project | `789012,345678` |
| `ANTHROPIC_API_KEY` | console.anthropic.com | `sk-ant-xxx` |

### Required for production (AWS)

| Variable | Purpose | Example |
|----------|---------|---------|
| `DB_PASSWORD` | Postgres password | `strong-random-pass` |
| `CORS_ORIGINS` | Frontend domain(s) | `https://app.yourdomain.com` |
| `NEXT_PUBLIC_API_URL` | API URL baked into frontend image | `https://api.yourdomain.com` |
| `NEXT_PUBLIC_SIGNALR_URL` | SignalR URL baked into frontend image | `https://api.yourdomain.com/hubs/trading` |

### Optional

| Variable | Default | Purpose |
|----------|---------|---------|
| `QC_POLL_INTERVAL_SECONDS` | `300` | How often to poll QC for new backtests |
| `BYBIT_API_KEY` | — | Crypto trading via Bybit |
| `BYBIT_API_SECRET` | — | Crypto trading via Bybit |
| `BYBIT_USE_TESTNET` | `true` | Bybit testnet vs mainnet |
| `SENDGRID_API_KEY` | — | Email alerts |
| `ALERT_EMAIL_RECIPIENTS` | — | Comma-separated email list |
| `TWILIO_ACCOUNT_SID` | — | SMS alerts |
| `TWILIO_AUTH_TOKEN` | — | SMS alerts |
| `TWILIO_FROM_NUMBER` | — | SMS sender number |
| `ALERT_SMS_RECIPIENTS` | — | Comma-separated phone numbers |
| `ANTHROPIC_MODEL` | `claude-sonnet-4-20250514` | Which Claude model to use for analysis |

### Key behaviors controlled by env

| Setting | Dev default | Prod (docker-compose.yml) |
|---------|------------|--------------------------|
| `ASPNETCORE_ENVIRONMENT` | `Development` | `Production` |
| `DATABASE_PROVIDER` | `Sqlite` | `PostgreSQL` |
| `ALPACA_PAPER` | `true` | `true` (keep until ready for live) |
| Hangfire storage | In-memory | PostgreSQL (survives restarts) |
| DB schema | `EnsureCreated()` | `Migrate()` (applies changes) |
| Swagger UI | Enabled | Disabled |
