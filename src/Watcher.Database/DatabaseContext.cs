using Microsoft.EntityFrameworkCore;
using Watcher.Database.Entities;

namespace Watcher.Database;

public class DatabaseContext : DbContext
{
    public DbSet<ServerMessage> Messages { get; set; }

    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
    {
    }

    public DatabaseContext()
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseNpgsql($"Host=localhost;Username=postgres;Database=postgres");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        _ = modelBuilder.Entity<ServerMessage>()
            .HasKey(x => x.Id);

        _ = modelBuilder.Entity<ServerMessage>()
            .Property(x => x.Id).HasDefaultValueSql();
    }
}
