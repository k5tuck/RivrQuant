namespace RivrQuant.Application.Notifications;

/// <summary>
/// Publishes real-time events to connected frontend clients. The Application layer
/// depends on this interface; the concrete implementation (SignalR) lives in the API
/// layer to avoid a circular project reference.
/// </summary>
public interface IRealtimeEventPublisher
{
    /// <summary>
    /// Sends a named event with an arbitrary payload to all connected clients.
    /// </summary>
    /// <param name="eventName">The event name the client subscribes to.</param>
    /// <param name="data">The payload object (serialised to JSON by the transport).</param>
    /// <param name="ct">Cancellation token.</param>
    Task PublishAsync(string eventName, object data, CancellationToken ct);
}
