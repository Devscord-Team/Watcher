using Newtonsoft.Json;
using System.Text;

namespace Watcher.Runner.Storage;
public class MessagesStorage : IMessagesStorage
{
    /// <summary>
    /// json is temporary
    /// todo: add database
    /// </summary>
    private const string MESSAGES_INFO_HISTORY_PATH = "MessagesInfoHistory.json";
    private static readonly Lock obj = new ();

    public void SaveMessageInfo(MessageInfo message)
    {
        var json = JsonConvert.SerializeObject(message);
        var content = json + ",\r\n";

        lock (obj)
        {
            File.AppendAllText(MESSAGES_INFO_HISTORY_PATH, content);
        }
    }

    public void SaveMessagesInfos(IEnumerable<MessageInfo> messages)
    {
        var fullText = new StringBuilder();
        foreach (var message in messages)
        {
            var json = JsonConvert.SerializeObject(message);
            var content = json + ",\r\n";
            fullText.Append(content);
        }

        lock (obj)
        {
            File.AppendAllText(MESSAGES_INFO_HISTORY_PATH, fullText.ToString());
        }
    }

    public MessageInfo[] GetAllMessagesInfos(ulong? serverId = null, ulong? channelId = null, DateTime? fromSentAtUtc = null)
    {
        if (!File.Exists(MESSAGES_INFO_HISTORY_PATH))
        {
            return [];
        }

        var jsonItems = File.ReadAllText(MESSAGES_INFO_HISTORY_PATH);

        var json = $"[{jsonItems}]";
        var result = JsonConvert.DeserializeObject<IEnumerable<MessageInfo>>(json)!;

        if (serverId.HasValue)
        {
            result = result.Where(x => x.ServerId == serverId.Value);
        }

        if (channelId.HasValue)
        {
            result = result.Where(x => x.ChannelId == channelId.Value);
        }

        if (fromSentAtUtc.HasValue)
        {
            result = result.Where(x => x.SentAt >= fromSentAtUtc.Value);
        }

        return result.ToArray();
    }
}
