namespace RivrQuant.Domain.Models.Strategies;

/// <summary>
/// Represents a versioned snapshot of a strategy's configuration at a point in time,
/// enabling audit trails and rollback capabilities.
/// </summary>
public class StrategyVersion
{
    /// <summary>
    /// Unique internal identifier for the version record.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Foreign key linking this version to its parent strategy.
    /// </summary>
    public Guid StrategyId { get; init; }

    /// <summary>
    /// Sequential version number, starting from 1 and incrementing with each change.
    /// </summary>
    public int VersionNumber { get; init; }

    /// <summary>
    /// Optional notes describing the changes made in this version.
    /// </summary>
    public string? ChangeNotes { get; init; }

    /// <summary>
    /// JSON-serialized snapshot of all strategy parameters at the time this version was created.
    /// </summary>
    public string ParametersSnapshot { get; init; } = string.Empty;

    /// <summary>
    /// Timestamp when this version was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}
