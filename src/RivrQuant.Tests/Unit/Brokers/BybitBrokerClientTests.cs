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
        var authenticator = new BybitAuthenticator("test-api-key", "test-api-secret");

        var request = new HttpRequestMessage(HttpMethod.Get, "https://api.bybit.com/test");
        authenticator.SignRequest(request, null, 5000);

        request.Headers.Should().Contain(h => h.Key == "X-BAPI-API-KEY");
        request.Headers.Should().Contain(h => h.Key == "X-BAPI-SIGN");
        request.Headers.Should().Contain(h => h.Key == "X-BAPI-TIMESTAMP");
        request.Headers.Should().Contain(h => h.Key == "X-BAPI-RECV-WINDOW");
        request.Headers.GetValues("X-BAPI-API-KEY").First().Should().Be("test-api-key");
    }

    [Fact]
    public void BybitAuthenticator_ProducesConsistentSignatures()
    {
        var authenticator = new BybitAuthenticator("test-key", "test-secret");

        var request1 = new HttpRequestMessage(HttpMethod.Get, "https://api.bybit.com/test");
        var request2 = new HttpRequestMessage(HttpMethod.Get, "https://api.bybit.com/test");

        authenticator.SignRequest(request1, "test-payload", 5000);
        // Different timestamps will produce different signatures,
        // but the structure should be consistent
        authenticator.SignRequest(request2, "test-payload", 5000);

        request1.Headers.GetValues("X-BAPI-API-KEY").First().Should().Be(
            request2.Headers.GetValues("X-BAPI-API-KEY").First());
        request1.Headers.GetValues("X-BAPI-RECV-WINDOW").First().Should().Be(
            request2.Headers.GetValues("X-BAPI-RECV-WINDOW").First());
    }
}
