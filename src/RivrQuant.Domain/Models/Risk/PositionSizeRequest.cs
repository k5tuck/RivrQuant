using RivrQuant.Domain.Enums;
using RivrQuant.Domain.Models.Execution;
using RivrQuant.Domain.Models.Trading;

namespace RivrQuant.Domain.Models.Risk;

/// <summary>
/// Represents a request to calculate the recommended position size for a trade,
/// bundling the signal information with current portfolio state, risk parameters,
/// and cost estimates needed by the position sizer.
/// </summary>
public sealed record PositionSizeRequest
{
    /// <summary>
    /// Ticker symbol of the instrument to be traded.
    /// </summary>
    public required string Symbol { get; init; }

    /// <summary>
    /// Direction of the proposed trade (Buy or Sell).
    /// </summary>
    public required OrderSide Side { get; init; }

    /// <summary>
    /// Current market price of the instrument.
    /// </summary>
    public required decimal CurrentPrice { get; init; }

    /// <summary>
    /// Confidence level of the trading signal, ranging from 0 (no confidence) to 1 (maximum confidence).
    /// Used by some sizing methods to scale position size proportionally to signal strength.
    /// </summary>
    public required decimal SignalConfidence { get; init; }

    /// <summary>
    /// Current portfolio state including equity, cash balance, and buying power.
    /// </summary>
    public required Portfolio CurrentPortfolio { get; init; }

    /// <summary>
    /// Current drawdown state of the portfolio, used to apply drawdown-based position size reductions.
    /// </summary>
    public required DrawdownState CurrentDrawdownState { get; init; }

    /// <summary>
    /// Current volatility target snapshot, used to apply volatility-based position size adjustments.
    /// </summary>
    public required VolatilityTarget CurrentVolTarget { get; init; }

    /// <summary>
    /// Pre-trade execution cost estimate, used to factor transaction costs into sizing decisions.
    /// </summary>
    public required ExecutionCostEstimate EstimatedCosts { get; init; }

    /// <summary>
    /// Historical win rate for this symbol or strategy, expressed as a decimal (e.g., 0.55 for 55%).
    /// Required for Kelly criterion sizing. Null if historical data is unavailable.
    /// </summary>
    public decimal? HistoricalWinRate { get; init; }

    /// <summary>
    /// Historical average ratio of winning trade size to losing trade size.
    /// Required for Kelly criterion sizing. Null if historical data is unavailable.
    /// </summary>
    public decimal? HistoricalAvgWinLossRatio { get; init; }

    /// <summary>
    /// Annualized volatility of the individual asset, expressed as a decimal.
    /// Required for volatility-target and risk-parity sizing. Null if historical data is unavailable.
    /// </summary>
    public decimal? AssetAnnualizedVol { get; init; }
}
