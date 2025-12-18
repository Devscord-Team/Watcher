using Autofac;
using Discord.WebSocket;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Json;
using Watcher.Database;
using Watcher.Runner.Background;
using Watcher.Runner.DiscordEventHandlers;
using Watcher.Runner.Domain.AnomalyDetection;
using Watcher.Runner.Logging;
using Watcher.Runner.Monitoring;
using Watcher.Runner.Providers;
using Watcher.Runner.Storage;

namespace Watcher.Runner;
public static class ContainersFactory
{
    public static IContainer Create()
    {
        var build = new ContainerBuilder()
            .RegisterLogging()
            .RegisterDiscord()
            .RegisterStorage()
            .RegisterProviders()
            .RegisterAnomalyDetection()
            .RegisterEvents()
            .RegisterRealTimeMonitors()
            .RegisterBackground()
            .Build();

        return build;
    }

    private static ContainerBuilder RegisterLogging(this ContainerBuilder builder)
    {
        var openObserveEmail = Environment.GetEnvironmentVariable("ZO_ROOT_USER_EMAIL", EnvironmentVariableTarget.User);
        var openObservePassword = Environment.GetEnvironmentVariable("ZO_ROOT_USER_PASSWORD", EnvironmentVariableTarget.User);

        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .MinimumLevel.Debug()
            .WriteTo.Console(new JsonFormatter(), LogEventLevel.Debug)
            .WriteTo.OpenObserve("http://localhost:5080", "default", openObserveEmail, openObservePassword, "default", LogEventLevel.Debug)
            .CreateLogger();

        var eventLogger = new EventLogger((Logger)Log.Logger);
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
        _ = builder.RegisterType<DatabaseContextFactory>()
            .As<IDatabaseContextFactory>()
            .SingleInstance();

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

    private static ContainerBuilder RegisterEvents(this ContainerBuilder builder)
    {
        _ = builder.RegisterType<EventBus>()
            .As<IEventBus>()
            .SingleInstance();

        return builder;
    }

    private static ContainerBuilder RegisterRealTimeMonitors(this ContainerBuilder builder)
    {
        _ = builder.RegisterType<RealTimeActivityMonitor>()
            .As<IRealTimeActivityMonitor>()
            .SingleInstance();

        return builder;
    }

    private static ContainerBuilder RegisterBackground(this ContainerBuilder builder)
    {
        _ = builder.RegisterType<AnomalyChannelsScanner>()
            .As<IBackground>()
            .SingleInstance();

        return builder;
    }
}
