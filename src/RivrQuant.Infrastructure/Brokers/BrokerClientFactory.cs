namespace RivrQuant.Infrastructure.Brokers;

using RivrQuant.Domain.Enums;
using RivrQuant.Domain.Interfaces;
using RivrQuant.Infrastructure.Brokers.Alpaca;
using RivrQuant.Infrastructure.Brokers.Bybit;

/// <summary>
/// Resolves the correct <see cref="IBrokerClient"/> for a given <see cref="BrokerType"/>.
/// Both broker clients are injected at construction so they share the same DI lifetime
/// (scoped), avoiding duplicate instances within a single request.
/// </summary>
public sealed class BrokerClientFactory : IBrokerClientFactory
{
    private readonly AlpacaBrokerClient _alpaca;
    private readonly BybitBrokerClient _bybit;

    /// <summary>Initializes a new instance of <see cref="BrokerClientFactory"/>.</summary>
    public BrokerClientFactory(AlpacaBrokerClient alpaca, BybitBrokerClient bybit)
    {
        _alpaca = alpaca;
        _bybit = bybit;
    }

    /// <inheritdoc />
    public IBrokerClient GetClient(BrokerType brokerType) => brokerType switch
    {
        BrokerType.Alpaca => _alpaca,
        BrokerType.Bybit  => _bybit,
        _ => throw new ArgumentOutOfRangeException(nameof(brokerType), $"No broker client registered for {brokerType}.")
    };
}
