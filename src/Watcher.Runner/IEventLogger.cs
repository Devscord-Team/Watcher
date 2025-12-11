using Discord;

namespace Watcher.Runner;

public interface IEventLogger
{
    void Event_BreakingProgramException(Exception exception);
    void Event_ProgramClosed();
    void Event_ProgramStarted();
    void LogMessage(LogMessage logMessage);
}
