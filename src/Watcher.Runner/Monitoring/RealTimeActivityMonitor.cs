using System.Collections.Concurrent;

namespace Watcher.Runner.Monitoring;
public class RealTimeActivityMonitor
{
    private ConcurrentBag<MessageInfo> lastHourMessages = new();

    public void Initialize()
    {

    }
}
