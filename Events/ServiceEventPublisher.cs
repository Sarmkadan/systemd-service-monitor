// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SystemdServiceMonitor.Events;

/// <summary>
/// Publishes events about service state changes and monitoring activities.
/// Uses a pub/sub pattern to decouple event producers from consumers.
/// </summary>
public interface IServiceEventPublisher
{
    /// <summary>
    /// Publishes a service state change event.
    /// </summary>
    Task PublishServiceStateChangedAsync(ServiceStateChangedEvent eventData);

    /// <summary>
    /// Publishes a service restart event.
    /// </summary>
    Task PublishServiceRestartedAsync(ServiceRestartedEvent eventData);

    /// <summary>
    /// Publishes a service health check event.
    /// </summary>
    Task PublishHealthCheckAsync(ServiceHealthCheckEvent eventData);

    /// <summary>
    /// Publishes a service control operation event (start, stop, restart, etc.).
    /// </summary>
    Task PublishServiceControlAsync(ServiceControlEvent eventData);
}

/// <summary>
/// In-memory implementation of the service event publisher.
/// For distributed scenarios, implement this with message queue (RabbitMQ, Azure Service Bus).
/// </summary>
public class ServiceEventPublisher : IServiceEventPublisher
{
    private readonly ILogger<ServiceEventPublisher> _logger;
    private readonly List<Func<ServiceEventBase, Task>> _subscribers = new();
    private readonly object _subscriberLock = new();

    public ServiceEventPublisher(ILogger<ServiceEventPublisher> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task PublishServiceStateChangedAsync(ServiceStateChangedEvent eventData)
    {
        _logger.LogInformation("Service state changed: {ServiceName} -> {NewState}",
            eventData.ServiceName, eventData.NewState);

        await PublishEventAsync(eventData);
    }

    public async Task PublishServiceRestartedAsync(ServiceRestartedEvent eventData)
    {
        _logger.LogWarning("Service restarted: {ServiceName} (restart count: {RestartCount})",
            eventData.ServiceName, eventData.RestartCount);

        await PublishEventAsync(eventData);
    }

    public async Task PublishHealthCheckAsync(ServiceHealthCheckEvent eventData)
    {
        if (!eventData.IsHealthy)
        {
            _logger.LogWarning("Health check failed for {ServiceName}: {Reason}",
                eventData.ServiceName, eventData.FailureReason);
        }

        await PublishEventAsync(eventData);
    }

    public async Task PublishServiceControlAsync(ServiceControlEvent eventData)
    {
        _logger.LogInformation("Service control: {ServiceName} -> {Operation}",
            eventData.ServiceName, eventData.Operation);

        await PublishEventAsync(eventData);
    }

    /// <summary>
    /// Subscribes to all service events.
    /// </summary>
    public void Subscribe(Func<ServiceEventBase, Task> handler)
    {
        if (handler == null)
            throw new ArgumentNullException(nameof(handler));

        lock (_subscriberLock)
        {
            _subscribers.Add(handler);
        }
    }

    /// <summary>
    /// Unsubscribes from service events.
    /// </summary>
    public void Unsubscribe(Func<ServiceEventBase, Task> handler)
    {
        if (handler == null)
            return;

        lock (_subscriberLock)
        {
            _subscribers.Remove(handler);
        }
    }

    private async Task PublishEventAsync(ServiceEventBase eventData)
    {
        List<Func<ServiceEventBase, Task>> subscribersCopy;

        lock (_subscriberLock)
        {
            subscribersCopy = new List<Func<ServiceEventBase, Task>>(_subscribers);
        }

        var tasks = subscribersCopy.Select(handler =>
        {
            try
            {
                return handler(eventData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in event subscriber");
                return Task.CompletedTask;
            }
        });

        await Task.WhenAll(tasks);
    }
}

/// <summary>
/// Base class for all service events.
/// </summary>
public abstract class ServiceEventBase
{
    public string EventId { get; set; } = Guid.NewGuid().ToString();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string ServiceName { get; set; } = string.Empty;
}

/// <summary>
/// Event published when a service changes state.
/// </summary>
public class ServiceStateChangedEvent : ServiceEventBase
{
    public string PreviousState { get; set; } = string.Empty;
    public string NewState { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}

/// <summary>
/// Event published when a service restarts.
/// </summary>
public class ServiceRestartedEvent : ServiceEventBase
{
    public int RestartCount { get; set; }
    public TimeSpan Uptime { get; set; }
    public string? CrashReason { get; set; }
}

/// <summary>
/// Event published after a health check.
/// </summary>
public class ServiceHealthCheckEvent : ServiceEventBase
{
    public bool IsHealthy { get; set; }
    public string? FailureReason { get; set; }
    public long ResponseTimeMs { get; set; }
}

/// <summary>
/// Event published when a control operation is performed on a service.
/// </summary>
public class ServiceControlEvent : ServiceEventBase
{
    public string Operation { get; set; } = string.Empty; // start, stop, restart, reload, enable, disable
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public long ExecutionTimeMs { get; set; }
}
