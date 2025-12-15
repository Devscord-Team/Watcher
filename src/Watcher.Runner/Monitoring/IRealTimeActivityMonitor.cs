namespace Watcher.Runner.Monitoring;

public interface IRealTimeActivityMonitor
{
    void Initialize();
    MessageInfo[] GetHalfLastHourMessages(bool clearOutdatedData = true);
    int GetHalfLastHourMessagesCount(bool clearOutdatedData = true);
    MessageInfo[] GetLastHourMessages(bool clearOutdatedData = true);
    int GetLastHourMessagesCount(bool clearOutdatedData = true);
}