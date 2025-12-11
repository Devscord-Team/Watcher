using Discord;

namespace Watcher.Runner;

public class LogHandler(IEventLogger eventLogger) : IDiscordEventHandler<LogMessage>
{
    public Task Handle(LogMessage message)
    {
        eventLogger.LogMessage(message);
        return Task.CompletedTask;
    }
}