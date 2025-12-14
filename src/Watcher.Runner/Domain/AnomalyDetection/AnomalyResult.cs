namespace Watcher.Runner.Domain.AnomalyDetection;

public record AnomalyResult(
    ulong ChannelId,
    DateTime DetectedAt,
    int CurrentCount,
    double AverageCount)
{
    public double Multiplier => this.CurrentCount / Math.Max(this.AverageCount, 1);
}