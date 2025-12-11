using Autofac;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;

namespace Watcher.Runner;
public static class ContainersFactory
{
    public static IContainer Create()
    {
        return new ContainerBuilder()
            .RegisterLogging()
            .RegisterDiscord()
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
        builder.RegisterType<DiscordInitializer>()
            .As<IDiscordInitializer>()
            .SingleInstance();

        return builder;
    }
}
