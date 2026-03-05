using MathNet.Numerics.Statistics;
using RivrQuant.Domain.Models.Execution;

namespace RivrQuant.Infrastructure.Execution;

/// <summary>
/// Builds an <see cref="ExecutionReport"/> from a collection of <see cref="FillAnalysis"/>
/// records, computing aggregate statistics such as mean deviation, standard deviation,
/// and identification of best/worst fills.
/// </summary>
public sealed class ExecutionReportGenerator
{
    /// <summary>
    /// Builds an execution quality report from the provided fill analyses.
    /// </summary>
    /// <param name="analyses">The fill analysis records to aggregate.</param>
    /// <param name="from">The start of the reporting period.</param>
    /// <param name="to">The end of the reporting period.</param>
    /// <returns>An <see cref="ExecutionReport"/> with aggregate statistics.</returns>
    public ExecutionReport BuildReport(
        IReadOnlyList<FillAnalysis> analyses,
        DateTimeOffset from,
        DateTimeOffset to)
    {
        if (analyses.Count == 0)
        {
            return new ExecutionReport
            {
                PeriodStart = from,
                PeriodEnd = to,
                TotalFills = 0,
                MeanDeviationBps = 0m,
                StdDevDeviationBps = 0m,
                MeanActualSlippageBps = 0m,
                MeanEstimatedSlippageBps = 0m,
                TotalCostDifferenceDollars = 0m,
                BestFillSymbol = string.Empty,
                BestFillDeviationBps = 0m,
                WorstFillSymbol = string.Empty,
                WorstFillDeviationBps = 0m,
                FillsWithinEstimate = 0,
                FillsExceedingEstimate = 0,
                GeneratedAt = DateTimeOffset.UtcNow
            };
        }

        var deviations = analyses.Select(a => (double)a.DeviationBps).ToArray();
        var meanDeviation = (decimal)deviations.Mean();
        var stdDevDeviation = deviations.Length > 1
            ? (decimal)deviations.StandardDeviation()
            : 0m;

        var meanActualSlippage = analyses.Average(a => a.ActualSlippageBps);
        var meanEstimatedSlippage = analyses.Average(a => a.EstimatedSlippageBps);
        var totalCostDifference = analyses.Sum(a => a.CostDifferenceDollars);

        // Best fill = lowest deviation (negative means better than expected)
        var bestFill = analyses.OrderBy(a => a.DeviationBps).First();

        // Worst fill = highest deviation (positive means worse than expected)
        var worstFill = analyses.OrderByDescending(a => a.DeviationBps).First();

        // Fills where actual slippage was within or below the estimate
        var withinEstimate = analyses.Count(a => a.DeviationBps <= 0m);
        var exceedingEstimate = analyses.Count(a => a.DeviationBps > 0m);

        return new ExecutionReport
        {
            PeriodStart = from,
            PeriodEnd = to,
            TotalFills = analyses.Count,
            MeanDeviationBps = Math.Round(meanDeviation, 4),
            StdDevDeviationBps = Math.Round(stdDevDeviation, 4),
            MeanActualSlippageBps = Math.Round(meanActualSlippage, 4),
            MeanEstimatedSlippageBps = Math.Round(meanEstimatedSlippage, 4),
            TotalCostDifferenceDollars = Math.Round(totalCostDifference, 2),
            BestFillSymbol = bestFill.Symbol,
            BestFillDeviationBps = bestFill.DeviationBps,
            WorstFillSymbol = worstFill.Symbol,
            WorstFillDeviationBps = worstFill.DeviationBps,
            FillsWithinEstimate = withinEstimate,
            FillsExceedingEstimate = exceedingEstimate,
            GeneratedAt = DateTimeOffset.UtcNow
        };
    }
}
