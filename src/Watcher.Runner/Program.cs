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

client.SlashCommandExecuted += (message) =>
{
    var options = message.Data.Options.Select(x => $"{x.Name} {x.Value}").ToArray();
    Console.WriteLine(message.CommandName);
    var response = $"Saved. {message.CommandName} | {string.Join(", ", options)}";
    Console.WriteLine(response);
    return message.RespondAsync(response);
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

    foreach (var command in await client.GetGlobalApplicationCommandsAsync())
    {
        await command.DeleteAsync();
    }

    foreach (var guild in client.Guilds)
    {
        if (guild.Name != "Devscord")
        {
            continue;
        }

        var commands = await guild.GetApplicationCommandsAsync();
        if (commands?.Count > 0)
        {
            _ = await guild.BulkOverwriteApplicationCommandAsync([notificationsCommand]);
        }
        else
        {
            _ = await guild.CreateApplicationCommandAsync(notificationsCommand);
        }
    }

    Console.WriteLine("Commands registered!");
};

await client.LoginAsync(TokenType.Bot, token);
await client.StartAsync();

await Task.Delay(-1);