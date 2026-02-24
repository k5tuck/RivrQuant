// Copyright (c) RivrQuant. All rights reserved.
// Licensed under the MIT License.

namespace RivrQuant.Infrastructure.QuantConnect;

/// <summary>
/// Configuration for QuantConnect API integration.
/// Binds to the <c>QuantConnect</c> section in appsettings and supports
/// environment-variable overrides for sensitive values (QC_USER_ID, QC_API_TOKEN).
/// </summary>
public sealed class QcConfiguration
{
    /// <summary>
    /// The configuration section name used in appsettings.json.
    /// </summary>
    public const string SectionName = "QuantConnect";

    /// <summary>
    /// The QuantConnect user identifier (numeric string).
    /// Can be set via the <c>QC_USER_ID</c> environment variable.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// The QuantConnect API token used for authentication.
    /// Can be set via the <c>QC_API_TOKEN</c> environment variable.
    /// </summary>
    public string ApiToken { get; set; } = string.Empty;

    /// <summary>
    /// The list of QuantConnect project identifiers to monitor for new backtests.
    /// </summary>
    public IReadOnlyList<string> ProjectIds { get; set; } = Array.Empty<string>();

    /// <summary>
    /// The interval, in seconds, between polling cycles for new backtest results.
    /// Defaults to 300 seconds (5 minutes).
    /// </summary>
    public int PollIntervalSeconds { get; set; } = 300;

    /// <summary>
    /// The base URL for the QuantConnect REST API.
    /// Defaults to <c>https://www.quantconnect.com</c>.
    /// </summary>
    public string BaseUrl { get; set; } = "https://www.quantconnect.com";

    /// <summary>
    /// Validates that all required configuration values are present.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <see cref="UserId"/> or <see cref="ApiToken"/> is missing or whitespace.
    /// </exception>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(UserId))
            throw new InvalidOperationException(
                "QC_USER_ID is required. Set it in environment variables or appsettings.");

        if (string.IsNullOrWhiteSpace(ApiToken))
            throw new InvalidOperationException(
                "QC_API_TOKEN is required. Set it in environment variables or appsettings.");
    }
}
