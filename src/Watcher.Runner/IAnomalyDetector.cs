
namespace Watcher.Runner;

public interface IAnomalyDetector
{
    Task ForceRefreshCache();
    Task Initialize();
    Task RefreshCache();
    AnomalyResult? ScanChannel(ulong channelId);
}