# =============================================================================
# Infrastructure
# =============================================================================

variable "aws_region" {
  description = "AWS region to deploy into."
  type        = string
  default     = "us-east-1"
}

variable "instance_type" {
  description = "EC2 instance type. t3.medium (2 vCPU / 4 GB) is the recommended minimum for running .NET API + Next.js + PostgreSQL concurrently."
  type        = string
  default     = "t3.medium"
}

variable "key_name" {
  description = "Name of an existing EC2 key pair for SSH access."
  type        = string
}

variable "allowed_ssh_cidr" {
  description = "CIDR block allowed to SSH into the instance. Restrict to your IP for production (e.g. '1.2.3.4/32')."
  type        = string
  default     = "0.0.0.0/0"
}

variable "volume_size_gb" {
  description = "Root EBS volume size in GB."
  type        = number
  default     = 40
}

# =============================================================================
# Networking / Domain
# =============================================================================

variable "domain_name" {
  description = "Fully-qualified domain name for the app (e.g. 'app.yourdomain.com'). Leave empty to access by IP only (no TLS)."
  type        = string
  default     = ""
}

variable "route53_zone_id" {
  description = "Route53 hosted zone ID. When set alongside domain_name, an A record pointing to the Elastic IP is created automatically."
  type        = string
  default     = ""
}

variable "certbot_email" {
  description = "Email address for Let's Encrypt TLS certificate notifications. Required when domain_name is set."
  type        = string
  default     = ""
}

# =============================================================================
# Git
# =============================================================================

variable "git_repo_url" {
  description = "HTTPS URL of the Git repository to clone on the EC2 instance (e.g. 'https://github.com/org/RivrQuant.git')."
  type        = string
}

variable "git_token" {
  description = "Personal access token for cloning a private repository. Injected into the clone URL at bootstrap. Leave empty for public repos."
  type        = string
  sensitive   = true
  default     = ""
}

# =============================================================================
# Application
# =============================================================================

variable "rivrquant_mode" {
  description = "Trading mode: Paper | LiveTestnet | LiveProduction."
  type        = string
  default     = "Paper"
}

variable "cors_origins" {
  description = "Comma-separated CORS-allowed origins (e.g. 'https://app.yourdomain.com'). Defaults to the domain_name when set."
  type        = string
  default     = ""
}

# =============================================================================
# Database
# =============================================================================

variable "db_password" {
  description = "PostgreSQL password for the 'rq' user."
  type        = string
  sensitive   = true
}

# =============================================================================
# QuantConnect
# =============================================================================

variable "qc_user_id" {
  description = "QuantConnect user ID."
  type        = string
  sensitive   = true
}

variable "qc_api_token" {
  description = "QuantConnect API token."
  type        = string
  sensitive   = true
}

variable "qc_project_ids" {
  description = "Comma-separated QuantConnect project IDs to poll (e.g. '12345,67890')."
  type        = string
}

variable "qc_poll_interval_seconds" {
  description = "How often (in seconds) to poll QuantConnect for new backtest results."
  type        = number
  default     = 300
}

# =============================================================================
# Alpaca (Stocks)
# =============================================================================

variable "alpaca_api_key" {
  description = "Alpaca API key."
  type        = string
  sensitive   = true
}

variable "alpaca_api_secret" {
  description = "Alpaca API secret."
  type        = string
  sensitive   = true
}

variable "alpaca_paper" {
  description = "Use Alpaca paper trading endpoint (true) or live (false)."
  type        = bool
  default     = true
}

variable "alpaca_base_url" {
  description = "Alpaca base URL. Switch to 'https://api.alpaca.markets' for live trading."
  type        = string
  default     = "https://paper-api.alpaca.markets"
}

# =============================================================================
# Bybit (Crypto)
# =============================================================================

variable "bybit_api_key" {
  description = "Bybit API key."
  type        = string
  sensitive   = true
}

variable "bybit_api_secret" {
  description = "Bybit API secret."
  type        = string
  sensitive   = true
}

variable "bybit_use_testnet" {
  description = "Use Bybit testnet (true) or live mainnet (false)."
  type        = bool
  default     = true
}

variable "bybit_testnet_url" {
  description = "Bybit testnet base URL."
  type        = string
  default     = "https://api-testnet.bybit.com"
}

variable "bybit_live_url" {
  description = "Bybit live mainnet base URL."
  type        = string
  default     = "https://api.bybit.com"
}

# =============================================================================
# Anthropic (AI Analysis)
# =============================================================================

variable "anthropic_api_key" {
  description = "Anthropic API key for Claude AI backtest analysis."
  type        = string
  sensitive   = true
}

variable "anthropic_model" {
  description = "Anthropic model ID to use for analysis."
  type        = string
  default     = "claude-sonnet-4-20250514"
}

variable "anthropic_max_tokens" {
  description = "Maximum tokens per Anthropic API response."
  type        = number
  default     = 4096
}

# =============================================================================
# SendGrid (Email Alerts)
# =============================================================================

variable "sendgrid_api_key" {
  description = "SendGrid API key for email alerts."
  type        = string
  sensitive   = true
}

variable "sendgrid_from_email" {
  description = "Sender email address for alerts."
  type        = string
  default     = "alerts@rivrquant.com"
}

variable "sendgrid_from_name" {
  description = "Sender display name for alerts."
  type        = string
  default     = "RivrQuant Alerts"
}

variable "alert_email_recipients" {
  description = "Comma-separated email addresses to receive alerts."
  type        = string
}

# =============================================================================
# Twilio (SMS Alerts)
# =============================================================================

variable "twilio_account_sid" {
  description = "Twilio account SID."
  type        = string
  sensitive   = true
}

variable "twilio_auth_token" {
  description = "Twilio auth token."
  type        = string
  sensitive   = true
}

variable "twilio_from_number" {
  description = "Twilio sender phone number in E.164 format (e.g. '+12025550123')."
  type        = string
}

variable "alert_sms_recipients" {
  description = "Comma-separated phone numbers (E.164) to receive SMS alerts."
  type        = string
}
