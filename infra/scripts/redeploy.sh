#!/usr/bin/env bash
# =============================================================================
# RivrQuant Re-deploy Script
# Run on the EC2 instance to pull the latest code and refresh secrets.
#
# Usage:
#   ssh -i ~/.ssh/<key>.pem ec2-user@<ip> 'bash /opt/rivrquant/infra/scripts/redeploy.sh'
#
# What it does:
#   1. Pulls latest code from the configured git remote
#   2. Re-fetches all secrets from AWS Secrets Manager → .env
#   3. Rebuilds and restarts Docker Compose services
#   4. Reloads nginx configuration
# =============================================================================
set -euo pipefail

APP_DIR="/opt/rivrquant"
COMPOSE_FILE="$APP_DIR/docker-compose.yml"
LOG_TAG="rivrquant-redeploy"

log() { echo "[$(date -u '+%Y-%m-%dT%H:%M:%SZ')] $*" | tee >(logger -t "$LOG_TAG"); }

# ---------------------------------------------------------------------------
# Resolve secret ARN from the .env metadata comment (written at bootstrap),
# or fall back to the well-known secret name.
# ---------------------------------------------------------------------------
SECRET_ID="${RIVRQUANT_SECRET_ARN:-rivrquant/app-config}"
AWS_REGION="${AWS_REGION:-$(curl -s --max-time 5 \
  http://169.254.169.254/latest/meta-data/placement/region || echo "us-east-1")}"

log "=== RivrQuant re-deploy started ==="
log "App dir  : $APP_DIR"
log "Secret   : $SECRET_ID"
log "Region   : $AWS_REGION"

# ---------------------------------------------------------------------------
# 1. Pull latest code
# ---------------------------------------------------------------------------
log "Pulling latest code..."
cd "$APP_DIR"
git fetch origin
git reset --hard origin/HEAD
log "Code updated to $(git rev-parse --short HEAD)."

# ---------------------------------------------------------------------------
# 2. Refresh secrets → .env
# ---------------------------------------------------------------------------
log "Fetching secrets from AWS Secrets Manager..."
SECRET_JSON=$(aws secretsmanager get-secret-value \
  --secret-id  "$SECRET_ID" \
  --region     "$AWS_REGION" \
  --query      SecretString \
  --output     text)

echo "$SECRET_JSON" | jq -r 'to_entries[] | .key + "=" + (.value | @sh)' \
  > "$APP_DIR/.env"

chmod 600 "$APP_DIR/.env"
log ".env refreshed with $(wc -l < "$APP_DIR/.env") variables."

# ---------------------------------------------------------------------------
# 3. Rebuild and restart containers
# ---------------------------------------------------------------------------
log "Rebuilding Docker images and restarting services..."

# Source the .env so NEXT_PUBLIC_* build args are available to docker compose.
set -a; source "$APP_DIR/.env"; set +a

docker compose -f "$COMPOSE_FILE" up --build -d --remove-orphans

log "Containers:"
docker compose -f "$COMPOSE_FILE" ps

# ---------------------------------------------------------------------------
# 4. Reload nginx (pick up any config changes from the new code, if any)
# ---------------------------------------------------------------------------
if systemctl is-active --quiet nginx; then
  nginx -t && systemctl reload nginx && log "nginx reloaded."
fi

log "=== RivrQuant re-deploy complete: $(date -u) ==="
