using Newtonsoft.Json;

namespace Watcher.Runner.Storage;
public class MessagesStorage : IMessagesStorage
{
    private const string MESSAGES_INFO_HISTORY_PATH = "MessagesInfoHistory.json";

    public void SaveMessageInfo(MessageInfo message)
    {
        var json = JsonConvert.SerializeObject(message);
        var content = json + ",\r\n";

        File.AppendAllText(MESSAGES_INFO_HISTORY_PATH, content);
    }

    public MessageInfo[] GetAllMessagesInfos()
    {
        var jsonItems = File.ReadAllText(MESSAGES_INFO_HISTORY_PATH);

        var json = $"[{jsonItems}]";
        var result = JsonConvert.DeserializeObject<MessageInfo[]>(json)!;

        return result;
    }
}
