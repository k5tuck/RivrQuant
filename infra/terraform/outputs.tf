output "public_ip" {
  description = "Elastic IP address of the RivrQuant EC2 instance."
  value       = aws_eip.app.public_ip
}

output "app_url" {
  description = "URL to access the application (HTTPS if domain set, HTTP+IP otherwise)."
  value = var.domain_name != "" ? "https://${var.domain_name}" : "http://${aws_eip.app.public_ip}"
}

output "api_url" {
  description = "Direct URL for the .NET API."
  value = var.domain_name != "" ? "https://${var.domain_name}/api" : "http://${aws_eip.app.public_ip}:5000"
}

output "ssh_command" {
  description = "SSH command to connect to the instance."
  value       = "ssh -i ~/.ssh/${var.key_name}.pem ec2-user@${aws_eip.app.public_ip}"
}

output "bootstrap_log" {
  description = "SSH command to tail the cloud-init bootstrap log (useful during first-run setup)."
  value       = "ssh -i ~/.ssh/${var.key_name}.pem ec2-user@${aws_eip.app.public_ip} 'sudo tail -f /var/log/cloud-init-output.log'"
}

output "secret_arn" {
  description = "ARN of the AWS Secrets Manager secret holding all app environment variables."
  value       = aws_secretsmanager_secret.app.arn
}

output "instance_id" {
  description = "EC2 instance ID (useful for AWS Console / SSM Session Manager)."
  value       = aws_instance.app.id
}
