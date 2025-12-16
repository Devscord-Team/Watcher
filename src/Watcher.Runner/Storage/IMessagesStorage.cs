
using Watcher.Database.Entities;

namespace Watcher.Runner.Storage;

public interface IMessagesStorage
{
    Task<MessageInfo[]> GetMessagesInfos(ulong? serverId = null, ulong? channelId = null, DateTime? fromSentAtUtc = null);
    Task SaveMessage(ServerMessage message);
    Task SaveMessages(IEnumerable<ServerMessage> messages);
}