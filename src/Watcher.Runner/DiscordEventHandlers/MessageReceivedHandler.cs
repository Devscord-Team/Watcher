using Discord.WebSocket;
using Watcher.Runner.Extensions;
using Watcher.Runner.Logging;
using Watcher.Runner.Storage;

namespace Watcher.Runner.DiscordEventHandlers;
public class MessageReceivedHandler(IEventLogger eventLogger, IMessagesStorage messagesStorage) : IDiscordEventHandler<SocketMessage>
{
    public async Task Handle(SocketMessage message)
    {
        var messageInfo = message.ToMessageInfo();

        eventLogger.Event_ReceivedMessage(messageInfo);
        messagesStorage.SaveMessageInfo(messageInfo);

        await Task.CompletedTask;
    }
}
