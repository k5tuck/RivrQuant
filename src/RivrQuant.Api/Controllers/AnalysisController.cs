namespace RivrQuant.Api.Controllers;

using Microsoft.AspNetCore.Mvc;
using RivrQuant.Application.Services;

/// <summary>AI analysis report endpoints.</summary>
[ApiController]
[Route("api/[controller]")]
public sealed class AnalysisController : ControllerBase
{
    private readonly AnalysisService _service;

    /// <summary>Initializes a new instance of <see cref="AnalysisController"/>.</summary>
    public AnalysisController(AnalysisService service) => _service = service;

    /// <summary>Lists all analysis reports.</summary>
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var reports = await _service.ListReportsAsync(ct);
        return Ok(reports);
    }

    /// <summary>Gets a specific analysis report.</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var report = await _service.GetReportAsync(id, ct);
        if (report is null) return NotFound();
        return Ok(report);
    }

    /// <summary>Runs analysis on demand for a backtest.</summary>
    [HttpPost("{backtestId:guid}/run")]
    public async Task<IActionResult> RunAnalysis(Guid backtestId, CancellationToken ct)
    {
        var report = await _service.RunAnalysisAsync(backtestId, ct);
        return Ok(report);
    }
}
