using RivrQuant.Domain.Enums;

namespace RivrQuant.Domain.Models.Execution;

/// <summary>
/// Represents a persisted record of slippage observed on a single fill,
/// used for historical tracking, model calibration, and execution quality reporting.
/// </summary>
public class SlippageRecord
{
    /// <summary>
    /// Unique internal identifier for the slippage record.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Identifier of the order that generated this slippage observation.
    /// </summary>
    public string OrderId { get; init; } = string.Empty;

    /// <summary>
    /// Ticker symbol of the instrument that was traded.
    /// </summary>
    public string Symbol { get; init; } = string.Empty;

    /// <summary>
    /// Broker through which the order was executed.
    /// </summary>
    public BrokerType Broker { get; init; }

    /// <summary>
    /// Pre-trade estimated slippage in basis points from the cost model.
    /// </summary>
    public decimal ExpectedSlippageBps { get; init; }

    /// <summary>
    /// Actual slippage observed upon fill, measured in basis points.
    /// </summary>
    public decimal ActualSlippageBps { get; init; }

    /// <summary>
    /// Deviation between actual and expected slippage in basis points.
    /// Positive values indicate the model underestimated slippage.
    /// </summary>
    public decimal DeviationBps { get; init; }

    /// <summary>
    /// Notional value of the trade at the time of execution.
    /// </summary>
    public decimal NotionalValue { get; init; }

    /// <summary>
    /// Timestamp when this slippage record was captured.
    /// </summary>
    public DateTimeOffset RecordedAt { get; init; } = DateTimeOffset.UtcNow;
}
