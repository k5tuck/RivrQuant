using Microsoft.AspNetCore.Mvc;
using RivrQuant.Application.Services;
using RivrQuant.Domain.Enums;

namespace RivrQuant.Api.Controllers;

/// <summary>REST API endpoints for risk management: position sizing, drawdown state, volatility targets.</summary>
[ApiController]
[Route("api/[controller]")]
public sealed class RiskController : ControllerBase
{
    private readonly RiskManagementService _riskService;

    public RiskController(RiskManagementService riskService)
    {
        _riskService = riskService;
    }

    /// <summary>Get the current drawdown state including deleverage level and multiplier.</summary>
    [HttpGet("drawdown")]
    public async Task<IActionResult> GetDrawdownState(CancellationToken ct)
    {
        var state = await _riskService.GetDrawdownStateAsync(ct);
        return Ok(state);
    }

    /// <summary>Get the current volatility target state.</summary>
    [HttpGet("volatility")]
    public async Task<IActionResult> GetVolatilityTarget(CancellationToken ct)
    {
        var target = await _riskService.GetVolatilityTargetAsync(ct);
        return Ok(target);
    }

    /// <summary>Calculate a recommended position size for a given symbol and parameters.</summary>
    [HttpPost("position-size")]
    public async Task<IActionResult> CalculatePositionSize(
        [FromBody] PositionSizeInputDto input,
        CancellationToken ct)
    {
        var recommendation = await _riskService.CalculatePositionSizeAsync(
            input.Symbol,
            input.Side,
            input.CurrentPrice,
            input.SignalConfidence,
            input.HistoricalWinRate,
            input.HistoricalAvgWinLossRatio,
            input.AssetAnnualizedVol,
            input.Broker,
            ct);

        return Ok(recommendation);
    }

    /// <summary>Get current drawdown multiplier (quick check for order validation).</summary>
    [HttpGet("drawdown/multiplier")]
    public async Task<IActionResult> GetDrawdownMultiplier(CancellationToken ct)
    {
        var multiplier = await _riskService.GetDrawdownMultiplierAsync(ct);
        return Ok(new { multiplier });
    }
}

public sealed record PositionSizeInputDto
{
    public required string Symbol { get; init; }
    public OrderSide Side { get; init; } = OrderSide.Buy;
    public required decimal CurrentPrice { get; init; }
    public decimal SignalConfidence { get; init; } = 0.5m;
    public decimal? HistoricalWinRate { get; init; }
    public decimal? HistoricalAvgWinLossRatio { get; init; }
    public decimal? AssetAnnualizedVol { get; init; }
    public BrokerType Broker { get; init; } = BrokerType.Alpaca;
}
