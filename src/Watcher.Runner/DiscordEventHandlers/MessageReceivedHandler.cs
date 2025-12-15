using Discord.WebSocket;
using Watcher.Runner.Extensions;
using Watcher.Runner.Logging;
using Watcher.Runner.Monitoring;
using Watcher.Runner.Storage;

namespace Watcher.Runner.DiscordEventHandlers;
public class MessageReceivedHandler(IEventLogger eventLogger, IMessagesStorage messagesStorage, IEventBus eventBus, IRealTimeActivityMonitor realTimeActivityMonitor) : IDiscordEventHandler<SocketMessage>
{
    private const string MARCHEW_MESSAGE = "Moim zdaniem to nie ma tak, że coś jest programowaniem, albo nie jest programowaniem. Gdybym miał powiedzieć, co cenię w programowaniu najbardziej, powiedziałbym, że ludzi. Ekhm… Ludzi, którzy podali mi pomocną dokumentacje, kiedy sobie nie radziłem, kiedy byłem sam. I co ciekawe, to właśnie przypadkowe spotkania wpływają na nasze życie. Chodzi o to, że kiedy wyznaje się pewne wartości, nawet pozornie uniwersalne, bywa, że nie znajduje się zrozumienia, które by tak rzec, które pomaga się nam rozwijać. Ja miałem szczęście, by tak rzec, ponieważ je znalazłem. I dziękuję życiu. Dziękuję mu, życie to śpiew, życie to taniec, życie to miłość. Wielu ludzi pyta mnie o to samo, ale jak ty to robisz? Skąd czerpiesz tę radość? A ja odpowiadam, że to proste, to umiłowanie życia, to właśnie ono sprawia, że dzisiaj na przykład programuje nawigacje, a jutro… kto wie, dlaczego by nie, oddam się pracy społecznej i będę ot, choćby uczyć… znaczy… juniorów.";

    public async Task Handle(SocketMessage message)
    {
        var messageInfo = message.ToMessageInfo();

        eventLogger.Event_ReceivedMessage(messageInfo);
        messagesStorage.SaveMessageInfo(messageInfo);
        eventBus.Publish(new MessageInfoReceivedEvent(messageInfo));

        var normalized = message.Content.Trim().ToLower();
        if (normalized is "-marchew" or "!marchew")
        {
            _ = await message.Channel.SendMessageAsync(MARCHEW_MESSAGE);
        }

        if (normalized is "-stats")
        {
            string getContent()
            {
                var lastHour = realTimeActivityMonitor.GetLastHourMessagesCount(false);
                var lastHalfHour = realTimeActivityMonitor.GetHalfLastHourMessagesCount(true);

                return $"Statystyki wiadomości:\r\nOstatnia godzina: {lastHour}\r\nOstatnie pół godziny: {lastHalfHour}";
            }

            var content = getContent();
            var sentMessage = await message.Channel.SendMessageAsync(content);
            _ = Task.Run(async () =>
            {
                while (true)
                {
                    var newContent = getContent();
                    if (newContent != content)
                    {
                        await sentMessage.ModifyAsync(x => x.Content = getContent());
                        content = newContent;
                    }

                    await Task.Delay(1000);
                }
            });
        }
    }
}

public record MessageInfoReceivedEvent(MessageInfo MessageInfo) : IEvent;