using Microsoft.EntityFrameworkCore;
using Watcher.Database.Entities;

namespace Watcher.Database
{
    public interface IDatabaseContext : IDisposable
    {
        DbSet<ServerMessage> Messages { get; set; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}