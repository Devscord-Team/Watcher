using Discord;

namespace Watcher.Runner;

public interface IEventLogger
{
    void LogMessage(LogMessage logMessage);
}
