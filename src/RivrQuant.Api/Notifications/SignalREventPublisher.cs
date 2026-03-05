namespace RivrQuant.Api.Notifications;

using Microsoft.AspNetCore.SignalR;
using RivrQuant.Api.Hubs;
using RivrQuant.Application.Notifications;

/// <summary>
/// Publishes real-time events to all connected SignalR clients via <see cref="TradingHub"/>.
/// This concrete implementation lives in the API layer to avoid a circular project
/// reference from Application → API.
/// </summary>
public sealed class SignalREventPublisher : IRealtimeEventPublisher
{
    private readonly IHubContext<TradingHub> _hub;

    /// <summary>Initializes a new instance of <see cref="SignalREventPublisher"/>.</summary>
    public SignalREventPublisher(IHubContext<TradingHub> hub)
    {
        _hub = hub;
    }

    /// <inheritdoc />
    public Task PublishAsync(string eventName, object data, CancellationToken ct)
        => _hub.Clients.All.SendAsync(eventName, data, ct);
}
