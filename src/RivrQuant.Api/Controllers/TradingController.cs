namespace RivrQuant.Api.Controllers;

using Microsoft.AspNetCore.Mvc;
using RivrQuant.Application.DTOs;
using RivrQuant.Application.Services;

/// <summary>Trading operations: positions, orders, kill switch.</summary>
[ApiController]
[Route("api/[controller]")]
public sealed class TradingController : ControllerBase
{
    private readonly TradingService _service;

    /// <summary>Initializes a new instance of <see cref="TradingController"/>.</summary>
    public TradingController(TradingService service) => _service = service;

    /// <summary>Gets all open positions across brokers.</summary>
    [HttpGet("positions")]
    public async Task<IActionResult> GetPositions(CancellationToken ct)
    {
        var positions = await _service.GetPositionsAsync(ct);
        return Ok(positions);
    }

    /// <summary>Gets order history.</summary>
    [HttpGet("orders")]
    public async Task<IActionResult> GetOrders(CancellationToken ct)
    {
        var orders = await _service.GetOrdersAsync(ct);
        return Ok(orders);
    }

    /// <summary>Places a new order.</summary>
    [HttpPost("orders")]
    public async Task<IActionResult> PlaceOrder([FromBody] PlaceOrderDto dto, CancellationToken ct)
    {
        var order = await _service.PlaceOrderAsync(dto, ct);
        return Ok(order);
    }

    /// <summary>Cancels an existing order.</summary>
    [HttpDelete("orders/{id}")]
    public async Task<IActionResult> CancelOrder(string id, CancellationToken ct)
    {
        await _service.CancelOrderAsync(id, ct);
        return NoContent();
    }

    /// <summary>Kill switch: closes all positions across all brokers.</summary>
    [HttpPost("close-all")]
    public async Task<IActionResult> CloseAll(CancellationToken ct)
    {
        await _service.CloseAllPositionsAsync(ct);
        return Ok(new { message = "All positions closed" });
    }
}
