using Discord;
using Watcher.Runner.Logging;

namespace Watcher.Runner.DiscordEventHandlers;

public class LogHandler(IEventLogger eventLogger) : IDiscordEventHandler<LogMessage>
{
    public Task Handle(LogMessage message)
    {
        eventLogger.LogMessage(message);
        return Task.CompletedTask;
    }
}