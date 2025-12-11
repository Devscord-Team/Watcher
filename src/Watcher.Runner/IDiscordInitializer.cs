
namespace Watcher.Runner;

public interface IDiscordInitializer
{
    Task Initialize(string token);
}