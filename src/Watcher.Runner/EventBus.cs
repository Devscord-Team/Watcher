using System.Collections.Concurrent;
using System.Collections.Immutable;
using Watcher.Runner.Logging;

namespace Watcher.Runner;

public class EventBus(IEventLogger eventLogger) : IEventBus
{
    private readonly ConcurrentDictionary<string, ImmutableList<Action<object>>> _subscribers = new();
    private readonly object _lock = new();

    public IDisposable Subscribe<T>(Action<T> handler) where T : IEvent
    {
        var typeName = typeof(T).FullName!;
        var wrappedHandler = new Action<object>(x => handler((T)x));

        lock (this._lock)
        {
            _ = this._subscribers.AddOrUpdate(
                typeName,
                _ => ImmutableList.Create(wrappedHandler),
                (_, existing) => existing.Add(wrappedHandler));
        }

        eventLogger.Event_EventSubscribed(typeName);

        return new Subscription(() => this.Unsubscribe(typeName, wrappedHandler));
    }

    private void Unsubscribe(string typeName, Action<object> handler)
    {
        lock (this._lock)
        {
            if (!this._subscribers.TryGetValue(typeName, out var handlers))
            {
                throw new InvalidOperationException("Cannot unsubscribe from event type without any handlers.");
            }

            var updated = handlers.Remove(handler);

            if (updated.IsEmpty)
            {
                _ = this._subscribers.TryRemove(typeName, out _);
            }
            else
            {
                this._subscribers[typeName] = updated;
            }
        }

        eventLogger.Event_EventUnsubscribed(typeName);
    }

    public void Publish<T>(T @event) where T : IEvent
    {
        var typeName = typeof(T).FullName!;
        eventLogger.Event_PublishedEventOfType(typeName);

        if (!this._subscribers.TryGetValue(typeName, out var handlers))
        {
            return;
        }

        _ = Task.Run(() => this.ExecuteHandlers(typeName, handlers, @event));
    }

    private void ExecuteHandlers<T>(string typeName, ImmutableList<Action<object>> handlers, T @event)
    {
        var exceptions = new List<Exception>();

        foreach (var handler in handlers)
        {
            try
            {
                handler(@event!);
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }
        }

        if (exceptions.Count > 0)
        {
            var aggregated = new AggregateException(exceptions);
            eventLogger.Event_EventHandlingException(typeName, aggregated);
        }

        eventLogger.Event_AllEventHandlersExecuted(typeName, handlers.Count);
    }

    private sealed class Subscription(Action unsubscribeAction) : IDisposable
    {
        private int _disposed;

        public void Dispose()
        {
            if (Interlocked.Exchange(ref this._disposed, 1) == 0)
            {
                unsubscribeAction();
            }
        }
    }
}