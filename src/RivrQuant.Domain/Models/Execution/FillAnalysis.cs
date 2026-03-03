using RivrQuant.Domain.Enums;

namespace RivrQuant.Domain.Models.Execution;

/// <summary>
/// Represents a post-trade analysis of a fill execution, comparing actual execution
/// quality against pre-trade cost estimates. Persisted to the database for
/// ongoing execution quality monitoring and model calibration.
/// </summary>
public class FillAnalysis
{
    /// <summary>
    /// Unique internal identifier for the fill analysis record.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Identifier of the order that was filled.
    /// </summary>
    public string OrderId { get; init; } = string.Empty;

    /// <summary>
    /// Ticker symbol of the instrument that was traded.
    /// </summary>
    public string Symbol { get; init; } = string.Empty;

    /// <summary>
    /// Expected fill price based on the pre-trade cost model estimate.
    /// </summary>
    public decimal ExpectedFillPrice { get; init; }

    /// <summary>
    /// Actual fill price achieved in the market.
    /// </summary>
    public decimal ActualFillPrice { get; init; }

    /// <summary>
    /// Actual slippage incurred, measured in basis points relative to the expected fill price.
    /// </summary>
    public decimal SlippageBps { get; init; }

    /// <summary>
    /// Pre-trade estimated slippage in basis points from the cost model.
    /// </summary>
    public decimal EstimatedSlippageBps { get; init; }

    /// <summary>
    /// Deviation between actual and estimated slippage in basis points.
    /// Positive values indicate the model underestimated slippage.
    /// </summary>
    public decimal SlippageDeviationBps { get; init; }

    /// <summary>
    /// Actual total execution cost in dollar terms, including slippage, spread, and commission.
    /// </summary>
    public decimal ActualTotalCostDollars { get; init; }

    /// <summary>
    /// Pre-trade estimated total execution cost in dollar terms from the cost model.
    /// </summary>
    public decimal EstimatedTotalCostDollars { get; init; }

    /// <summary>
    /// Deviation between actual and estimated total cost in dollar terms.
    /// Positive values indicate the model underestimated costs.
    /// </summary>
    public decimal CostDeviationDollars { get; init; }

    /// <summary>
    /// Elapsed time from order submission to fill execution.
    /// </summary>
    public TimeSpan OrderToFillLatency { get; init; }

    /// <summary>
    /// Broker through which the order was executed.
    /// </summary>
    public BrokerType Broker { get; init; }

    /// <summary>
    /// Timestamp when this fill analysis was performed.
    /// </summary>
    public DateTimeOffset AnalyzedAt { get; init; } = DateTimeOffset.UtcNow;
}
