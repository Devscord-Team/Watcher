using Watcher.Runner.Storage;

namespace Watcher.Runner;
public class AnomalyDetector(IMessagesStorage storage)
{
    public async Task ScanChannel(ulong channelId)
    {
        //todo
        //get statistics (standard activity per period, for example day of week, part of day etc)
        //monitor current activity
        //send event when current activity is higher than average
        await Task.CompletedTask;
    }
}
