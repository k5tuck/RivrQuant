using RivrQuant.Domain.Enums;
using RivrQuant.Domain.Models.Execution;
using RivrQuant.Domain.Models.Trading;

namespace RivrQuant.Domain.Interfaces;

/// <summary>
/// Provides post-trade fill analysis and execution quality reporting capabilities.
/// Implementations compare actual fill results against pre-trade cost estimates to
/// measure model accuracy and detect execution quality degradation.
/// </summary>
public interface IFillAnalyzer
{
    /// <summary>
    /// Analyzes a single fill by comparing actual execution against the pre-trade cost estimate,
    /// computing slippage deviation, cost deviation, and fill latency metrics.
    /// </summary>
    /// <param name="order">The order that was filled.</param>
    /// <param name="fill">The fill execution to analyze.</param>
    /// <param name="preTradeEstimate">The pre-trade cost estimate that was produced before order submission.</param>
    /// <param name="ct">A cancellation token to cancel the asynchronous operation.</param>
    /// <returns>A <see cref="FillAnalysis"/> containing the detailed comparison results.</returns>
    Task<FillAnalysis> AnalyzeFillAsync(
        Order order,
        Fill fill,
        ExecutionCostEstimate preTradeEstimate,
        CancellationToken ct);

    /// <summary>
    /// Generates an aggregate execution quality report across all fills within the
    /// specified time period, optionally filtered by broker.
    /// </summary>
    /// <param name="from">Inclusive start of the reporting period.</param>
    /// <param name="to">Inclusive end of the reporting period.</param>
    /// <param name="brokerFilter">Optional broker filter. Null includes all brokers.</param>
    /// <param name="ct">A cancellation token to cancel the asynchronous operation.</param>
    /// <returns>An <see cref="ExecutionReport"/> summarizing execution quality statistics.</returns>
    Task<ExecutionReport> GenerateReportAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        BrokerType? brokerFilter,
        CancellationToken ct);
}
