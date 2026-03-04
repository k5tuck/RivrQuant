# =============================================================================
# Locals
# =============================================================================

locals {
  # If cors_origins not explicitly set, default to the app domain (or empty).
  cors_origins = var.cors_origins != "" ? var.cors_origins : (
    var.domain_name != "" ? "https://${var.domain_name}" : ""
  )

  # Frontend build-time URLs: must know the public hostname at docker build time.
  next_public_api_url     = var.domain_name != "" ? "https://${var.domain_name}" : ""
  next_public_signalr_url = var.domain_name != "" ? "https://${var.domain_name}/hubs/trading" : ""

  # certbot email: use explicit value or fall back to admin@<domain>.
  certbot_email = var.certbot_email != "" ? var.certbot_email : (
    var.domain_name != "" ? "admin@${var.domain_name}" : ""
  )

  # Inject git token into clone URL for private repos.
  git_clone_url = var.git_token != "" ? replace(
    var.git_repo_url, "://", "://${var.git_token}@"
  ) : var.git_repo_url
}

# =============================================================================
# AMI — Amazon Linux 2023 (latest, resolved at apply time via SSM)
# =============================================================================

data "aws_ssm_parameter" "al2023_ami" {
  name = "/aws/service/ami-amazon-linux-latest/al2023-ami-kernel-default-x86_64"
}

# =============================================================================
# VPC
# =============================================================================

resource "aws_vpc" "main" {
  cidr_block           = "10.0.0.0/16"
  enable_dns_hostnames = true
  enable_dns_support   = true

  tags = { Name = "rivrquant-vpc" }
}

resource "aws_internet_gateway" "main" {
  vpc_id = aws_vpc.main.id
  tags   = { Name = "rivrquant-igw" }
}

resource "aws_subnet" "public" {
  vpc_id                  = aws_vpc.main.id
  cidr_block              = "10.0.1.0/24"
  availability_zone       = "${var.aws_region}a"
  map_public_ip_on_launch = true

  tags = { Name = "rivrquant-public-subnet" }
}

resource "aws_route_table" "public" {
  vpc_id = aws_vpc.main.id

  route {
    cidr_block = "0.0.0.0/0"
    gateway_id = aws_internet_gateway.main.id
  }

  tags = { Name = "rivrquant-public-rt" }
}

resource "aws_route_table_association" "public" {
  subnet_id      = aws_subnet.public.id
  route_table_id = aws_route_table.public.id
}

# =============================================================================
# Security Group
# =============================================================================

resource "aws_security_group" "app" {
  name        = "rivrquant-sg"
  description = "RivrQuant: allow HTTP, HTTPS, and SSH"
  vpc_id      = aws_vpc.main.id

  ingress {
    description = "HTTP"
    from_port   = 80
    to_port     = 80
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }

  ingress {
    description = "HTTPS"
    from_port   = 443
    to_port     = 443
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }

  ingress {
    description = "SSH"
    from_port   = 22
    to_port     = 22
    protocol    = "tcp"
    cidr_blocks = [var.allowed_ssh_cidr]
  }

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }

  tags = { Name = "rivrquant-sg" }
}

# =============================================================================
# EC2 Instance
# =============================================================================

resource "aws_instance" "app" {
  ami                    = data.aws_ssm_parameter.al2023_ami.value
  instance_type          = var.instance_type
  key_name               = var.key_name
  subnet_id              = aws_subnet.public.id
  vpc_security_group_ids = [aws_security_group.app.id]
  iam_instance_profile   = aws_iam_instance_profile.app.name

  # Changing any templatefile var triggers instance replacement + re-bootstrap.
  user_data_replace_on_change = true
  user_data = templatefile("${path.module}/templates/user_data.sh.tpl", {
    secret_arn              = aws_secretsmanager_secret.app.arn
    aws_region              = var.aws_region
    git_clone_url           = local.git_clone_url
    domain_name             = var.domain_name
    certbot_email           = local.certbot_email
    next_public_api_url     = local.next_public_api_url
    next_public_signalr_url = local.next_public_signalr_url
  })

  root_block_device {
    volume_type = "gp3"
    volume_size = var.volume_size_gb
    encrypted   = true
  }

  tags = { Name = "rivrquant-app" }
}

# =============================================================================
# Elastic IP
# =============================================================================

resource "aws_eip" "app" {
  instance = aws_instance.app.id
  domain   = "vpc"
  tags     = { Name = "rivrquant-eip" }
}

# =============================================================================
# Route53 A Record (optional)
# =============================================================================

resource "aws_route53_record" "app" {
  count = var.route53_zone_id != "" && var.domain_name != "" ? 1 : 0

  zone_id = var.route53_zone_id
  name    = var.domain_name
  type    = "A"
  ttl     = 300
  records = [aws_eip.app.public_ip]
}
