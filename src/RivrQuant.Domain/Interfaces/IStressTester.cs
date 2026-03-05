using RivrQuant.Domain.Models.Stress;
using RivrQuant.Domain.Models.Trading;

namespace RivrQuant.Domain.Interfaces;

/// <summary>
/// Provides portfolio stress testing capabilities, applying historical and synthetic
/// shock scenarios to evaluate portfolio resilience under adverse market conditions.
/// <para>
/// <strong>Tier 3 stub:</strong> This interface defines the contract for stress testing
/// functionality. Full implementation requires integration with a historical market data
/// provider for replay scenarios and calibrated shock models for synthetic scenarios.
/// Prerequisites include a functioning <see cref="IExposureTracker"/> for position-level
/// exposure data and an <see cref="IExecutionCostModel"/> for liquidity stress scenarios.
/// </para>
/// </summary>
public interface IStressTester
{
    /// <summary>
    /// Runs a historical replay stress test by applying actual market movements from a past
    /// crisis period to the current portfolio positions.
    /// </summary>
    /// <param name="scenario">The stress scenario definition specifying the historical period and parameters.</param>
    /// <param name="currentPortfolio">The current portfolio state to stress test.</param>
    /// <param name="ct">A cancellation token to cancel the asynchronous operation.</param>
    /// <returns>A <see cref="StressTestResult"/> containing the estimated portfolio impact.</returns>
    Task<StressTestResult> RunHistoricalScenarioAsync(
        StressScenario scenario,
        Portfolio currentPortfolio,
        CancellationToken ct);

    /// <summary>
    /// Runs a synthetic shock stress test by applying hypothetical market dislocations
    /// (equity drop, volatility spike, and spread widening) to the current portfolio.
    /// </summary>
    /// <param name="marketDropPercent">Hypothetical market decline as a percentage (e.g., -20.0 for 20% drop).</param>
    /// <param name="volMultiplier">Multiplier applied to current volatility levels.</param>
    /// <param name="spreadMultiplier">Multiplier applied to current bid-ask spreads.</param>
    /// <param name="currentPortfolio">The current portfolio state to stress test.</param>
    /// <param name="ct">A cancellation token to cancel the asynchronous operation.</param>
    /// <returns>A <see cref="StressTestResult"/> containing the estimated portfolio impact.</returns>
    Task<StressTestResult> RunSyntheticShockAsync(
        decimal marketDropPercent,
        decimal volMultiplier,
        decimal spreadMultiplier,
        Portfolio currentPortfolio,
        CancellationToken ct);

    /// <summary>
    /// Runs all predefined stress scenarios against the current portfolio and returns
    /// the results for each scenario.
    /// </summary>
    /// <param name="currentPortfolio">The current portfolio state to stress test.</param>
    /// <param name="ct">A cancellation token to cancel the asynchronous operation.</param>
    /// <returns>A read-only list of <see cref="StressTestResult"/> records, one per scenario.</returns>
    Task<IReadOnlyList<StressTestResult>> RunAllScenariosAsync(
        Portfolio currentPortfolio,
        CancellationToken ct);
}
