using Watcher.Runner.DiscordEventHandlers;
using Watcher.Runner.Providers;
using Watcher.Runner.Storage;

namespace Watcher.Runner.Monitoring;
public class RealTimeActivityMonitor(IEventBus eventBus, IDateTimeProvider dateTimeProvider, IMessagesStorage messagesStorage) : IRealTimeActivityMonitor
{
    private List<MessageInfo> lastHourMessages = new();
    private readonly Lock obj = new();

    public void Initialize()
    {
        this.lastHourMessages = [.. messagesStorage
            .GetAllMessagesInfos()
            .Where(x => x.SentAt > dateTimeProvider.GetUtcNow().AddHours(-1))];

        eventBus.Subscribe<MessageInfoReceivedEvent>(x =>
        {
            lock (obj)
            {
                this.lastHourMessages.Add(x.MessageInfo);
            }
        });
    }

    public MessageInfo[] GetLastHourMessages()
    {
        this.ClearOutdatedData(dateTimeProvider.GetUtcNow().AddHours(-1));
        return [.. lastHourMessages];
    }

    public int GetLastHourMessagesCount()
    {
        this.ClearOutdatedData(dateTimeProvider.GetUtcNow().AddHours(-1));
        return this.lastHourMessages.Count;
    }

    public MessageInfo[] GetHalfLastHourMessages()
    {
        this.ClearOutdatedData(dateTimeProvider.GetUtcNow().AddHours(-1));
        var halfHour = lastHourMessages.Where(x => x.SentAt > dateTimeProvider.GetUtcNow().AddMinutes(-30));
        return [.. halfHour];
    }

    public int GetHalfLastHourMessagesCount()
    {
        this.ClearOutdatedData(dateTimeProvider.GetUtcNow().AddHours(-1));
        var halfHour = lastHourMessages.Where(x => x.SentAt > dateTimeProvider.GetUtcNow().AddMinutes(-30));
        return halfHour.Count();
    }

    private void ClearOutdatedData(DateTime from)
    {
        lock (obj)
        {
            lastHourMessages = [.. lastHourMessages.Where(x => x.SentAt > from)];
        }
    }
}
