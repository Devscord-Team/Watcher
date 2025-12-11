using Autofac;
using Watcher.Runner;

//todo try, catch, log
var container = ContainersFactory.Create();
var initializer = container.Resolve<IDiscordInitializer>();

var token = Environment.GetEnvironmentVariable("WATCHER_DISCORD_TOKEN") 
    ?? throw new ArgumentNullException("Discord token is empty. Set WATCHER_DISCORD_TOKEN environment variable.");

await initializer.Initialize(token);
await Task.Delay(-1);