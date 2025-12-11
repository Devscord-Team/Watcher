using Discord;

namespace Watcher.Runner.Logging;

public interface IEventLogger
{
    void Event_BreakingProgramException(Exception exception);
    void Event_ProgramClosed();
    void Event_ProgramStarted();
    void Event_ReceivedMessage(StatMessageInfo statMessageInfo);
    void LogMessage(LogMessage logMessage);
}
