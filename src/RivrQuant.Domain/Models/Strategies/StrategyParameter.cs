namespace RivrQuant.Domain.Models.Strategies;

/// <summary>
/// Represents a single configurable parameter of a trading strategy,
/// including its current value and valid range for optimization.
/// </summary>
public class StrategyParameter
{
    /// <summary>
    /// Unique internal identifier for the parameter.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Foreign key linking this parameter to its parent strategy.
    /// </summary>
    public Guid StrategyId { get; init; }

    /// <summary>
    /// Name of the parameter (e.g., "LookbackPeriod", "StopLossPercent").
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Optional description explaining what this parameter controls.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Current value of the parameter.
    /// </summary>
    public decimal Value { get; set; }

    /// <summary>
    /// Minimum allowed value for this parameter during optimization.
    /// </summary>
    public decimal MinValue { get; init; }

    /// <summary>
    /// Maximum allowed value for this parameter during optimization.
    /// </summary>
    public decimal MaxValue { get; init; }

    /// <summary>
    /// Step size for parameter sweeps during optimization (e.g., 0.01 for fine granularity).
    /// </summary>
    public decimal StepSize { get; init; }
}
