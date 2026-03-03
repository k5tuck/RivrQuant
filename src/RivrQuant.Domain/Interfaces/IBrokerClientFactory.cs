// Copyright (c) RivrQuant. All rights reserved.
// Licensed under the MIT License.

using RivrQuant.Domain.Enums;

namespace RivrQuant.Domain.Interfaces;

/// <summary>
/// Resolves the correct <see cref="IBrokerClient"/> implementation for a given
/// <see cref="BrokerType"/>. Decouples consumers from concrete broker types and
/// enables per-strategy broker routing.
/// </summary>
public interface IBrokerClientFactory
{
    /// <summary>
    /// Returns the <see cref="IBrokerClient"/> registered for the specified broker.
    /// </summary>
    /// <param name="brokerType">The target broker.</param>
    /// <returns>The matching <see cref="IBrokerClient"/> implementation.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="brokerType"/> has no registered client.</exception>
    IBrokerClient GetClient(BrokerType brokerType);
}
