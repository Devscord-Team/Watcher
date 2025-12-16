using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using Watcher.Runner.Domain.AnomalyDetection;

namespace Watcher.Runner.DiscordEventHandlers;

public class SlashCommandExecutedHandler(IAnomalyDetector anomalyDetector) : IDiscordEventHandler<SocketSlashCommand>
{
    public async Task Handle(SocketSlashCommand message)
    {
        var channel = message.Data.Options.First().Value as ISocketMessageChannel;
        var scanResult = await anomalyDetector.ScanChannel(channel!.Id);

        var response = scanResult != null ? $"```json\r\n{JsonConvert.SerializeObject(scanResult, Formatting.Indented)}\r\n```" : "Nie wykryto anomalii na wskazanym kanale.";

        await message.RespondAsync(response);
    }
}
