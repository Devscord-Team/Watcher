using Discord;
using Watcher.Database.Entities;

namespace Watcher.Runner.Extensions;

public static class MessageExtensions
{
    public static MessageInfo ToMessageInfo(this IMessage message)
    {
        var channel = (IGuildChannel)message.Channel;
        var author = message.Author;
        var messageInfo = new MessageInfo(channel.GuildId, channel.Id, author.Id, message.Id, message.Timestamp.UtcDateTime);
        return messageInfo;
    }

    public static ServerMessage ToServerMessage(this IMessage message)
    {
        var channel = (IGuildChannel)message.Channel;
        var author = message.Author;

        var result = new ServerMessage()
        {
            ServerId = channel.GuildId,
            ChannelId = channel.Id,
            UserId = author.Id,
            MessageId = message.Id,
            Content = message.Content,
            SentAtUtc = message.Timestamp.UtcDateTime
        };

        return result;
    }
}