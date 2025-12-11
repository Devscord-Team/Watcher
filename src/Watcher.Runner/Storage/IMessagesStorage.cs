
namespace Watcher.Runner.Storage;

public interface IMessagesStorage
{
    MessageInfo[] GetAllMessagesInfos();
    void SaveMessageInfo(MessageInfo message);
    void SaveMessagesInfos(IEnumerable<MessageInfo> messages);
}