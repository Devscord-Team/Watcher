using Discord;
using Serilog.Core;
using Serilog.Events;
using Watcher.Runner.Domain.AnomalyDetection;

namespace Watcher.Runner.Logging;

public class EventLogger(Logger logger) : IEventLogger
{
    private const string EVENT_TEMPLATE = "{Event} {@Payload}";
    private const string EVENT_ONLY_TEMPLATE = "{Event}";
    private const string LOG_MESSAGE_TEMPLATE = "{@LogMessage}";

    public void LogMessage(LogMessage logMessage)
    {
        var level = logMessage.Severity switch
        {
            LogSeverity.Critical => LogEventLevel.Fatal,
            LogSeverity.Error => LogEventLevel.Error,
            LogSeverity.Warning => LogEventLevel.Warning,
            LogSeverity.Info => LogEventLevel.Information,
            LogSeverity.Verbose => LogEventLevel.Verbose,
            LogSeverity.Debug => LogEventLevel.Debug,
            _ => throw new NotImplementedException(),
        };

        logger.Write(level, LOG_MESSAGE_TEMPLATE, logMessage);
    }

    public void Event_ProgramStarted()
        => this.Info(nameof(Event_ProgramStarted));

    public void Event_ProgramClosed()
        => this.Info(nameof(Event_ProgramClosed));

    public void Event_BreakingProgramException(Exception exception)
        => this.Fatal(nameof(Event_BreakingProgramException), exception);

    public void Event_ReceivedMessage(MessageInfo statMessageInfo)
        => this.Info(nameof(Event_ReceivedMessage), statMessageInfo);

    public void Event_SavedMessagesInfos(ulong guildId, ulong channelId, int count) 
        => this.Info(nameof(Event_SavedMessagesInfos), new { GuildId = guildId, ChannelId = channelId, Count = count });

    public void Event_AnomalyDetectorCacheRefreshFinished()
        => this.Info(nameof(Event_AnomalyDetectorCacheRefreshFinished));

    public void Event_AnomalyDetectorCacheRefreshStarted()
        => this.Info(nameof(Event_AnomalyDetectorCacheRefreshStarted));

    public void Event_AnomalyDetectorScanChannelFinished(ulong channelId, AnomalyResult? result)
        => this.Info(nameof(Event_AnomalyDetectorScanChannelFinished), new { ChannelId = channelId, Result = result });

    public void Event_AnomalyDetectorScanChannelStarted(ulong channelId)
        => this.Info(nameof(Event_AnomalyDetectorScanChannelStarted), new { ChannelId = channelId });

    public void Event_AllEventHandlersExecuted(string typeName, int handlersCount)
        => this.Info(nameof(Event_AllEventHandlersExecuted), new { TypeName = typeName, HandlersCount = handlersCount });

    public void Event_EventHandlingException(string typeName, Exception exception)
        => this.Err(nameof(Event_EventHandlingException), new { TypeName = typeName }, exception);

    public void Event_EventSubscribed(string typeName)
        => this.Info(nameof(Event_EventSubscribed), new { TypeName = typeName });

    public void Event_EventUnsubscribed(string typeName)
        => this.Info(nameof(Event_EventUnsubscribed), new { TypeName = typeName });

    public void Event_PublishedEventOfType(string typeName)
        => this.Info(nameof(Event_PublishedEventOfType), new { TypeName = typeName });

    public void Dispose()
        => logger.Dispose();

    private void Warn<T>(string eventName, T payload)
        => logger.Warning(EVENT_TEMPLATE, eventName, payload);

    private void Info<T>(string eventName, T payload)
        => logger.Information(EVENT_TEMPLATE, eventName, payload);

    private void Info(string eventName)
        => logger.Information(EVENT_ONLY_TEMPLATE, eventName);

    private void Err<T>(string eventName, T payload, Exception exception)
        => logger.Error(exception, EVENT_TEMPLATE, eventName, payload);

    private void Fatal(string eventName, Exception exception)
        => logger.Fatal(exception, EVENT_TEMPLATE, eventName);

    private void Fatal<T>(string eventName, T payload, Exception exception)
        => logger.Fatal(exception, EVENT_TEMPLATE, eventName, payload);
}
