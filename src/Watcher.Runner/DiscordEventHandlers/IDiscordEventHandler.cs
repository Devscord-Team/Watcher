namespace Watcher.Runner.DiscordEventHandlers;

public interface IDiscordEventHandler<T>
{
    Task Handle(T @event);
}