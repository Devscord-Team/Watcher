using Discord;
using Discord.WebSocket;

namespace Watcher.Runner.DiscordEventHandlers;

public class SlashCommandExecutedHandler : IDiscordEventHandler<SocketSlashCommand>
{
    //experiments
    public async Task Handle(SocketSlashCommand message)
    {
        var channel = message.Data.Options.First().Value as ISocketMessageChannel;

        var messages = await channel!.GetMessagesAsync(10, CacheMode.AllowDownload).FlattenAsync();
        var content = string.Join("\n", messages.Select(x => $"{x.Author.Username}: {x.Content}"));
        await message.RespondAsync(content);
    }
}
