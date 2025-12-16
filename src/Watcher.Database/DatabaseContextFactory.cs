namespace Watcher.Database;

public class DatabaseContextFactory : IDatabaseContextFactory
{
    public IDatabaseContext Create() => new DatabaseContext();
}
