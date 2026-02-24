namespace RivrQuant.Api.Controllers;

using Microsoft.AspNetCore.Mvc;
using RivrQuant.Application.DTOs;
using RivrQuant.Application.Services;
using RivrQuant.Domain.Models.Alerts;

/// <summary>Alert rules and history endpoints.</summary>
[ApiController]
[Route("api/[controller]")]
public sealed class AlertController : ControllerBase
{
    private readonly AlertAppService _service;

    /// <summary>Initializes a new instance of <see cref="AlertController"/>.</summary>
    public AlertController(AlertAppService service) => _service = service;

    /// <summary>Lists all alert rules.</summary>
    [HttpGet("rules")]
    public async Task<IActionResult> GetRules(CancellationToken ct)
    {
        var rules = await _service.GetRulesAsync(ct);
        return Ok(rules);
    }

    /// <summary>Creates a new alert rule.</summary>
    [HttpPost("rules")]
    public async Task<IActionResult> CreateRule([FromBody] CreateAlertRuleDto dto, CancellationToken ct)
    {
        var alertRule = new AlertRule
        {
            Name = dto.Name,
            ConditionType = dto.ConditionType,
            Threshold = dto.Threshold,
            ComparisonOperator = dto.ComparisonOperator,
            SendEmail = dto.SendEmail,
            SendSms = dto.SendSms,
            CooldownPeriod = TimeSpan.FromMinutes(dto.CooldownMinutes)
        };
        var rule = await _service.CreateRuleAsync(alertRule, ct);
        return Ok(rule);
    }

    /// <summary>Deletes an alert rule.</summary>
    [HttpDelete("rules/{id:guid}")]
    public async Task<IActionResult> DeleteRule(Guid id, CancellationToken ct)
    {
        await _service.DeleteRuleAsync(id, ct);
        return NoContent();
    }

    /// <summary>Toggles an alert rule's active state.</summary>
    [HttpPut("rules/{id:guid}/toggle")]
    public async Task<IActionResult> ToggleRule(Guid id, CancellationToken ct)
    {
        await _service.ToggleRuleAsync(id, ct);
        return NoContent();
    }

    /// <summary>Gets alert event history.</summary>
    [HttpGet("history")]
    public async Task<IActionResult> GetHistory(CancellationToken ct)
    {
        var events = await _service.GetHistoryAsync(null, null, ct);
        return Ok(events);
    }

    /// <summary>Acknowledges an alert event.</summary>
    [HttpPut("history/{id:guid}/acknowledge")]
    public async Task<IActionResult> Acknowledge(Guid id, CancellationToken ct)
    {
        await _service.AcknowledgeAsync(id, ct);
        return NoContent();
    }
}
