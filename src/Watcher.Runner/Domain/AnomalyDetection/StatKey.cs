namespace Watcher.Runner.Domain.AnomalyDetection;

public record StatKey(ulong ChannelId, DayOfWeek DayOfWeek, TimeOnly TimeSlot);
