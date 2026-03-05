using Microsoft.AspNetCore.Mvc;
using RivrQuant.Application.Services;

namespace RivrQuant.Api.Controllers;

/// <summary>REST API endpoints for strategy capital allocations and rebalance triggers.</summary>
[ApiController]
[Route("api/[controller]")]
public sealed class AllocationController : ControllerBase
{
    private readonly AllocationService _allocationService;

    public AllocationController(AllocationService allocationService)
    {
        _allocationService = allocationService;
    }

    /// <summary>Get current capital allocations for all active strategies.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAllocations(CancellationToken ct)
    {
        var allocations = await _allocationService.GetAllocationsAsync(ct);
        return Ok(allocations);
    }

    /// <summary>Evaluate whether rebalancing is needed based on drift from targets.</summary>
    [HttpGet("rebalance")]
    public async Task<IActionResult> EvaluateRebalance(CancellationToken ct)
    {
        var decision = await _allocationService.EvaluateRebalanceAsync(ct);
        return Ok(decision);
    }

    /// <summary>Get strategy performance rankings.</summary>
    [HttpGet("rankings")]
    public async Task<IActionResult> GetRankings(CancellationToken ct)
    {
        var rankings = await _allocationService.GetStrategyRankingsAsync(ct);
        return Ok(rankings);
    }
}
