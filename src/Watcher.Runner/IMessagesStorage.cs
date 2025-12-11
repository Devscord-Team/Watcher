namespace Watcher.Runner;

public interface IMessagesStorage
{
    MessageInfo[] GetAllMessagesInfos();
    void SaveMessageInfo(MessageInfo message);
}