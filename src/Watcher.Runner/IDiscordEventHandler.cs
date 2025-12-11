namespace Watcher.Runner;

public interface IDiscordEventHandler<T>
{
    Task Handle(T @event);
}