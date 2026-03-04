# =============================================================================
# IAM Role for EC2
# =============================================================================

resource "aws_iam_role" "app" {
  name        = "rivrquant-ec2-role"
  description = "Allows the RivrQuant EC2 instance to read app secrets from Secrets Manager."

  assume_role_policy = jsonencode({
    Version = "2012-10-17"
    Statement = [{
      Effect    = "Allow"
      Principal = { Service = "ec2.amazonaws.com" }
      Action    = "sts:AssumeRole"
    }]
  })

  tags = { Name = "rivrquant-ec2-role" }
}

# =============================================================================
# Policy: read the single app secret
# =============================================================================

resource "aws_iam_policy" "secrets_read" {
  name        = "rivrquant-secrets-read"
  description = "Allow read-only access to the RivrQuant app config secret."

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [{
      Effect   = "Allow"
      Action   = ["secretsmanager:GetSecretValue", "secretsmanager:DescribeSecret"]
      Resource = aws_secretsmanager_secret.app.arn
    }]
  })
}

resource "aws_iam_role_policy_attachment" "secrets_read" {
  role       = aws_iam_role.app.name
  policy_arn = aws_iam_policy.secrets_read.arn
}

# SSM Managed Instance Core — enables Session Manager SSH-free access and
# CloudWatch agent registration (no cost beyond what you use).
resource "aws_iam_role_policy_attachment" "ssm_core" {
  role       = aws_iam_role.app.name
  policy_arn = "arn:aws:iam::aws:policy/AmazonSSMManagedInstanceCore"
}

# =============================================================================
# Instance Profile
# =============================================================================

resource "aws_iam_instance_profile" "app" {
  name = "rivrquant-ec2-profile"
  role = aws_iam_role.app.name
}
