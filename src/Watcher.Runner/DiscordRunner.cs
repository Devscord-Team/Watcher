using Discord;
using Discord.WebSocket;

namespace Watcher.Runner;
public class DiscordRunner(IEventLogger eventLogger) : IDiscordRunner
{
    private bool started = false;
    private static readonly Lock obj = new ();

    private readonly DiscordSocketConfig config = new()
    {
        GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent
    };

    public async Task Run(string token)
    {
        if (started)
        {
            return;
        }

        lock(obj)
        {
            started = true;
        }

        var client = new DiscordSocketClient(config);
        this.ConfigureLog(client);
        this.ConfigureSlashCommandExecuted(client);
        this.ConfigureReady(client);

        await client.LoginAsync(TokenType.Bot, token);
        await client.StartAsync();
    }

    private void ConfigureLog(DiscordSocketClient client)
    {
        client.Log += (x) =>
        {
            eventLogger.LogMessage(x);
            return Task.CompletedTask;
        };
    }

    private void ConfigureSlashCommandExecuted(DiscordSocketClient client)
    {
        client.SlashCommandExecuted += (message) =>
        {
            var options = message.Data.Options.Select(x => $"{x.Name} {x.Value}").ToArray();
            Console.WriteLine(message.CommandName);
            var response = $"Saved. {message.CommandName} | {string.Join(", ", options)}";
            Console.WriteLine(response);
            return message.RespondAsync(response);
        };
    }

    private void ConfigureReady(DiscordSocketClient client)
    {
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
        };
    }
}
