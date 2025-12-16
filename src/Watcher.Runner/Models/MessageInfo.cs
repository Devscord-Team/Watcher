using Watcher.Database.Entities;

public record MessageInfo(ulong ServerId, ulong ChannelId, ulong UserId, ulong MessageId, DateTime SentAt)
{
    public int DatabaseId { get; private set; }

    public MessageInfo ChangeTimezone(TimeZoneInfo destinationTimeZone)
        => this with
        {
            SentAt = TimeZoneInfo.ConvertTimeFromUtc(this.SentAt, destinationTimeZone)
        };

    public static MessageInfo FromServerMessage(ServerMessage serverMessage) 
        => new(
            serverMessage.ServerId,
            serverMessage.ChannelId,
            serverMessage.UserId,
            serverMessage.MessageId,
            serverMessage.SentAtUtc)
        {
            DatabaseId = serverMessage.Id 
        };
}