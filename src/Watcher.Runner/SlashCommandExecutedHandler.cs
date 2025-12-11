using Discord.WebSocket;

namespace Watcher.Runner;

public class SlashCommandExecutedHandler : IDiscordEventHandler<SocketSlashCommand>
{
    public async Task Handle(SocketSlashCommand message)
    {
        await Task.CompletedTask;
    }
}
