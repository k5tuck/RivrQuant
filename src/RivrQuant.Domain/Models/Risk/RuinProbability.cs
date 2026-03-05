namespace RivrQuant.Domain.Models.Risk;

/// <summary>
/// Represents the estimated probability of portfolio ruin based on Monte Carlo simulation,
/// including confidence intervals and simulation parameters. This is a computed value object
/// generated on demand for risk assessment purposes.
/// </summary>
public sealed record RuinProbability
{
    /// <summary>
    /// Estimated probability of ruin (portfolio declining below the ruin threshold),
    /// expressed as a value between 0.0 and 1.0.
    /// </summary>
    public required double Probability { get; init; }

    /// <summary>
    /// Median number of days to ruin across simulation paths that resulted in ruin.
    /// Null if no simulation paths reached the ruin threshold.
    /// </summary>
    public required double? MedianTimeToRuinDays { get; init; }

    /// <summary>
    /// Worst-case drawdown observed across all simulation paths, expressed as a decimal
    /// (e.g., -0.45 for a 45% drawdown).
    /// </summary>
    public required double WorstCaseDrawdown { get; init; }

    /// <summary>
    /// Lower bound of the 95% confidence interval for the ruin probability estimate.
    /// </summary>
    public required double ConfidenceInterval95Lower { get; init; }

    /// <summary>
    /// Upper bound of the 95% confidence interval for the ruin probability estimate.
    /// </summary>
    public required double ConfidenceInterval95Upper { get; init; }

    /// <summary>
    /// Total number of Monte Carlo simulation paths executed.
    /// </summary>
    public required int SimulationsRun { get; init; }

    /// <summary>
    /// Simulation horizon in calendar days.
    /// </summary>
    public required int HorizonDays { get; init; }

    /// <summary>
    /// The equity threshold below which a simulation path is considered to have reached ruin,
    /// expressed as a fraction of initial equity (e.g., 0.5 means ruin occurs at 50% of starting equity).
    /// </summary>
    public required double RuinThreshold { get; init; }

    /// <summary>
    /// Timestamp when this ruin probability estimate was calculated.
    /// </summary>
    public required DateTimeOffset CalculatedAt { get; init; }
}
