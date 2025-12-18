using Autofac;
using Watcher.Runner;
using Watcher.Runner.Background;
using Watcher.Runner.Domain.AnomalyDetection;
using Watcher.Runner.Logging;
using Watcher.Runner.Monitoring;

using var container = ContainersFactory.Create();
using var eventLogger = container.Resolve<IEventLogger>();

try
{
    eventLogger.Event_ProgramStarted();

    var token = Environment.GetEnvironmentVariable("WATCHER_DISCORD_TOKEN")
        ?? throw new ArgumentException("Discord token is empty. Set WATCHER_DISCORD_TOKEN environment variable.");

    //Initialize
    await Task.WhenAll([
        container.Resolve<IAnomalyDetector>().Initialize(),
        container.Resolve<IRealTimeActivityMonitor>().Initialize()
    ]);

    //todo auto restart when job is finished
    var backgroundJobs = container.Resolve<IEnumerable<IBackground>>().Select(x => x.ExecuteAsync(CancellationToken.None)).ToArray();

    var discordRunner = container.Resolve<IDiscordRunner>();
    await discordRunner.Run(token);
    await Task.WhenAll(backgroundJobs);
    await Task.Delay(-1);
}
catch (Exception exception)
{
    eventLogger.Event_BreakingProgramException(exception);
}
finally
{
    eventLogger.Event_ProgramClosed();
}