namespace RivrQuant.Application.BackgroundJobs;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RivrQuant.Application.Notifications;
using RivrQuant.Domain.Interfaces;
using RivrQuant.Domain.Models.Risk;

/// <summary>
/// Long-running hosted service that evaluates drawdown state every 15 seconds.
/// Replaces <see cref="DrawdownMonitorJob"/> (Hangfire) to bypass Hangfire's
/// 1-minute minimum cron resolution, which is insufficient for scalping strategies.
///
/// When a drawdown breach is detected (deleverage level changes or drawdown exceeds
/// thresholds), a <c>DrawdownBreached</c> event is pushed to all connected frontend
/// clients via <see cref="IRealtimeEventPublisher"/>.
/// </summary>
public sealed class DrawdownMonitorService : BackgroundService
{
    private static readonly TimeSpan CheckInterval = TimeSpan.FromSeconds(15);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DrawdownMonitorService> _logger;

    /// <summary>Initializes a new instance of <see cref="DrawdownMonitorService"/>.</summary>
    public DrawdownMonitorService(
        IServiceScopeFactory scopeFactory,
        ILogger<DrawdownMonitorService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "DrawdownMonitorService started — checking every {Interval}s",
            CheckInterval.TotalSeconds);

        // Delay the first check slightly to allow the application to finish starting up.
        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            await RunCheckAsync(stoppingToken);
            await Task.Delay(CheckInterval, stoppingToken);
        }

        _logger.LogInformation("DrawdownMonitorService stopped.");
    }

    private async Task RunCheckAsync(CancellationToken ct)
    {
        // IDrawdownManager and IRealtimeEventPublisher are scoped services —
        // create a fresh scope for each check cycle.
        using var scope = _scopeFactory.CreateScope();
        var drawdownManager = scope.ServiceProvider.GetRequiredService<IDrawdownManager>();
        var publisher       = scope.ServiceProvider.GetRequiredService<IRealtimeEventPublisher>();

        try
        {
            var stateBefore = await drawdownManager.GetCurrentStateAsync(ct);
            await drawdownManager.EvaluateAndActAsync(ct);
            var stateAfter  = await drawdownManager.GetCurrentStateAsync(ct);

            // Emit a SignalR event when the deleverage level escalates or the drawdown
            // percentage has worsened meaningfully (> 0.5 pp change since last check).
            var levelChanged   = stateAfter.ActiveLevel != stateBefore.ActiveLevel;
            var drawdownWorsened = stateAfter.DrawdownPercent < stateBefore.DrawdownPercent - 0.5m;

            if (levelChanged || drawdownWorsened)
            {
                _logger.LogWarning(
                    "DrawdownBreached — Level: {PrevLevel} → {NewLevel}, Drawdown: {Drawdown:P2}, " +
                    "Current equity: {Equity:C}, Peak equity: {Peak:C}",
                    stateBefore.ActiveLevel, stateAfter.ActiveLevel,
                    Math.Abs(stateAfter.DrawdownPercent / 100m),
                    stateAfter.CurrentEquity, stateAfter.PeakEquity);

                await publisher.PublishAsync("DrawdownBreached", new
                {
                    previousLevel    = stateBefore.ActiveLevel.ToString(),
                    newLevel         = stateAfter.ActiveLevel.ToString(),
                    drawdownPercent  = stateAfter.DrawdownPercent,
                    drawdownDollars  = stateAfter.DrawdownDollars,
                    currentEquity    = stateAfter.CurrentEquity,
                    peakEquity       = stateAfter.PeakEquity,
                    daysInDrawdown   = stateAfter.DaysInDrawdown,
                    currentMultiplier = stateAfter.CurrentMultiplier,
                    pausedStrategies = stateAfter.PausedStrategies,
                    lastUpdated      = stateAfter.LastUpdated
                }, ct);
            }
            else
            {
                _logger.LogDebug(
                    "Drawdown check OK — Level: {Level}, Drawdown: {Drawdown:P2}",
                    stateAfter.ActiveLevel,
                    Math.Abs(stateAfter.DrawdownPercent / 100m));
            }
        }
        catch (OperationCanceledException)
        {
            // Shutdown requested — let the loop exit gracefully.
        }
        catch (Exception ex)
        {
            // CRITICAL: drawdown protection may be compromised. Log but do NOT crash
            // the hosted service — the next 15-second tick will retry.
            _logger.LogCritical(ex,
                "DrawdownMonitorService check FAILED — drawdown protection may be compromised. " +
                "Trading should be halted until this is resolved.");
        }
    }
}
