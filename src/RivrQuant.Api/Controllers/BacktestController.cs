namespace RivrQuant.Api.Controllers;

using Microsoft.AspNetCore.Mvc;
using RivrQuant.Application.Services;

/// <summary>Backtest management and analysis endpoints.</summary>
[ApiController]
[Route("api/[controller]")]
public sealed class BacktestController : ControllerBase
{
    private readonly BacktestService _service;

    /// <summary>Initializes a new instance of <see cref="BacktestController"/>.</summary>
    public BacktestController(BacktestService service) => _service = service;

    /// <summary>Lists all backtests.</summary>
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var backtests = await _service.GetAllAsync(ct);
        return Ok(backtests);
    }

    /// <summary>Gets a single backtest with full details.</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var backtest = await _service.GetByIdAsync(id, ct);
        if (backtest is null) return NotFound();
        return Ok(backtest);
    }

    /// <summary>Triggers AI analysis for a backtest.</summary>
    [HttpPost("{id:guid}/analyze")]
    public async Task<IActionResult> Analyze(Guid id, CancellationToken ct)
    {
        var report = await _service.AnalyzeAsync(id, ct);
        return Ok(report);
    }

    /// <summary>Compares multiple backtests.</summary>
    [HttpPost("compare")]
    public async Task<IActionResult> Compare([FromBody] Guid[] ids, CancellationToken ct)
    {
        var comparison = await _service.CompareAsync(ids, ct);
        return Ok(comparison);
    }
}
