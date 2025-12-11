using Discord.WebSocket;
using Newtonsoft.Json;

namespace Watcher.Runner;
public class MessageReceivedHandler(IEventLogger eventLogger) : IDiscordEventHandler<SocketMessage>
{
    public async Task Handle(SocketMessage message)
    {
        var statMessageInfo = message.ToStatMessageInfo();
        eventLogger.Event_ReceivedMessage(statMessageInfo);

        var json = JsonConvert.SerializeObject(statMessageInfo);
        var content = json + "\r\n";

        File.AppendAllText($"MessagesHistory.json", content);
        await Task.CompletedTask;
    }
}
