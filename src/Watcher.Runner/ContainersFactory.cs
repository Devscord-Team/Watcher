using Autofac;
using Discord.WebSocket;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;
using Watcher.Runner.DiscordEventHandlers;
using Watcher.Runner.Logging;
using Watcher.Runner.Providers;
using Watcher.Runner.Storage;

namespace Watcher.Runner;
public static class ContainersFactory
{
    public static IContainer Create()
    {
        return new ContainerBuilder()
            .RegisterLogging()
            .RegisterDiscord()
            .RegisterStorage()
            .RegisterProviders()
            .RegisterAnomalyDetection()
            .Build();
    }

    private static ContainerBuilder RegisterLogging(this ContainerBuilder builder)
    {
        var logger = new LoggerConfiguration()
            .WriteTo.Console(new JsonFormatter(), LogEventLevel.Debug)
            .CreateLogger();

        var eventLogger = new EventLogger(logger);
        _ = builder.RegisterInstance(eventLogger)
            .As<IEventLogger>()
            .SingleInstance();

        return builder;
    }

    private static ContainerBuilder RegisterDiscord(this ContainerBuilder builder)
    {
        _ = builder.RegisterType<DiscordRunner>()
            .As<IDiscordRunner>()
            .SingleInstance();

        _ = builder.RegisterType<MessageReceivedHandler>()
            .As<IDiscordEventHandler<SocketMessage>>()
            .SingleInstance();

        _ = builder.RegisterType<SlashCommandExecutedHandler>()
            .As<IDiscordEventHandler<SocketSlashCommand>>()
            .SingleInstance();

        _ = builder.RegisterType<LogHandler>()
            .As<IDiscordEventHandler<Discord.LogMessage>>()
            .SingleInstance();

        return builder;
    }

    private static ContainerBuilder RegisterStorage(this ContainerBuilder builder)
    {
        _ = builder.RegisterType<MessagesStorage>()
            .As<IMessagesStorage>()
            .SingleInstance();

        return builder;
    }

    private static ContainerBuilder RegisterProviders(this ContainerBuilder builder)
    {
        _ = builder.RegisterType<DateTimeProvider>()
            .As<IDateTimeProvider>()
            .SingleInstance();

        return builder;
    }

    private static ContainerBuilder RegisterAnomalyDetection(this ContainerBuilder builder)
    {
        _ = builder.RegisterType<AnomalyDetector>()
            .As<IAnomalyDetector>()
            .SingleInstance();

        return builder;
    }
}
