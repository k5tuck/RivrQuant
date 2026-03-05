using Microsoft.AspNetCore.Mvc;
using RivrQuant.Application.Services;
using RivrQuant.Domain.Enums;

namespace RivrQuant.Api.Controllers;

/// <summary>REST API endpoints for execution cost estimation and fill quality reports.</summary>
[ApiController]
[Route("api/[controller]")]
public sealed class ExecutionController : ControllerBase
{
    private readonly ExecutionService _executionService;

    public ExecutionController(ExecutionService executionService)
    {
        _executionService = executionService;
    }

    /// <summary>Estimate execution costs for a hypothetical order.</summary>
    [HttpPost("estimate")]
    public async Task<IActionResult> EstimateCost(
        [FromBody] CostEstimateInputDto input,
        CancellationToken ct)
    {
        var estimate = await _executionService.EstimateCostAsync(
            input.Symbol, input.Side, input.Quantity, input.CurrentPrice, input.Broker, ct);
        return Ok(estimate);
    }

    /// <summary>Generate an execution quality report over a time period.</summary>
    [HttpGet("report")]
    public async Task<IActionResult> GetExecutionReport(
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to,
        [FromQuery] BrokerType? broker,
        CancellationToken ct)
    {
        var fromDate = from ?? DateTimeOffset.UtcNow.AddDays(-30);
        var toDate = to ?? DateTimeOffset.UtcNow;
        var report = await _executionService.GenerateReportAsync(fromDate, toDate, broker, ct);
        return Ok(report);
    }
}

public sealed record CostEstimateInputDto
{
    public required string Symbol { get; init; }
    public OrderSide Side { get; init; } = OrderSide.Buy;
    public decimal Quantity { get; init; } = 1m;
    public required decimal CurrentPrice { get; init; }
    public BrokerType Broker { get; init; } = BrokerType.Alpaca;
}
