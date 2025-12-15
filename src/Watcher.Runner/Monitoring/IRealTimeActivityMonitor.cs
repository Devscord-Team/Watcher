namespace Watcher.Runner.Monitoring;

public interface IRealTimeActivityMonitor
{
    MessageInfo[] GetHalfLastHourMessages();
    int GetHalfLastHourMessagesCount();
    MessageInfo[] GetLastHourMessages();
    int GetLastHourMessagesCount();
    void Initialize();
}