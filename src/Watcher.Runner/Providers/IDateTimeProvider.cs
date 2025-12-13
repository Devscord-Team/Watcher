
namespace Watcher.Runner.Providers;

public interface IDateTimeProvider
{
    DateTime GetUtcNow();
}