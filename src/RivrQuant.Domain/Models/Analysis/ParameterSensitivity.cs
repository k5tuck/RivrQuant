namespace RivrQuant.Domain.Models.Analysis;

/// <summary>Results of parameter sensitivity analysis across backtest runs.</summary>
public class ParameterSensitivity
{
    /// <summary>Unique identifier.</summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>Name of the parameter being analyzed.</summary>
    public string ParameterName { get; init; } = string.Empty;

    /// <summary>Data points showing metric values at different parameter settings.</summary>
    public IReadOnlyList<ParameterPoint> Points { get; init; } = Array.Empty<ParameterPoint>();

    /// <summary>Correlation between this parameter and Sharpe ratio across runs.</summary>
    public double CorrelationWithSharpe { get; init; }

    /// <summary>Correlation between this parameter and total return across runs.</summary>
    public double CorrelationWithReturn { get; init; }

    /// <summary>Optimal value of this parameter based on Sharpe maximization.</summary>
    public double OptimalValue { get; init; }

    /// <summary>When this analysis was performed.</summary>
    public DateTimeOffset CalculatedAt { get; init; } = DateTimeOffset.UtcNow;
}

/// <summary>A single data point in a parameter sensitivity analysis.</summary>
public class ParameterPoint
{
    /// <summary>The parameter value used in this backtest run.</summary>
    public decimal ParameterValue { get; init; }

    /// <summary>Sharpe ratio achieved at this parameter value.</summary>
    public double SharpeRatio { get; init; }

    /// <summary>Total return achieved at this parameter value.</summary>
    public double TotalReturn { get; init; }

    /// <summary>Maximum drawdown at this parameter value.</summary>
    public double MaxDrawdown { get; init; }
}
