namespace Watcher.Database.Entities;

public class ServerMessage
{
    public int Id { get; set; }
    public required ulong ServerId { get; set; }
    public required ulong ChannelId { get; set; }
    public required ulong UserId { get; set; }
    public required ulong MessageId { get; set; }
    public required string Content { get; set; }
    public required DateTime SentAtUtc { get; set; }
}