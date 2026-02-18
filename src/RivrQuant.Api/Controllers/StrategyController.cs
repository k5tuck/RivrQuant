namespace RivrQuant.Api.Controllers;

using Microsoft.AspNetCore.Mvc;
using RivrQuant.Application.Services;

/// <summary>Strategy CRUD endpoints.</summary>
[ApiController]
[Route("api/[controller]")]
public sealed class StrategyController : ControllerBase
{
    private readonly StrategyService _service;

    /// <summary>Initializes a new instance of <see cref="StrategyController"/>.</summary>
    public StrategyController(StrategyService service) => _service = service;

    /// <summary>Lists all strategies.</summary>
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var strategies = await _service.ListAsync(ct);
        return Ok(strategies);
    }

    /// <summary>Gets a specific strategy.</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var strategy = await _service.GetAsync(id, ct);
        if (strategy is null) return NotFound();
        return Ok(strategy);
    }
}
