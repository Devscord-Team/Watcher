using Watcher.Runner.DiscordEventHandlers;
using Watcher.Runner.Providers;
using Watcher.Runner.Storage;

namespace Watcher.Runner.Monitoring;
public class RealTimeActivityMonitor(IEventBus eventBus, IDateTimeProvider dateTimeProvider, IMessagesStorage messagesStorage) : IRealTimeActivityMonitor, IDisposable
{
    private List<MessageInfo> lastHourMessages = [];
    private readonly Lock obj = new();
    private IDisposable? messageInfoReceivedEventSubscription;

    public void Initialize()
    {
        this.lastHourMessages = [.. messagesStorage.GetAllMessagesInfos(fromSentAtUtc: dateTimeProvider.GetUtcNow().AddHours(-1))];

        this.messageInfoReceivedEventSubscription = eventBus.Subscribe<MessageInfoReceivedEvent>(x =>
        {
            lock (this.obj)
            {
                this.lastHourMessages.Add(x.MessageInfo);
            }
        });
    }

    public MessageInfo[] GetLastHourMessages(bool clearOutdatedData = true)
    {
        if (clearOutdatedData)
        {
            this.ClearOutdatedData(dateTimeProvider.GetUtcNow().AddHours(-1));
        }

        return [.. this.lastHourMessages];
    }

    public int GetLastHourMessagesCount(bool clearOutdatedData = true)
    {
        if (clearOutdatedData)
        {
            this.ClearOutdatedData(dateTimeProvider.GetUtcNow().AddHours(-1));
        }

        return this.lastHourMessages.Count;
    }

    public MessageInfo[] GetHalfLastHourMessages(bool clearOutdatedData = true)
    {
        if (clearOutdatedData)
        {
            this.ClearOutdatedData(dateTimeProvider.GetUtcNow().AddHours(-1));
        }

        var halfHour = this.lastHourMessages.Where(x => x.SentAt > dateTimeProvider.GetUtcNow().AddMinutes(-30));
        return [.. halfHour];
    }

    public int GetHalfLastHourMessagesCount(bool clearOutdatedData = true)
    {
        if (clearOutdatedData)
        {
            this.ClearOutdatedData(dateTimeProvider.GetUtcNow().AddHours(-1));
        }

        var halfHour = this.lastHourMessages.Where(x => x.SentAt > dateTimeProvider.GetUtcNow().AddMinutes(-30));
        return halfHour.Count();
    }

    private void ClearOutdatedData(DateTime from)
    {
        lock (this.obj)
        {
            this.lastHourMessages = [.. this.lastHourMessages.Where(x => x.SentAt > from)];
        }
    }

    public void Dispose() => this.messageInfoReceivedEventSubscription?.Dispose();
}
