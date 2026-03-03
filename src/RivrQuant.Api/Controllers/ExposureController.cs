using Microsoft.AspNetCore.Mvc;
using RivrQuant.Application.Services;

namespace RivrQuant.Api.Controllers;

/// <summary>REST API endpoints for portfolio exposure tracking and correlation analysis.</summary>
[ApiController]
[Route("api/[controller]")]
public sealed class ExposureController : ControllerBase
{
    private readonly ExposureService _exposureService;

    public ExposureController(ExposureService exposureService)
    {
        _exposureService = exposureService;
    }

    /// <summary>Get the current portfolio exposure breakdown.</summary>
    [HttpGet]
    public async Task<IActionResult> GetCurrentExposure(CancellationToken ct)
    {
        var exposure = await _exposureService.GetCurrentExposureAsync(ct);
        return Ok(exposure);
    }

    /// <summary>Get the cross-asset correlation matrix.</summary>
    [HttpGet("correlations")]
    public async Task<IActionResult> GetCorrelationMatrix(
        [FromQuery] int lookbackDays = 60,
        CancellationToken ct = default)
    {
        var snapshot = await _exposureService.GetCorrelationMatrixAsync(lookbackDays, ct);
        return Ok(snapshot);
    }

    /// <summary>Validate whether a new order would breach exposure limits.</summary>
    [HttpPost("validate")]
    public async Task<IActionResult> ValidateOrderExposure(
        [FromBody] ExposureValidationDto input,
        CancellationToken ct)
    {
        var (isAllowed, blockReason) = await _exposureService.ValidateOrderExposureAsync(
            input.Symbol, input.NotionalValue, input.IsLong, ct);

        return Ok(new { isAllowed, blockReason });
    }
}

public sealed record ExposureValidationDto
{
    public required string Symbol { get; init; }
    public decimal NotionalValue { get; init; }
    public bool IsLong { get; init; } = true;
}
