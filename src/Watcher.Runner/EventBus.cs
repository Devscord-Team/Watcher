using System.Collections.Concurrent;

namespace Watcher.Runner;
public class EventBus : IEventBus
{
    private readonly ConcurrentDictionary<string, List<Action<object>>> subscribers = new();

    public void Subscribe<T>(Action<T> handler) where T : IEvent
    {
        var typeName = typeof(T).FullName!;
        if (this.subscribers.TryGetValue(typeName, out var eventSubscribers))
        {
            eventSubscribers.Add(x => handler((T)x));
        }
        else
        {
            if (!this.subscribers.TryAdd(typeName, [x => handler((T)x)]))
            {
                this.subscribers[typeName].Add(x => handler((T)x));
            }
        }
    }

    public void Publish<T>(T @event)
    {
        var typeName = typeof(T).FullName!;
        if (this.subscribers.TryGetValue(typeName, out var eventSubscribers))
        {
            _ = Task.Run(() =>
            {
                try
                {
                    var tasks = eventSubscribers.Select(handler => Task.Run(() => handler(@event!)));
                    Task.WaitAll(tasks);
                }
                catch (Exception)
                {
                    //todo log exception
                    throw;
                }
            });
        }
    }
}
