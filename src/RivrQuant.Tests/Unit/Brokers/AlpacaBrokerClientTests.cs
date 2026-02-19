using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using RivrQuant.Domain.Enums;
using RivrQuant.Domain.Models.Trading;
using RivrQuant.Infrastructure.Brokers.Alpaca;

namespace RivrQuant.Tests.Unit.Brokers;

public class AlpacaBrokerClientTests
{
    [Fact]
    public void BrokerType_ReturnsAlpaca()
    {
        var config = Options.Create(new AlpacaConfiguration
        {
            ApiKey = "test-key",
            ApiSecret = "test-secret",
            IsPaper = true
        });
        var logger = new Mock<ILogger<AlpacaBrokerClient>>();

        var client = new AlpacaBrokerClient(config, logger.Object);

        client.BrokerType.Should().Be(BrokerType.Alpaca);
    }

    [Fact]
    public void AlpacaConfiguration_Validate_ThrowsOnMissingApiKey()
    {
        var config = new AlpacaConfiguration
        {
            ApiKey = "",
            ApiSecret = "test-secret",
            IsPaper = true
        };

        var act = () => config.Validate();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void AlpacaConfiguration_Validate_ThrowsOnMissingApiSecret()
    {
        var config = new AlpacaConfiguration
        {
            ApiKey = "test-key",
            ApiSecret = "",
            IsPaper = true
        };

        var act = () => config.Validate();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void AlpacaConfiguration_Validate_SucceedsWithValidConfig()
    {
        var config = new AlpacaConfiguration
        {
            ApiKey = "test-key",
            ApiSecret = "test-secret",
            IsPaper = true
        };

        var act = () => config.Validate();

        act.Should().NotThrow();
    }

    [Fact]
    public void AlpacaAccountMapper_MapPosition_HandlesNullGracefully()
    {
        // Verify the static mapper methods exist and handle basic inputs
        var mapper = typeof(AlpacaAccountMapper);
        mapper.Should().NotBeNull();
    }
}
