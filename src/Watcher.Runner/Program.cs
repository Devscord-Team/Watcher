using Autofac;
using Watcher.Runner;

var container = ContainersFactory.Create();
var eventLogger = container.Resolve<IEventLogger>();

try
{
    eventLogger.Event_ProgramStarted();

    var token = Environment.GetEnvironmentVariable("WATCHER_DISCORD_TOKEN")
        ?? throw new ArgumentNullException("Discord token is empty. Set WATCHER_DISCORD_TOKEN environment variable.");

    var discordRunner = container.Resolve<IDiscordRunner>();
    await discordRunner.Run(token);
    await Task.Delay(-1);
}
catch(Exception exception)
{
    eventLogger.Event_BreakingProgramException(exception);
}
finally
{
    eventLogger.Event_ProgramClosed();
}
