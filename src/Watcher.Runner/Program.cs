using Discord;
using Discord.WebSocket;

var token = Environment.GetEnvironmentVariable("WATCHER_DISCORD_TOKEN");
if (token is null)
{
    Console.WriteLine("Discord token is empty. Set WATCHER_DISCORD_TOKEN environment variable.");
    return;
}

var config = new DiscordSocketConfig
{
    GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent
};

var client = new DiscordSocketClient(config);

client.Log += (x) =>
{
    Console.WriteLine(x.ToString());
    return Task.CompletedTask;
};

client.MessageReceived += (message) =>
{
    Console.WriteLine(message.Content);
    return Task.CompletedTask;
};

client.Ready += async () =>
{
    var notificationsCommand = new SlashCommandBuilder()
        .WithName("notify-me")
        .WithDescription("Wysyła jedno powiadomienie kiedy wykryje zwiększony ruch.")
        .AddOptions(
        [
            new SlashCommandOptionBuilder()
                .WithName("channel")
                .WithDescription("Kanał do obserwowania.")
                .WithType(ApplicationCommandOptionType.Channel)
                .WithRequired(true)
        ])
        .Build();

    await client.BulkOverwriteGlobalApplicationCommandsAsync([notificationsCommand]);

    Console.WriteLine("Commands registered!");
};

await client.LoginAsync(TokenType.Bot, token);
await client.StartAsync();

await Task.Delay(-1);