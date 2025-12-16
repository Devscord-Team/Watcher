using Watcher.Runner.Logging;
using Watcher.Runner.Providers;
using Watcher.Runner.Storage;

namespace Watcher.Runner.Domain.AnomalyDetection;

public class AnomalyDetector(IMessagesStorage storage, IEventLogger eventLogger, IDateTimeProvider dateTimeProvider) : IAnomalyDetector
{
    private Dictionary<StatKey, ChannelStats>? statsCache;
    private DateTime? lastRefreshTime;

    private readonly Lock cacheLock = new();
    private bool isReloadingCache = false;

    private const int WINDOW_MINUTES = 240;
    private const int HISTORICAL_WEEKS = 4;
    private const int MIN_HISTORICAL_WEEKS = 1;
    private const double ANOMALY_THRESHOLD = 2.0;
    private const int MIN_MESSAGES_FOR_ANOMALY = 5;

    public async Task Initialize() => await this.RefreshCache();

    //todo - figure out how refresh should work, without multi downloads in same time
    public async Task RefreshCache()
    {
        if (isReloadingCache || this.lastRefreshTime.HasValue && this.lastRefreshTime.Value > dateTimeProvider.GetUtcNow().AddHours(-1))
        {
            return;
        }

        lock (this.cacheLock)
        {
            if (isReloadingCache)
            {
                return;
            }

            isReloadingCache = true;
        }
        
        eventLogger.Event_AnomalyDetectorCacheRefreshStarted();
        var now = dateTimeProvider.GetUtcNow();
        var freshMessages = await storage.GetMessagesInfos(fromSentAtUtc: now.Date.AddDays((-7 * HISTORICAL_WEEKS) - 1));

        lock (this.cacheLock)
        {
            if (!isReloadingCache)
            {
                return;
            }

            var messagesByChan = freshMessages
                .GroupBy(m => m.ChannelId)
                .ToDictionary(g => g.Key, g => g.ToArray());

            var newStatsCache = messagesByChan
                .SelectMany(kvp => this.BuildCacheEntriesForChannel(kvp.Key, kvp.Value, now))
                .Where(entry => entry.Stats != null)
                .ToDictionary(entry => entry.Key, entry => entry.Stats!);

            this.statsCache = newStatsCache;
            this.lastRefreshTime = dateTimeProvider.GetUtcNow();

            eventLogger.Event_AnomalyDetectorCacheRefreshFinished();
            isReloadingCache = false;
        }
    }

    public async Task<AnomalyResult?> ScanChannel(ulong channelId)
    {
        eventLogger.Event_AnomalyDetectorScanChannelStarted(channelId);

        var now = dateTimeProvider.GetUtcNow();
        var timeSlot = this.RoundToTimeSlot(now);
        var key = new StatKey(channelId, now.DayOfWeek, timeSlot);

        var stats = this.GetStatsFromCache(key);
        if (stats == null)
        {
            return null;
        }

        if (stats.WeeksOfData < MIN_HISTORICAL_WEEKS)
        {
            return null;
        }

        var currentCount = await this.GetCurrentMessageCount(channelId, now);

        var result = this.IsAnomaly(currentCount, stats)
            ? new AnomalyResult(channelId, now, currentCount, stats.Average)
            : null;

        eventLogger.Event_AnomalyDetectorScanChannelFinished(channelId, result);
        return result;
    }

    public async Task ForceRefreshCache()
    {
        lock (this.cacheLock)
        {
            this.lastRefreshTime = null;
        }

        await this.RefreshCache();
    }

    private IEnumerable<(StatKey Key, ChannelStats? Stats)> BuildCacheEntriesForChannel(
        ulong channelId,
        MessageInfo[] channelMessages,
        DateTime now)
    {
        return GetAllTimeSlots()
            .Select(slot => (
                Key: new StatKey(channelId, slot.Day, slot.Time),
                Stats: this.CalculateStatsForSlot(channelMessages, slot.Day, slot.Time, now)
            ));
    }

    private Task<int> GetCurrentMessageCount(ulong channelId, DateTime now)
    {
        var windowStart = now.AddMinutes(-WINDOW_MINUTES);
        return this.GetChannelMessages(channelId, windowStart);
    }

    private ChannelStats? GetStatsFromCache(StatKey key)
    {
        lock (this.cacheLock)
        {
            return this.statsCache == null
                ? throw new InvalidOperationException("Cache is not initialized. Call Initialize() first.")
                : this.statsCache.GetValueOrDefault(key);
        }
    }

    private bool IsAnomaly(int currentCount, ChannelStats stats)
    {
        return stats.Average > 0 && currentCount >= stats.Average * ANOMALY_THRESHOLD
            && currentCount >= MIN_MESSAGES_FOR_ANOMALY;
    }

    private ChannelStats? CalculateStatsForSlot(
        MessageInfo[] channelMessages,
        DayOfWeek dayOfWeek,
        TimeOnly timeSlot,
        DateTime now)
    {
        var counts = Enumerable.Range(1, HISTORICAL_WEEKS)
            .Select(weekAgo => this.GetMessageCountForHistoricalSlot(
                channelMessages, dayOfWeek, timeSlot, now, weekAgo))
            .ToArray();

        var weeksWithData = counts.Count(c => c > 0);

        if (weeksWithData == 0)
        {
            return null;
        }

        // todo get oldest week
        // if oldest week is newer than HISTORICAL_WEEKS, then divide by number of weeks in channel
        var average = counts.Average();

        return new ChannelStats(average, weeksWithData);
    }

    private int GetMessageCountForHistoricalSlot(
        MessageInfo[] channelMessages,
        DayOfWeek dayOfWeek,
        TimeOnly timeSlot,
        DateTime referenceDate,
        int weeksAgo)
    {
        var targetDate = this.FindPreviousWeekday(referenceDate.Date, dayOfWeek, weeksAgo);

        if (channelMessages.Length == 0)
        {
            return 0;
        }

        var oldestMessage = channelMessages.Min(m => m.SentAt);
        if (targetDate < oldestMessage.Date)
        {
            return 0;
        }

        var slotStart = targetDate.Add(timeSlot.ToTimeSpan()).AddMinutes(-WINDOW_MINUTES);
        var slotEnd = targetDate.Add(timeSlot.ToTimeSpan());

        return channelMessages.Count(m => m.SentAt >= slotStart && m.SentAt <= slotEnd);
    }

    private DateTime FindPreviousWeekday(DateTime startDate, DayOfWeek targetDay, int weeksAgo)
    {
        var daysBack = ((int)startDate.DayOfWeek - (int)targetDay + 7) % 7;

        if (daysBack == 0)
        {
            daysBack = 7;
        }

        return startDate.AddDays(-(daysBack + 7 * (weeksAgo - 1)));
    }

    private static IEnumerable<(DayOfWeek Day, TimeOnly Time)> GetAllTimeSlots()
    {
        var days = Enum.GetValues<DayOfWeek>();
        var slots = new List<(DayOfWeek, TimeOnly)>();

        const int minutesPerDay = 24 * 60;
        for (int minutes = 0; minutes < minutesPerDay; minutes += WINDOW_MINUTES)
        {
            var hour = minutes / 60;
            var minute = minutes % 60;
            var time = new TimeOnly(hour, minute);

            foreach (DayOfWeek day in days)
            {
                slots.Add((day, time));
            }
        }

        return slots;
    }

    private TimeOnly RoundToTimeSlot(DateTime dateTime)
    {
        var totalMinutes = dateTime.Hour * 60 + dateTime.Minute;
        var slotStartMinutes = (totalMinutes / WINDOW_MINUTES) * WINDOW_MINUTES;
        var hour = slotStartMinutes / 60;
        var minute = slotStartMinutes % 60;
        return new TimeOnly(hour, minute);
    }


    private async Task<int> GetChannelMessages(ulong channelId, DateTime startDate)
    {
        var freshMessages = await storage.GetMessagesInfos(channelId: channelId, fromSentAtUtc: startDate.ToUniversalTime());
        return freshMessages.Count();
    }
}
