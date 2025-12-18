namespace Watcher.Runner.Background
{
    public interface IBackground
    {
        Task ExecuteAsync(CancellationToken stoppingToken);
    }

    public class AnomalyChannelsScanner(IEventBus eventBus) : IBackground
    {
        public Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //eventBus.Subscribe<>
            return Task.CompletedTask;
        }
    }
}
