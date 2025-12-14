namespace Watcher.Runner.Domain.AnomalyDetection;

public interface IAnomalyDetector
{
    Task ForceRefreshCache();
    Task Initialize();
    Task RefreshCache();
    AnomalyResult? ScanChannel(ulong channelId);
}