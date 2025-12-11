using Newtonsoft.Json;
using System.Text;

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

    public void SaveMessagesInfos(IEnumerable<MessageInfo> messages)
    {
        var fullText = new StringBuilder();
        foreach (var message in messages)
        {
            var json = JsonConvert.SerializeObject(message);
            var content = json + ",\r\n";
            fullText.Append(content);
        }

        File.AppendAllText(MESSAGES_INFO_HISTORY_PATH, fullText.ToString());
    }

    public MessageInfo[] GetAllMessagesInfos()
    {
        if (!File.Exists(MESSAGES_INFO_HISTORY_PATH))
        {
            return [];
        }

        var jsonItems = File.ReadAllText(MESSAGES_INFO_HISTORY_PATH);

        var json = $"[{jsonItems}]";
        var result = JsonConvert.DeserializeObject<MessageInfo[]>(json)!;

        return result;
    }
}
