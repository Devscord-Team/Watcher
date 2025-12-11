using Discord.WebSocket;

namespace Watcher.Runner;
public class MessageReceivedHandler : IDiscordEventHandler<SocketMessage>
{
    public async Task Handle(SocketMessage message)
    {
        await Task.CompletedTask;
    }
}
