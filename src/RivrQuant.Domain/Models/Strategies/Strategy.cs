using RivrQuant.Domain.Enums;

namespace RivrQuant.Domain.Models.Strategies;

/// <summary>
/// Represents a trading strategy definition, including its configuration parameters
/// and version history for tracking changes over time.
/// </summary>
public class Strategy
{
    /// <summary>
    /// Unique internal identifier for the strategy.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Human-readable name of the strategy.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Optional description explaining the strategy's approach and logic.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Asset class this strategy is designed to trade (e.g., Stock, Crypto).
    /// </summary>
    public AssetClass AssetClass { get; init; }

    /// <summary>
    /// Broker through which this strategy's live orders are routed.
    /// Defaults to Alpaca (stocks/ETFs). Set to Bybit for crypto strategies.
    /// </summary>
    public BrokerType Broker { get; init; } = BrokerType.Alpaca;

    /// <summary>
    /// Indicates whether the strategy is currently active and eligible for execution.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Timestamp when the strategy was originally created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Timestamp of the most recent modification to the strategy. Null if never modified.
    /// </summary>
    public DateTimeOffset? LastModifiedAt { get; set; }

    /// <summary>
    /// Collection of configurable parameters that control the strategy's behavior.
    /// </summary>
    public ICollection<StrategyParameter> Parameters { get; init; } = new List<StrategyParameter>();

    /// <summary>
    /// Collection of version snapshots tracking the strategy's evolution over time.
    /// </summary>
    public ICollection<StrategyVersion> Versions { get; init; } = new List<StrategyVersion>();
}
