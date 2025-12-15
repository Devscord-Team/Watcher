public record MessageInfo(ulong ServerId, ulong ChannelId, ulong UserId, ulong MessageId, DateTime SentAt)
{
    public MessageInfo ChangeTimezone(TimeZoneInfo destinationTimeZone)
        => this with
        {
            SentAt = TimeZoneInfo.ConvertTimeFromUtc(this.SentAt, destinationTimeZone)
        };
}