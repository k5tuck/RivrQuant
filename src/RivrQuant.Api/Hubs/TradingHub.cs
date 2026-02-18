namespace RivrQuant.Api.Hubs;

using Microsoft.AspNetCore.SignalR;

/// <summary>SignalR hub for real-time trading updates to the frontend.</summary>
public sealed class TradingHub : Hub
{
    /// <summary>Sends a portfolio update to all connected clients.</summary>
    public async Task SendPortfolioUpdate(object data)
        => await Clients.All.SendAsync("PortfolioUpdate", data);

    /// <summary>Sends a position update to all connected clients.</summary>
    public async Task SendPositionUpdate(object data)
        => await Clients.All.SendAsync("PositionUpdate", data);

    /// <summary>Sends an order update to all connected clients.</summary>
    public async Task SendOrderUpdate(object data)
        => await Clients.All.SendAsync("OrderUpdate", data);

    /// <summary>Sends a trade executed notification to all connected clients.</summary>
    public async Task SendTradeExecuted(object data)
        => await Clients.All.SendAsync("TradeExecuted", data);

    /// <summary>Sends a new backtest detected notification.</summary>
    public async Task SendBacktestDetected(object data)
        => await Clients.All.SendAsync("BacktestDetected", data);

    /// <summary>Sends an analysis complete notification.</summary>
    public async Task SendAnalysisComplete(object data)
        => await Clients.All.SendAsync("AnalysisComplete", data);

    /// <summary>Sends an alert triggered notification.</summary>
    public async Task SendAlertTriggered(object data)
        => await Clients.All.SendAsync("AlertTriggered", data);

    /// <summary>Sends a broker status change notification.</summary>
    public async Task SendBrokerStatusChange(object data)
        => await Clients.All.SendAsync("BrokerStatusChange", data);

    /// <inheritdoc />
    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
    }

    /// <inheritdoc />
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
    }
}
