using Discord;

namespace Watcher.Runner.Logging;

public interface IEventLogger : IDisposable
{
    void Event_BreakingProgramException(Exception exception);
    void Event_ProgramClosed();
    void Event_ProgramStarted();
    void Event_ReceivedMessage(MessageInfo statMessageInfo);
    void LogMessage(LogMessage logMessage);
}
