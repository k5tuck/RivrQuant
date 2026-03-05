terraform {
  required_version = ">= 1.6"

  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 5.0"
    }
  }

  # Uncomment and configure to store state in S3 (recommended for teams).
  # Create the bucket manually before running `terraform init`.
  #
  # backend "s3" {
  #   bucket = "your-terraform-state-bucket"
  #   key    = "rivrquant/terraform.tfstate"
  #   region = "us-east-1"
  # }
}

provider "aws" {
  region = var.aws_region
}
