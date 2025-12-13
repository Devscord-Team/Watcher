using Watcher.Runner.Logging;
using Watcher.Runner.Providers;
using Watcher.Runner.Storage;

namespace Watcher.Runner;

public class AnomalyDetector(IMessagesStorage storage, IEventLogger eventLogger, IDateTimeProvider dateTimeProvider) : IAnomalyDetector
{
    private Dictionary<StatKey, ChannelStats>? statsCache;
    private DateTime? lastRefreshTime;

    private readonly Lock cacheLock = new();

    private readonly TimeZoneInfo cestTimezone = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");

    private const int WINDOW_MINUTES = 30;
    private const int HISTORICAL_WEEKS = 4;
    private const int MIN_HISTORICAL_WEEKS = 1;
    private const double ANOMALY_THRESHOLD = 2.0;
    private const int MIN_MESSAGES_FOR_ANOMALY = 5;

    public async Task Initialize() => await this.RefreshCache();

    public async Task RefreshCache()
    {
        lock (this.cacheLock)
        {
            if (lastRefreshTime.HasValue && lastRefreshTime.Value > dateTimeProvider.GetUtcNow().AddHours(-1))
            {
                return;
            }

            eventLogger.Event_AnomalyDetectorCacheRefreshStarted();

            var freshMessages = storage.GetAllMessagesInfos();
            var now = this.GetCurrentTime();

            var messagesByChan = freshMessages
                .Select(m => m.ChangeTimezone(this.cestTimezone))
                .GroupBy(m => m.ChannelId)
                .ToDictionary(g => g.Key, g => g.ToArray());

            var newStatsCache = messagesByChan
                .SelectMany(kvp => this.BuildCacheEntriesForChannel(kvp.Key, kvp.Value, now))
                .Where(entry => entry.Stats != null)
                .ToDictionary(entry => entry.Key, entry => entry.Stats!);

            this.statsCache = newStatsCache;
            this.lastRefreshTime = dateTimeProvider.GetUtcNow();


            eventLogger.Event_AnomalyDetectorCacheRefreshFinished();
        }

        await Task.CompletedTask;
    }

    public AnomalyResult? ScanChannel(ulong channelId)
    {
        eventLogger.Event_AnomalyDetectorScanChannelStarted(channelId);

        var now = this.GetCurrentTime();
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

        var currentCount = this.GetCurrentMessageCount(channelId, now);

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

    private int GetCurrentMessageCount(ulong channelId, DateTime now)
    {
        var windowStart = now.AddMinutes(-WINDOW_MINUTES);

        var channelMessagesCount = this.GetChannelMessages(channelId, windowStart);
        return channelMessagesCount;
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
        if (stats.Average <= 0)
        {
            return false;
        }

        return currentCount >= stats.Average * ANOMALY_THRESHOLD
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

        var average = counts.Where(c => c > 0).DefaultIfEmpty(0).Average();

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
        var times = Enumerable.Range(0, 48).Select(i => new TimeOnly(i / 2, i % 2 * 30));

        return days.SelectMany(day => times.Select(time => (day, time)));
    }

    private TimeOnly RoundToTimeSlot(DateTime dateTime)
    {
        var minute = dateTime.Minute < 30 ? 0 : 30;
        return new TimeOnly(dateTime.Hour, minute);
    }

    private DateTime GetCurrentTime()
        => TimeZoneInfo.ConvertTimeFromUtc(dateTimeProvider.GetUtcNow(), this.cestTimezone);

    private int GetChannelMessages(ulong channelId, DateTime startDate)
    {
        var freshMessages = storage.GetAllMessagesInfos();
        var channelMessages = freshMessages
            .Where(x => x.ChannelId == channelId)
            .Select(m => m.ChangeTimezone(this.cestTimezone))
            .Where(x => x.SentAt >= startDate);

        return channelMessages.Count();
    }
}

public record StatKey(ulong ChannelId, DayOfWeek DayOfWeek, TimeOnly TimeSlot);

public record ChannelStats(double Average, int WeeksOfData);

public record AnomalyResult(
    ulong ChannelId,
    DateTime DetectedAt,
    int CurrentCount,
    double AverageCount)
{
    public double Multiplier => this.CurrentCount / Math.Max(this.AverageCount, 1);
}