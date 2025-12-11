using Discord;

namespace Watcher.Runner.Extensions;

public static class MessageExtensions
{
    public static MessageInfo ToMessageInfo(this IMessage message)
    {
        var channel = (IGuildChannel)message.Channel;
        var author = message.Author;
        var messageInfo = new MessageInfo(channel.GuildId, channel.Id, author.Id, message.Id, message.Timestamp.DateTime);
        return messageInfo;
    }
}