using RivrQuant.Domain.Enums;

namespace RivrQuant.Domain.Models.Execution;

/// <summary>
/// Represents a pre-trade estimate of execution costs for a proposed order,
/// including slippage, spread, and commission components. This is a computed
/// value object used for decision-making and is not persisted directly.
/// </summary>
public sealed record ExecutionCostEstimate
{
    /// <summary>
    /// Ticker symbol of the instrument being traded.
    /// </summary>
    public required string Symbol { get; init; }

    /// <summary>
    /// Quantity of shares or units to be traded.
    /// </summary>
    public required decimal Quantity { get; init; }

    /// <summary>
    /// Current market price of the instrument at the time of estimation.
    /// </summary>
    public required decimal CurrentPrice { get; init; }

    /// <summary>
    /// Total notional value of the proposed trade (Quantity * CurrentPrice).
    /// </summary>
    public required decimal NotionalValue { get; init; }

    /// <summary>
    /// Estimated market impact slippage in basis points.
    /// </summary>
    public required decimal EstimatedSlippageBps { get; init; }

    /// <summary>
    /// Estimated market impact slippage in dollar terms.
    /// </summary>
    public required decimal EstimatedSlippageDollars { get; init; }

    /// <summary>
    /// Estimated bid-ask spread cost in basis points.
    /// </summary>
    public required decimal EstimatedSpreadCostBps { get; init; }

    /// <summary>
    /// Estimated bid-ask spread cost in dollar terms.
    /// </summary>
    public required decimal EstimatedSpreadCostDollars { get; init; }

    /// <summary>
    /// Estimated broker commission in dollar terms.
    /// </summary>
    public required decimal CommissionDollars { get; init; }

    /// <summary>
    /// Total estimated execution cost in dollar terms, combining slippage, spread, and commission.
    /// </summary>
    public required decimal TotalCostDollars { get; init; }

    /// <summary>
    /// Total estimated execution cost in basis points relative to notional value.
    /// </summary>
    public required decimal TotalCostBps { get; init; }

    /// <summary>
    /// Total estimated cost expressed as a percentage of the instrument's current price.
    /// </summary>
    public required decimal CostAsPercentOfPrice { get; init; }

    /// <summary>
    /// Broker through which the trade would be executed.
    /// </summary>
    public required BrokerType Broker { get; init; }

    /// <summary>
    /// Timestamp when this cost estimate was calculated.
    /// </summary>
    public required DateTimeOffset EstimatedAt { get; init; }
}
