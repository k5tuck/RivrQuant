using System.Text.Json;
using RivrQuant.Domain.Enums;

namespace RivrQuant.Domain.Models.Risk;

/// <summary>
/// Represents a persisted record of a deleveraging event, capturing the transition
/// between deleverage levels and the actions taken in response to portfolio stress.
/// </summary>
public class DeleverageEvent
{
    /// <summary>
    /// Unique internal identifier for the deleverage event.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Timestamp when the deleverage event was triggered.
    /// </summary>
    public DateTimeOffset TriggeredAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// The deleverage level before this event was triggered.
    /// </summary>
    public DeleverageLevel PreviousLevel { get; init; }

    /// <summary>
    /// The new deleverage level after this event was triggered.
    /// </summary>
    public DeleverageLevel NewLevel { get; init; }

    /// <summary>
    /// Portfolio drawdown percentage at the time the event was triggered.
    /// </summary>
    public decimal DrawdownPercentAtTrigger { get; init; }

    /// <summary>
    /// Total portfolio value at the time the event was triggered.
    /// </summary>
    public decimal PortfolioValueAtTrigger { get; init; }

    /// <summary>
    /// Peak equity value from which the drawdown is measured.
    /// </summary>
    public decimal PeakEquity { get; init; }

    /// <summary>
    /// Human-readable description of the actions taken during this deleverage event
    /// (e.g., "Reduced all positions by 50%, paused MomentumAlpha strategy").
    /// </summary>
    public string ActionsTaken { get; init; } = string.Empty;

    /// <summary>
    /// The reason that triggered this deleverage event.
    /// </summary>
    public DeleverageReason Reason { get; init; }

    /// <summary>
    /// JSON-serialized list of strategy names that were paused as part of this deleverage event.
    /// Use <see cref="GetPausedStrategies"/> and <see cref="SetPausedStrategies"/> for typed access.
    /// </summary>
    public string? PausedStrategiesJson { get; set; }

    /// <summary>
    /// Deserializes the <see cref="PausedStrategiesJson"/> property into a typed list of strategy names.
    /// Returns an empty list if the JSON is null or empty.
    /// </summary>
    /// <returns>A list of paused strategy names.</returns>
    public IReadOnlyList<string> GetPausedStrategies()
    {
        if (string.IsNullOrWhiteSpace(PausedStrategiesJson))
            return Array.Empty<string>();

        return JsonSerializer.Deserialize<List<string>>(PausedStrategiesJson) ?? new List<string>();
    }

    /// <summary>
    /// Serializes the provided list of strategy names into the <see cref="PausedStrategiesJson"/> property.
    /// </summary>
    /// <param name="strategies">The list of strategy names to serialize. Pass null to clear.</param>
    public void SetPausedStrategies(IReadOnlyList<string>? strategies)
    {
        PausedStrategiesJson = strategies is null or { Count: 0 }
            ? null
            : JsonSerializer.Serialize(strategies);
    }
}
