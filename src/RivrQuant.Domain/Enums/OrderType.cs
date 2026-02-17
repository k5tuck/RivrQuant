namespace RivrQuant.Domain.Enums;

/// <summary>
/// Represents the type of an order, determining how and when it is executed.
/// </summary>
public enum OrderType
{
    /// <summary>
    /// An order to buy or sell immediately at the best available current price.
    /// </summary>
    Market,

    /// <summary>
    /// An order to buy or sell at a specified price or better.
    /// </summary>
    Limit,

    /// <summary>
    /// An order that becomes a market order once the stop price is reached,
    /// used to limit losses on a position.
    /// </summary>
    StopLoss,

    /// <summary>
    /// An order that becomes a limit order once the stop price is reached,
    /// combining stop-loss protection with limit-order price control.
    /// </summary>
    StopLimit,

    /// <summary>
    /// A stop order that adjusts automatically as the market price moves
    /// in a favorable direction, trailing by a specified amount or percentage.
    /// </summary>
    TrailingStop
}
