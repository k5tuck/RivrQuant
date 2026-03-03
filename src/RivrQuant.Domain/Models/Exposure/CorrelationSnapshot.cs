using System.Text.Json;

namespace RivrQuant.Domain.Models.Exposure;

/// <summary>
/// Represents a persisted snapshot of the pairwise correlation matrix for a set of portfolio symbols,
/// computed over a specified lookback period. Used for correlation-based risk monitoring,
/// diversification analysis, and deleverage triggers.
/// </summary>
public class CorrelationSnapshot
{
    /// <summary>
    /// Unique internal identifier for the correlation snapshot.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// JSON-serialized list of symbols included in the correlation matrix.
    /// Use <see cref="GetSymbols"/> and <see cref="SetSymbols"/> for typed access.
    /// </summary>
    public string SymbolsJson { get; set; } = "[]";

    /// <summary>
    /// JSON-serialized correlation matrix stored as a jagged array (double[][]).
    /// Row and column indices correspond to the symbols in <see cref="SymbolsJson"/>.
    /// Use <see cref="GetMatrix"/> and <see cref="SetMatrix"/> for typed access.
    /// </summary>
    public string MatrixJson { get; set; } = "[]";

    /// <summary>
    /// Number of calendar days of historical data used to compute the correlation matrix.
    /// </summary>
    public int LookbackDays { get; init; }

    /// <summary>
    /// Timestamp when this correlation snapshot was computed.
    /// </summary>
    public DateTimeOffset SnapshotAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Deserializes the <see cref="SymbolsJson"/> property into a typed list of symbol strings.
    /// Returns an empty list if the JSON is null or empty.
    /// </summary>
    /// <returns>A list of symbol strings.</returns>
    public List<string> GetSymbols()
    {
        if (string.IsNullOrWhiteSpace(SymbolsJson))
            return new List<string>();

        return JsonSerializer.Deserialize<List<string>>(SymbolsJson) ?? new List<string>();
    }

    /// <summary>
    /// Serializes the provided list of symbols into the <see cref="SymbolsJson"/> property.
    /// </summary>
    /// <param name="symbols">The list of symbol strings to serialize.</param>
    public void SetSymbols(List<string> symbols)
    {
        SymbolsJson = JsonSerializer.Serialize(symbols);
    }

    /// <summary>
    /// Deserializes the <see cref="MatrixJson"/> property into a jagged array of doubles
    /// representing the correlation matrix. Returns an empty array if the JSON is null or empty.
    /// </summary>
    /// <returns>A jagged array representing the correlation matrix.</returns>
    public double[][] GetMatrix()
    {
        if (string.IsNullOrWhiteSpace(MatrixJson))
            return Array.Empty<double[]>();

        return JsonSerializer.Deserialize<double[][]>(MatrixJson) ?? Array.Empty<double[]>();
    }

    /// <summary>
    /// Serializes the provided correlation matrix into the <see cref="MatrixJson"/> property.
    /// </summary>
    /// <param name="matrix">The jagged array representing the correlation matrix to serialize.</param>
    public void SetMatrix(double[][] matrix)
    {
        MatrixJson = JsonSerializer.Serialize(matrix);
    }
}
