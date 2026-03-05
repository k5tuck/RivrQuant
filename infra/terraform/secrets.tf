# =============================================================================
# Secrets Manager — single JSON secret with all env vars
# =============================================================================
#
# The EC2 bootstrap script fetches this secret via the instance IAM profile,
# then writes it to /opt/rivrquant/.env for docker-compose to consume.
# To rotate a secret: update the value here, run `terraform apply`, then
# run infra/scripts/redeploy.sh on the instance.

resource "aws_secretsmanager_secret" "app" {
  name                    = "rivrquant/app-config"
  description             = "All RivrQuant application environment variables"
  recovery_window_in_days = 7

  tags = { Name = "rivrquant-app-config" }
}

resource "aws_secretsmanager_secret_version" "app" {
  secret_id = aws_secretsmanager_secret.app.id

  secret_string = jsonencode({
    # --- Application ---
    ASPNETCORE_ENVIRONMENT = "Production"
    RIVRQUANT_MODE         = var.rivrquant_mode

    # --- Database ---
    DATABASE_PROVIDER   = "PostgreSQL"
    DATABASE_CONNECTION = "Host=postgres;Database=rivrquant;Username=rq;Password=${var.db_password}"
    DB_PASSWORD         = var.db_password

    # --- QuantConnect ---
    QC_USER_ID               = var.qc_user_id
    QC_API_TOKEN             = var.qc_api_token
    QC_PROJECT_IDS           = var.qc_project_ids
    QC_POLL_INTERVAL_SECONDS = tostring(var.qc_poll_interval_seconds)

    # --- Alpaca ---
    ALPACA_API_KEY    = var.alpaca_api_key
    ALPACA_API_SECRET = var.alpaca_api_secret
    ALPACA_PAPER      = tostring(var.alpaca_paper)
    ALPACA_BASE_URL   = var.alpaca_base_url

    # --- Bybit ---
    BYBIT_API_KEY     = var.bybit_api_key
    BYBIT_API_SECRET  = var.bybit_api_secret
    BYBIT_USE_TESTNET = tostring(var.bybit_use_testnet)
    BYBIT_TESTNET_URL = var.bybit_testnet_url
    BYBIT_LIVE_URL    = var.bybit_live_url

    # --- Anthropic ---
    ANTHROPIC_API_KEY    = var.anthropic_api_key
    ANTHROPIC_MODEL      = var.anthropic_model
    ANTHROPIC_MAX_TOKENS = tostring(var.anthropic_max_tokens)

    # --- SendGrid ---
    SENDGRID_API_KEY       = var.sendgrid_api_key
    SENDGRID_FROM_EMAIL    = var.sendgrid_from_email
    SENDGRID_FROM_NAME     = var.sendgrid_from_name
    ALERT_EMAIL_RECIPIENTS = var.alert_email_recipients

    # --- Twilio ---
    TWILIO_ACCOUNT_SID   = var.twilio_account_sid
    TWILIO_AUTH_TOKEN    = var.twilio_auth_token
    TWILIO_FROM_NUMBER   = var.twilio_from_number
    ALERT_SMS_RECIPIENTS = var.alert_sms_recipients

    # --- CORS / SignalR ---
    CORS_ORIGINS   = local.cors_origins
    SIGNALR_ENABLE = "true"

    # --- Frontend build-time vars (consumed by docker-compose build args) ---
    NEXT_PUBLIC_API_URL     = local.next_public_api_url
    NEXT_PUBLIC_SIGNALR_URL = local.next_public_signalr_url
  })
}
