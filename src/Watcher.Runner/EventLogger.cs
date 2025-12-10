using Discord;
using Serilog.Events;

namespace Watcher.Runner;

public class EventLogger(Serilog.Core.Logger logger) : IEventLogger
{
    //private const string EVENT_TEMPLATE = "{Event} @{Payload}";
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
}
