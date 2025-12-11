using Discord;
using Discord.WebSocket;

namespace Watcher.Runner;

public static class SocketMessageExtensions
{
    public static StatMessageInfo ToStatMessageInfo(this SocketMessage message)
    {
        var channel = (IGuildChannel)message.Channel;
        var author = message.Author;
        var statMessageInfo = new StatMessageInfo(channel.GuildId, channel.Id, author.Id, message.Id, message.Timestamp.DateTime);
        return statMessageInfo;
    }
}