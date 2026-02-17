// Copyright (c) RivrQuant. All rights reserved.
// Licensed under the MIT License.

using RivrQuant.Domain.Enums;

namespace RivrQuant.Domain.Models.Analysis;

/// <summary>
/// Represents a classified market regime detected over a contiguous date range,
/// including per-regime performance and risk statistics.
/// </summary>
public class RegimeClassification
{
    /// <summary>
    /// Unique internal identifier for this regime classification record.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Foreign key linking this regime classification to its parent backtest result.
    /// </summary>
    public Guid BacktestResultId { get; init; }

    /// <summary>
    /// The detected market regime type for this period.
    /// </summary>
    public RegimeType Regime { get; init; }

    /// <summary>
    /// Start date of this regime period (inclusive).
    /// </summary>
    public DateTimeOffset StartDate { get; init; }

    /// <summary>
    /// End date of this regime period (inclusive).
    /// </summary>
    public DateTimeOffset EndDate { get; init; }

    /// <summary>
    /// Number of trading days in this regime period.
    /// </summary>
    public int DurationDays { get; init; }

    /// <summary>
    /// Annualized return achieved during this regime period.
    /// </summary>
    public double AnnualizedReturn { get; init; }

    /// <summary>
    /// Annualized Sharpe ratio during this regime period.
    /// </summary>
    public double SharpeRatio { get; init; }

    /// <summary>
    /// Maximum drawdown experienced during this regime period,
    /// expressed as a positive decimal (e.g., 0.10 = 10%).
    /// </summary>
    public double MaxDrawdown { get; init; }

    /// <summary>
    /// Average annualized volatility (standard deviation of daily returns, annualized)
    /// observed during this regime period.
    /// </summary>
    public double Volatility { get; init; }

    /// <summary>
    /// Total number of trades executed during this regime period.
    /// </summary>
    public int TradeCount { get; init; }

    /// <summary>
    /// Win rate of trades executed during this regime period, expressed as a decimal.
    /// </summary>
    public double WinRate { get; init; }

    /// <summary>
    /// Timestamp when this regime classification was computed.
    /// </summary>
    public DateTimeOffset ClassifiedAt { get; init; } = DateTimeOffset.UtcNow;
}
