using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using RivrQuant.Infrastructure.QuantConnect;

namespace RivrQuant.Tests.Integration;

/// <summary>
/// Integration tests for QcApiClient. These tests require valid QuantConnect credentials
/// and are skipped when credentials are not available.
/// </summary>
public class QcApiClientTests
{
    private static bool HasCredentials()
    {
        var userId = Environment.GetEnvironmentVariable("QC_USER_ID");
        var token = Environment.GetEnvironmentVariable("QC_API_TOKEN");
        return !string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(token) && userId != "demo";
    }

    [Fact]
    public void QcConfiguration_Validate_ThrowsOnMissingUserId()
    {
        var config = new QcConfiguration
        {
            UserId = "",
            ApiToken = "some-token",
            ProjectIds = new List<string> { "12345" }
        };

        var act = () => config.Validate();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void QcConfiguration_Validate_ThrowsOnMissingToken()
    {
        var config = new QcConfiguration
        {
            UserId = "12345",
            ApiToken = "",
            ProjectIds = new List<string> { "12345" }
        };

        var act = () => config.Validate();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void QcConfiguration_Validate_SucceedsWithValidConfig()
    {
        var config = new QcConfiguration
        {
            UserId = "12345",
            ApiToken = "valid-token",
            ProjectIds = new List<string> { "12345" }
        };

        var act = () => config.Validate();

        act.Should().NotThrow();
    }

    [Fact(Skip = "Requires valid QC credentials")]
    public async Task ListProjectsAsync_WithValidCredentials_ReturnsProjects()
    {
        if (!HasCredentials()) return;

        var config = Options.Create(new QcConfiguration
        {
            UserId = Environment.GetEnvironmentVariable("QC_USER_ID")!,
            ApiToken = Environment.GetEnvironmentVariable("QC_API_TOKEN")!,
            ProjectIds = new List<string> { "12345" }
        });
        var logger = new Mock<ILogger<QcApiClient>>();
        var httpClient = new HttpClient();

        var client = new QcApiClient(config, httpClient, logger.Object);
        var projects = await client.ListProjectsAsync(CancellationToken.None);

        projects.Should().NotBeNull();
    }

    [Fact(Skip = "Requires valid QC credentials")]
    public async Task GetBacktestsForProjectAsync_WithValidProject_ReturnsResults()
    {
        if (!HasCredentials()) return;

        var projectIds = Environment.GetEnvironmentVariable("QC_PROJECT_IDS")?.Split(',') ?? Array.Empty<string>();
        if (projectIds.Length == 0) return;

        var config = Options.Create(new QcConfiguration
        {
            UserId = Environment.GetEnvironmentVariable("QC_USER_ID")!,
            ApiToken = Environment.GetEnvironmentVariable("QC_API_TOKEN")!,
            ProjectIds = projectIds.ToList()
        });
        var logger = new Mock<ILogger<QcApiClient>>();
        var httpClient = new HttpClient();

        var client = new QcApiClient(config, httpClient, logger.Object);
        var backtests = await client.GetBacktestsForProjectAsync(projectIds[0], CancellationToken.None);

        backtests.Should().NotBeNull();
    }
}
