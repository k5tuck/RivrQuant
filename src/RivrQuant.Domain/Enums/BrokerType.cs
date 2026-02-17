namespace RivrQuant.Domain.Enums;

/// <summary>
/// Represents the supported broker or exchange integration.
/// </summary>
public enum BrokerType
{
    /// <summary>
    /// Alpaca Markets, providing commission-free stock and crypto trading via API.
    /// </summary>
    Alpaca,

    /// <summary>
    /// Bybit exchange, providing cryptocurrency derivatives and spot trading via API.
    /// </summary>
    Bybit
}
