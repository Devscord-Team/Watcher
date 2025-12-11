using Autofac;
using Discord;
using Discord.WebSocket;
using Watcher.Runner.DiscordEventHandlers;
using Watcher.Runner.Extensions;
using Watcher.Runner.Logging;
using Watcher.Runner.Storage;

namespace Watcher.Runner;
public class DiscordRunner(IComponentContext context, IMessagesStorage messagesStorage, IEventLogger eventLogger) : IDiscordRunner
{
    private bool started = false;
    private static readonly Lock obj = new ();

    private readonly DiscordSocketConfig config = new()
    {
        GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent
    };

    public async Task Run(string token)
    {
        lock(obj)
        {
            if (started)
            {
                return;
            }

            started = true;
        }

        var client = new DiscordSocketClient(config);
        this.ConfigureLog(client);
        this.ConfigureMessageReceived(client);
        this.ConfigureSlashCommandExecuted(client);
        this.ConfigureReady(client);

        await client.LoginAsync(TokenType.Bot, token);
        await client.StartAsync();
    }

    private void ConfigureLog(DiscordSocketClient client)
    {
        var handler = context.Resolve<IDiscordEventHandler<LogMessage>>();
        client.Log += handler.Handle;
    }

    private void ConfigureMessageReceived(DiscordSocketClient client)
    {
        var handler = context.Resolve<IDiscordEventHandler<SocketMessage>>();
        client.MessageReceived += handler.Handle;
    }

    private void ConfigureSlashCommandExecuted(DiscordSocketClient client)
    {
        var handler = context.Resolve<IDiscordEventHandler<SocketSlashCommand>>();
        client.SlashCommandExecuted += handler.Handle;
    }

    private void ConfigureReady(DiscordSocketClient client)
    {
        var commands = this.BuildCommands();

        client.Ready += async () =>
        {
            foreach (var guild in client.Guilds.Where(x => x.Name == "Devscord"))
            {
                var guildCommands = await guild.GetApplicationCommandsAsync();
                if (guildCommands?.Count > 0)
                {
                    _ = await guild.BulkOverwriteApplicationCommandAsync(commands);
                }
                else
                {
                    foreach (var command in commands)
                    {
                        _ = await guild.CreateApplicationCommandAsync(command);
                    }
                }

                _ = Task.Run(async () => 
                {
                    var savedMessages = messagesStorage.GetAllMessagesInfos();
                    var botUser = guild.CurrentUser;

                    foreach (var channel in guild.TextChannels)
                    {
                        var permissions = botUser.GetPermissions(channel);
                        if (!permissions.ViewChannel || !permissions.ReadMessageHistory)
                        {
                            continue;
                        }

                        ulong? lastMessageId = null;
                        while (true)
                        {
                            var batch = lastMessageId.HasValue
                                ? await channel.GetMessagesAsync(lastMessageId.Value, Direction.Before, 100).FlattenAsync()
                                : await channel.GetMessagesAsync(100).FlattenAsync();

                            if (!batch.Any())
                            {
                                break;
                            }

                            var toSave = batch
                                .Where(x => !savedMessages.Any(s => s.MessageId == x.Id))
                                .Select(x => x.ToMessageInfo());

                            messagesStorage.SaveMessagesInfos(toSave);
                            eventLogger.Event_SavedMessagesInfos(guild.Id, channel.Id, toSave.Count());

                            await Task.Delay(500);
                            lastMessageId = batch.Last().Id;
                        }
                    }
                });
            }
        };
    }

    private ApplicationCommandProperties[] BuildCommands()
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

        return [notificationsCommand];
    }
}
