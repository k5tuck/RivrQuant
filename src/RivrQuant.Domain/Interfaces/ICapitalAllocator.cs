using RivrQuant.Domain.Models.Allocation;
using RivrQuant.Domain.Models.Strategies;

namespace RivrQuant.Domain.Interfaces;

/// <summary>
/// Allocates portfolio capital across active strategies and evaluates whether rebalancing
/// is needed based on drift thresholds. Implementations may use equal-weight, risk-parity,
/// momentum-based ranking, or other allocation methodologies.
/// </summary>
public interface ICapitalAllocator
{
    /// <summary>
    /// Computes the capital allocation for each active strategy given the total available capital.
    /// </summary>
    /// <param name="totalCapital">Total portfolio capital available for allocation.</param>
    /// <param name="activeStrategies">The set of currently active strategies eligible for capital allocation.</param>
    /// <param name="ct">A cancellation token to cancel the asynchronous operation.</param>
    /// <returns>A read-only list of <see cref="StrategyAllocation"/> records, one per strategy.</returns>
    Task<IReadOnlyList<StrategyAllocation>> AllocateAsync(
        decimal totalCapital,
        IReadOnlyList<Strategy> activeStrategies,
        CancellationToken ct);

    /// <summary>
    /// Evaluates the current allocation drift across all strategies and determines
    /// whether a rebalance is recommended.
    /// </summary>
    /// <param name="ct">A cancellation token to cancel the asynchronous operation.</param>
    /// <returns>An <see cref="AllocationDecision"/> indicating whether rebalancing is needed and the current drift details.</returns>
    Task<AllocationDecision> EvaluateRebalanceAsync(CancellationToken ct);
}
