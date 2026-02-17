namespace RivrQuant.Domain.Models.Trading;

/// <summary>
/// Represents a single fill (partial or complete execution) of an order,
/// recording the price, quantity, and commission for the execution.
/// </summary>
public class Fill
{
    /// <summary>
    /// Unique internal identifier for the fill.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Foreign key linking this fill to its parent order.
    /// </summary>
    public Guid OrderId { get; init; }

    /// <summary>
    /// External identifier assigned by the broker or exchange for this fill event.
    /// </summary>
    public string ExternalFillId { get; init; } = string.Empty;

    /// <summary>
    /// Execution price at which this fill occurred.
    /// </summary>
    public decimal Price { get; init; }

    /// <summary>
    /// Quantity executed in this fill.
    /// </summary>
    public decimal Quantity { get; init; }

    /// <summary>
    /// Commission or fee charged by the broker for this fill.
    /// </summary>
    public decimal Commission { get; init; }

    /// <summary>
    /// Timestamp when this fill was executed.
    /// </summary>
    public DateTimeOffset FilledAt { get; init; }
}
