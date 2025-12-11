using Discord;
using Serilog.Events;

namespace Watcher.Runner;

public class EventLogger(Serilog.Core.Logger logger) : IEventLogger
{
    private const string EVENT_TEMPLATE = "{Event} @{Payload}";
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

    private void Warn<T>(string eventName, T payload)
        => logger.Warning(EVENT_TEMPLATE, eventName, payload);

    private void Info<T>(string eventName, T payload)
        => logger.Information(EVENT_TEMPLATE, eventName, payload);

    private void Info(string eventName)
        => logger.Information(EVENT_ONLY_TEMPLATE, eventName);

    private void Err<T>(string eventName, T payload)
        => logger.Error(EVENT_TEMPLATE, eventName, payload);

    private void Fatal<T>(string eventName, T payload)
        => logger.Fatal(EVENT_TEMPLATE, eventName, payload);
}
