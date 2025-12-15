using Discord;
using Watcher.Runner.Domain.AnomalyDetection;

namespace Watcher.Runner.Logging;

public interface IEventLogger : IDisposable
{
    void Event_AllEventHandlersExecuted(string typeName, int handlersCount);
    void Event_AnomalyDetectorCacheRefreshFinished();
    void Event_AnomalyDetectorCacheRefreshStarted();
    void Event_AnomalyDetectorScanChannelFinished(ulong channelId, AnomalyResult? result);
    void Event_AnomalyDetectorScanChannelStarted(ulong channelId);
    void Event_BreakingProgramException(Exception exception);
    void Event_EventHandlingException(string typeName, Exception exception);
    void Event_EventSubscribed(string typeName);
    void Event_EventUnsubscribed(string typeName);
    void Event_ProgramClosed();
    void Event_ProgramStarted();
    void Event_PublishedEventOfType(string typeName);
    void Event_ReceivedMessage(MessageInfo statMessageInfo);
    void Event_SavedMessagesInfos(ulong guildId, ulong channelId, int count);
    void LogMessage(LogMessage logMessage);
}
