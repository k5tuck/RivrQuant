namespace RivrQuant.Api.Controllers;

using Microsoft.AspNetCore.Mvc;
using RivrQuant.Application.Services;

/// <summary>Aggregated dashboard data endpoints.</summary>
[ApiController]
[Route("api/[controller]")]
public sealed class DashboardController : ControllerBase
{
    private readonly DashboardService _service;

    /// <summary>Initializes a new instance of <see cref="DashboardController"/>.</summary>
    public DashboardController(DashboardService service) => _service = service;

    /// <summary>Gets the full dashboard data (portfolio, positions, metrics, recent trades, alerts).</summary>
    [HttpGet]
    public async Task<IActionResult> GetDashboard(CancellationToken ct)
    {
        var dashboard = await _service.GetDashboardAsync(ct);
        return Ok(dashboard);
    }

    /// <summary>Gets portfolio data only.</summary>
    [HttpGet("portfolio")]
    public async Task<IActionResult> GetPortfolio(CancellationToken ct)
    {
        var portfolio = await _service.GetPortfolioAsync(ct);
        return Ok(portfolio);
    }
}
