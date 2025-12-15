
namespace Watcher.Runner.Storage;

public interface IMessagesStorage
{
    MessageInfo[] GetAllMessagesInfos(ulong? serverId = null, ulong? channelId = null, DateTime? fromSentAtUtc = null);
    void SaveMessageInfo(MessageInfo message);
    void SaveMessagesInfos(IEnumerable<MessageInfo> messages);
}