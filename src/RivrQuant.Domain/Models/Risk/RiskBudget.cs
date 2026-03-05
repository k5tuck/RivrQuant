namespace RivrQuant.Domain.Models.Risk;

/// <summary>
/// Represents the risk budget allocation for a single strategy, tracking how much
/// of the total portfolio risk has been allocated, consumed, and remains available.
/// Persisted to the database for audit and risk monitoring.
/// </summary>
public class RiskBudget
{
    /// <summary>
    /// Unique internal identifier for the risk budget record.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Identifier of the strategy to which this risk budget is assigned.
    /// </summary>
    public Guid StrategyId { get; init; }

    /// <summary>
    /// Human-readable name of the strategy.
    /// </summary>
    public string StrategyName { get; init; } = string.Empty;

    /// <summary>
    /// Percentage of total portfolio risk allocated to this strategy (e.g., 25.0 for 25%).
    /// </summary>
    public decimal AllocatedRiskPercent { get; init; }

    /// <summary>
    /// Percentage of total portfolio risk currently consumed by this strategy's open positions.
    /// </summary>
    public decimal UsedRiskPercent { get; init; }

    /// <summary>
    /// Percentage of total portfolio risk remaining available for new positions in this strategy.
    /// Computed as AllocatedRiskPercent minus UsedRiskPercent.
    /// </summary>
    public decimal RemainingRiskPercent { get; init; }

    /// <summary>
    /// Timestamp when this risk budget was calculated.
    /// </summary>
    public DateTimeOffset CalculatedAt { get; init; } = DateTimeOffset.UtcNow;
}
