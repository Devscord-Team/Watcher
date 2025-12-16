namespace Watcher.Runner.Monitoring;

public interface IRealTimeActivityMonitor
{
    Task Initialize();
    MessageInfo[] GetHalfLastHourMessages(bool clearOutdatedData = true);
    int GetHalfLastHourMessagesCount(bool clearOutdatedData = true);
    MessageInfo[] GetLastHourMessages(bool clearOutdatedData = true);
    int GetLastHourMessagesCount(bool clearOutdatedData = true);
}