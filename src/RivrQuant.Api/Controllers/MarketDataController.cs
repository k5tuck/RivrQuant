namespace RivrQuant.Api.Controllers;

using Microsoft.AspNetCore.Mvc;

/// <summary>Market data endpoints.</summary>
[ApiController]
[Route("api/[controller]")]
public sealed class MarketDataController : ControllerBase
{
    /// <summary>Gets historical price bars for a symbol.</summary>
    [HttpGet("{symbol}/bars")]
    public IActionResult GetBars(string symbol, [FromQuery] string? from, [FromQuery] string? to, [FromQuery] string? resolution)
    {
        return Ok(Array.Empty<object>());
    }

    /// <summary>Gets the latest price bar for a symbol.</summary>
    [HttpGet("{symbol}/latest")]
    public IActionResult GetLatest(string symbol)
    {
        return Ok(new { symbol, open = 0, high = 0, low = 0, close = 0, volume = 0 });
    }
}
