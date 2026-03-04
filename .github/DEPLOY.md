# Deploying RivrQuant to AWS

## Overview

```
GitHub push → CI (build/test) → Terraform apply (infra) → SSH redeploy (code)
```

Two independent workflows handle deployment:

| Workflow | File | Trigger |
|---|---|---|
| **Terraform** | `.github/workflows/terraform.yml` | Push to `main` (infra changes) or manual |
| **Deploy** | `.github/workflows/deploy.yml` | After Terraform succeeds, or manual |
| **CI** | `.github/workflows/ci.yml` | Every push/PR |

---

## First-time Setup

### 1. Configure Terraform locally

```bash
cd infra/terraform
cp terraform.tfvars.example terraform.tfvars
# Edit terraform.tfvars with your values
terraform init
terraform apply
```

After apply, note the outputs — especially `public_ip` and `secret_arn`.

### 2. Create an EC2 Key Pair (if you don't have one)

```bash
aws ec2 create-key-pair --key-name rivrquant --query KeyMaterial --output text \
  > ~/.ssh/rivrquant.pem
chmod 400 ~/.ssh/rivrquant.pem
```

Set `key_name = "rivrquant"` in `terraform.tfvars`.

### 3. Add GitHub Repository Secrets

Go to **Settings → Secrets and variables → Actions → Secrets** and add:

| Secret | Value |
|---|---|
| `AWS_ACCESS_KEY_ID` | IAM user key with EC2, IAM, Secrets Manager, Route53 permissions |
| `AWS_SECRET_ACCESS_KEY` | Corresponding secret |
| `EC2_SSH_PRIVATE_KEY` | Contents of `~/.ssh/rivrquant.pem` |
| `TF_VAR_DB_PASSWORD` | PostgreSQL password |
| `TF_VAR_QC_USER_ID` | QuantConnect user ID |
| `TF_VAR_QC_API_TOKEN` | QuantConnect API token |
| `TF_VAR_ALPACA_API_KEY` | Alpaca API key |
| `TF_VAR_ALPACA_API_SECRET` | Alpaca API secret |
| `TF_VAR_BYBIT_API_KEY` | Bybit API key |
| `TF_VAR_BYBIT_API_SECRET` | Bybit API secret |
| `TF_VAR_ANTHROPIC_API_KEY` | Anthropic API key |
| `TF_VAR_SENDGRID_API_KEY` | SendGrid API key |
| `TF_VAR_TWILIO_ACCOUNT_SID` | Twilio account SID |
| `TF_VAR_TWILIO_AUTH_TOKEN` | Twilio auth token |
| `TF_VAR_GIT_TOKEN` | GitHub PAT for cloning (if private repo) |

### 4. Add GitHub Repository Variables

Go to **Settings → Secrets and variables → Actions → Variables** and add:

| Variable | Example |
|---|---|
| `AWS_REGION` | `us-east-1` |
| `EC2_HOST` | `54.123.45.67` (Elastic IP from Terraform output) |
| `TF_KEY_NAME` | `rivrquant` |
| `TF_GIT_REPO_URL` | `https://github.com/your-org/RivrQuant.git` |
| `TF_QC_PROJECT_IDS` | `12345,67890` |
| `TF_ALERT_EMAIL_RECIPIENTS` | `you@example.com` |
| `TF_TWILIO_FROM_NUMBER` | `+12025550123` |
| `TF_ALERT_SMS_RECIPIENTS` | `+12025550124` |
| `TF_SECRET_ARN` | ARN from `terraform output secret_arn` |
| `TF_DOMAIN_NAME` | `app.yourdomain.com` _(optional)_ |
| `TF_ROUTE53_ZONE_ID` | `Z1234567890ABC` _(optional)_ |
| `TF_CERTBOT_EMAIL` | `you@yourdomain.com` _(optional)_ |
| `TF_INSTANCE_TYPE` | `t3.medium` _(optional)_ |
| `TF_ALLOWED_SSH_CIDR` | `1.2.3.4/32` _(optional)_ |
| `TF_ALPACA_PAPER` | `true` _(optional)_ |
| `TF_BYBIT_USE_TESTNET` | `true` _(optional)_ |
| `TF_RIVRQUANT_MODE` | `Paper` _(optional)_ |

---

## Deployment Workflows

### Infra + code deploy (recommended)
Push to `main` — CI runs, then Terraform applies any infra changes, then the Deploy workflow SSH-redeploys the app.

### Code-only redeploy (no infra changes)
```
GitHub → Actions → Deploy — SSH Re-deploy to EC2 → Run workflow
```

### Manual Terraform plan/apply/destroy
```
GitHub → Actions → Terraform — Plan & Apply → Run workflow → choose action
```

### SSH directly
```bash
ssh -i ~/.ssh/rivrquant.pem ec2-user@<EC2_HOST>
bash /opt/rivrquant/infra/scripts/redeploy.sh
```

---

## Monitoring Bootstrap Progress

On first launch, the instance runs the cloud-init bootstrap (installs Docker, clones repo, builds images). This takes 5–10 minutes.

```bash
# Tail the bootstrap log
ssh -i ~/.ssh/rivrquant.pem ec2-user@<EC2_HOST> \
  'sudo tail -f /var/log/cloud-init-output.log'
```

---

## Rotating Secrets

1. Update the secret value in `terraform.tfvars` (or directly in AWS Secrets Manager console).
2. Run `terraform apply` to update the Secrets Manager version.
3. Run the Deploy workflow (or `redeploy.sh`) — it re-fetches secrets on every deploy.
