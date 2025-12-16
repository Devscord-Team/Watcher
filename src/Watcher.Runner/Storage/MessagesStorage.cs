using Microsoft.EntityFrameworkCore;
using Watcher.Database;
using Watcher.Database.Entities;

namespace Watcher.Runner.Storage;
public class MessagesStorage : IMessagesStorage
{
    public async Task SaveMessage(ServerMessage message)
    {
        using var databaseContext = new DatabaseContext();
        databaseContext.Messages.Add(message);
        databaseContext.SaveChanges();
    }

    public async Task SaveMessages(IEnumerable<ServerMessage> messages)
    {
        using var databaseContext = new DatabaseContext();
        foreach (var message in messages)
        {
            if (databaseContext.Messages.Any(x => x.MessageId == message.MessageId && x.ChannelId == message.ChannelId))
            {
                continue;
            }

            databaseContext.Messages.Add(message);
        }

        await databaseContext.SaveChangesAsync();
    }

    public async Task<MessageInfo[]> GetMessagesInfos(ulong? serverId = null, ulong? channelId = null, DateTime? fromSentAtUtc = null)
    {
        using var databaseContext = new DatabaseContext();
        if (!await databaseContext.Messages.AnyAsync())
        {
            return [];
        }

        var query = databaseContext.Messages.AsQueryable();

        if (serverId.HasValue)
        {
            query = query.Where(x => x.ServerId == serverId.Value);
        }

        if (channelId.HasValue)
        {
            query = query.Where(x => x.ChannelId == channelId.Value);
        }

        if (fromSentAtUtc.HasValue)
        {
            query = query.Where(x => x.SentAtUtc >= DateTime.SpecifyKind(fromSentAtUtc.Value, DateTimeKind.Utc));
        }

        var result = await query.ToArrayAsync();

        return [.. result.Select(MessageInfo.FromServerMessage)];
    }
}
