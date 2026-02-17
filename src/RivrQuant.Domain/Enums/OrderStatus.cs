namespace RivrQuant.Domain.Enums;

/// <summary>
/// Represents the lifecycle status of an order.
/// </summary>
public enum OrderStatus
{
    /// <summary>
    /// The order has been submitted and is awaiting execution.
    /// </summary>
    Pending,

    /// <summary>
    /// The order has been partially executed, with remaining quantity still open.
    /// </summary>
    PartiallyFilled,

    /// <summary>
    /// The order has been fully executed for the entire requested quantity.
    /// </summary>
    Filled,

    /// <summary>
    /// The order was cancelled before full execution, either by the user or the system.
    /// </summary>
    Cancelled,

    /// <summary>
    /// The order was rejected by the broker or exchange and was not executed.
    /// </summary>
    Rejected,

    /// <summary>
    /// The order expired before it could be executed, typically due to a time-in-force constraint.
    /// </summary>
    Expired
}
