using Watcher.Runner.Storage;

namespace Watcher.Runner;
public class AnomalyDetector(IMessagesStorage storage)
{
    private MessageInfo[]? messageInfos;
    private static readonly Lock obj = new ();
    private readonly TimeZoneInfo cestTimezone = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");

    public async Task ScanChannel(ulong channelId)
    {
        //todo
        //get statistics (standard activity per period, for example day of week, part of day etc)
        //monitor current activity
        //send event when current activity is higher than average

        var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, cestTimezone);
        var monthAgo = now.AddMonths(-1);

        var historicalData = await this.GetMessageInfos();
        var allChannelMessages = historicalData
            .Where(x => x.ChannelId == channelId)
            .Select(x => x.ChangeTimezone(cestTimezone));
        var lastMonth = allChannelMessages
            .Where(x => x.SentAt >= monthAgo)
            .ToArray();

        var messagesPerWeekDay = lastMonth
            .GroupBy(x => x.SentAt.DayOfWeek)
            .ToDictionary(x => x.Key, x => x.Count());

        var messagesInCurrentDay = lastMonth.Where(x => x.SentAt.Date == now.Date);

        await Task.CompletedTask;
    }

    private Task<MessageInfo[]> GetMessageInfos()
    {
        if (messageInfos == null)
        {
            lock (obj)
            {
                messageInfos ??= storage.GetAllMessagesInfos();
            }
        }
        
        return Task.FromResult(messageInfos);
    }
}
