namespace RivrQuant.Domain.Models.Backtests;

/// <summary>
/// Contains comprehensive performance and risk metrics calculated from a backtest result,
/// including risk-adjusted returns, drawdown statistics, and trade-level analytics.
/// </summary>
public class BacktestMetrics
{
    /// <summary>
    /// Unique internal identifier for the metrics record.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Foreign key linking these metrics to the parent backtest result.
    /// </summary>
    public Guid BacktestResultId { get; init; }

    /// <summary>
    /// Sharpe ratio measuring risk-adjusted return relative to the risk-free rate.
    /// Higher values indicate better risk-adjusted performance.
    /// </summary>
    public double SharpeRatio { get; init; }

    /// <summary>
    /// Sortino ratio measuring downside risk-adjusted return, penalizing only negative volatility.
    /// Preferred over Sharpe when return distributions are asymmetric.
    /// </summary>
    public double SortinoRatio { get; init; }

    /// <summary>
    /// Maximum peak-to-trough decline in portfolio value, expressed as a decimal (e.g., -0.25 for 25% drawdown).
    /// </summary>
    public double MaxDrawdown { get; init; }

    /// <summary>
    /// Percentage of trades that were profitable, expressed as a decimal (e.g., 0.55 for 55%).
    /// </summary>
    public double WinRate { get; init; }

    /// <summary>
    /// Percentage of trades that were unprofitable, computed as one minus the win rate.
    /// </summary>
    public double LossRate => 1.0 - WinRate;

    /// <summary>
    /// Ratio of gross profits to gross losses. Values above 1.0 indicate overall profitability.
    /// </summary>
    public double ProfitFactor { get; init; }

    /// <summary>
    /// Calmar ratio measuring annualized return divided by maximum drawdown.
    /// Higher values indicate better return per unit of drawdown risk.
    /// </summary>
    public double CalmarRatio { get; init; }

    /// <summary>
    /// Value at Risk at the 95% confidence level, representing the maximum expected loss
    /// that will not be exceeded 95% of the time over a single period.
    /// </summary>
    public double ValueAtRisk95 { get; init; }

    /// <summary>
    /// Expected Shortfall (Conditional VaR) at the 95% confidence level, representing
    /// the average loss in the worst 5% of scenarios.
    /// </summary>
    public double ExpectedShortfall95 { get; init; }

    /// <summary>
    /// Beta coefficient measuring the strategy's sensitivity to benchmark market movements.
    /// A beta of 1.0 indicates movement in line with the benchmark.
    /// </summary>
    public double Beta { get; init; }

    /// <summary>
    /// Annualized return of the strategy, extrapolated from the backtest period.
    /// </summary>
    public double AnnualizedReturn { get; init; }

    /// <summary>
    /// Annualized volatility (standard deviation of returns) of the strategy.
    /// </summary>
    public double AnnualizedVolatility { get; init; }

    /// <summary>
    /// Total number of round-trip trades executed during the backtest.
    /// </summary>
    public int TotalTrades { get; init; }

    /// <summary>
    /// Number of trades that closed with a profit.
    /// </summary>
    public int WinningTrades { get; init; }

    /// <summary>
    /// Number of trades that closed with a loss.
    /// </summary>
    public int LosingTrades { get; init; }

    /// <summary>
    /// Average profit amount across all winning trades.
    /// </summary>
    public decimal AverageWin { get; init; }

    /// <summary>
    /// Average loss amount across all losing trades.
    /// </summary>
    public decimal AverageLoss { get; init; }

    /// <summary>
    /// Largest single-trade profit recorded during the backtest.
    /// </summary>
    public decimal LargestWin { get; init; }

    /// <summary>
    /// Largest single-trade loss recorded during the backtest.
    /// </summary>
    public decimal LargestLoss { get; init; }

    /// <summary>
    /// Average duration that positions were held across all trades.
    /// </summary>
    public TimeSpan AverageHoldingPeriod { get; init; }

    /// <summary>
    /// Timestamp when these metrics were calculated.
    /// </summary>
    public DateTimeOffset CalculatedAt { get; init; } = DateTimeOffset.UtcNow;
}
