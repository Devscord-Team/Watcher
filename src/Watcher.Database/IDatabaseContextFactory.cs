namespace Watcher.Database
{
    public interface IDatabaseContextFactory
    {
        IDatabaseContext Create();
    }
}