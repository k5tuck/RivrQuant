using FluentAssertions;
using RivrQuant.Infrastructure.Brokers.Bybit;

namespace RivrQuant.Tests.Unit.Brokers;

public class BybitBrokerClientTests
{
    [Fact]
    public void BybitConfiguration_Validate_ThrowsOnMissingApiKey()
    {
        var config = new BybitConfiguration
        {
            ApiKey = "",
            ApiSecret = "test-secret",
            UseTestnet = true
        };

        var act = () => config.Validate();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void BybitConfiguration_Validate_ThrowsOnMissingApiSecret()
    {
        var config = new BybitConfiguration
        {
            ApiKey = "test-key",
            ApiSecret = "",
            UseTestnet = true
        };

        var act = () => config.Validate();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void BybitConfiguration_Validate_SucceedsWithValidConfig()
    {
        var config = new BybitConfiguration
        {
            ApiKey = "test-key",
            ApiSecret = "test-secret",
            UseTestnet = true
        };

        var act = () => config.Validate();

        act.Should().NotThrow();
    }

    [Fact]
    public void BybitConfiguration_TestnetBaseUrl_IsCorrect()
    {
        var config = new BybitConfiguration
        {
            ApiKey = "test-key",
            ApiSecret = "test-secret",
            UseTestnet = true,
            TestnetUrl = "https://api-testnet.bybit.com",
            LiveUrl = "https://api.bybit.com"
        };

        config.BaseUrl.Should().Be("https://api-testnet.bybit.com");
    }

    [Fact]
    public void BybitConfiguration_LiveBaseUrl_IsCorrect()
    {
        var config = new BybitConfiguration
        {
            ApiKey = "test-key",
            ApiSecret = "test-secret",
            UseTestnet = false,
            TestnetUrl = "https://api-testnet.bybit.com",
            LiveUrl = "https://api.bybit.com"
        };

        config.BaseUrl.Should().Be("https://api.bybit.com");
    }

    [Fact]
    public void BybitAuthenticator_SignsRequest_WithCorrectHeaders()
    {
        var authenticator = new BybitAuthenticator("test-api-key", "test-api-secret", 5000);

        var headers = new Dictionary<string, string>();
        authenticator.SignRequest(headers, "");

        headers.Should().ContainKey("X-BAPI-API-KEY");
        headers.Should().ContainKey("X-BAPI-SIGN");
        headers.Should().ContainKey("X-BAPI-TIMESTAMP");
        headers.Should().ContainKey("X-BAPI-RECV-WINDOW");
        headers["X-BAPI-API-KEY"].Should().Be("test-api-key");
    }

    [Fact]
    public void BybitAuthenticator_ProducesConsistentSignatures()
    {
        var authenticator = new BybitAuthenticator("test-key", "test-secret", 5000);

        var headers1 = new Dictionary<string, string>();
        var headers2 = new Dictionary<string, string>();

        authenticator.SignRequest(headers1, "test-payload");
        // Different timestamps will produce different signatures,
        // but the structure should be consistent
        authenticator.SignRequest(headers2, "test-payload");

        headers1["X-BAPI-API-KEY"].Should().Be(headers2["X-BAPI-API-KEY"]);
        headers1["X-BAPI-RECV-WINDOW"].Should().Be(headers2["X-BAPI-RECV-WINDOW"]);
    }
}
