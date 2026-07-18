using Microsoft.EntityFrameworkCore;

namespace Zajednica.Feed.Infrastructure.Database;

public class FeedDbContext(DbContextOptions<FeedDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("feed");
        base.OnModelCreating(modelBuilder);
    }
}