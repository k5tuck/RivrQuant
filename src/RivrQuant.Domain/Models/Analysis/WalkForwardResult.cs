// Copyright (c) RivrQuant. All rights reserved.
// Licensed under the MIT License.

namespace RivrQuant.Domain.Models.Analysis;

/// <summary>
/// Represents the result of a single walk-forward validation window,
/// containing in-sample and out-of-sample performance metrics for
/// assessing strategy robustness and overfitting risk.
/// </summary>
public class WalkForwardResult
{
    /// <summary>
    /// Unique internal identifier for this walk-forward result record.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Foreign key linking this walk-forward result to its parent backtest result.
    /// </summary>
    public Guid BacktestResultId { get; init; }

    /// <summary>
    /// Zero-based index of this window within the walk-forward sequence.
    /// </summary>
    public int WindowIndex { get; init; }

    /// <summary>
    /// Start date of the in-sample (training) period.
    /// </summary>
    public DateTimeOffset InSampleStart { get; init; }

    /// <summary>
    /// End date of the in-sample (training) period.
    /// </summary>
    public DateTimeOffset InSampleEnd { get; init; }

    /// <summary>
    /// Start date of the out-of-sample (validation) period.
    /// </summary>
    public DateTimeOffset OutOfSampleStart { get; init; }

    /// <summary>
    /// End date of the out-of-sample (validation) period.
    /// </summary>
    public DateTimeOffset OutOfSampleEnd { get; init; }

    /// <summary>
    /// Annualized Sharpe ratio computed over the in-sample period.
    /// </summary>
    public double InSampleSharpe { get; init; }

    /// <summary>
    /// Annualized Sharpe ratio computed over the out-of-sample period.
    /// </summary>
    public double OutOfSampleSharpe { get; init; }

    /// <summary>
    /// Annualized return computed over the in-sample period.
    /// </summary>
    public double InSampleReturn { get; init; }

    /// <summary>
    /// Annualized return computed over the out-of-sample period.
    /// </summary>
    public double OutOfSampleReturn { get; init; }

    /// <summary>
    /// Ratio of out-of-sample Sharpe to in-sample Sharpe.
    /// Values near 1.0 suggest the strategy is robust; values much
    /// below 1.0 suggest overfitting to in-sample data.
    /// Returns <c>0.0</c> when the in-sample Sharpe is zero.
    /// </summary>
    public double Efficiency { get; init; }

    /// <summary>
    /// Number of trading days in the in-sample period.
    /// </summary>
    public int InSampleDays { get; init; }

    /// <summary>
    /// Number of trading days in the out-of-sample period.
    /// </summary>
    public int OutOfSampleDays { get; init; }

    /// <summary>
    /// Timestamp when this walk-forward window was analyzed.
    /// </summary>
    public DateTimeOffset AnalyzedAt { get; init; } = DateTimeOffset.UtcNow;
}
