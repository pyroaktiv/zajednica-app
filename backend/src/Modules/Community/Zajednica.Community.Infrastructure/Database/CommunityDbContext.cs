using Microsoft.EntityFrameworkCore;

namespace Zajednica.Community.Infrastructure.Database;

public class CommunityDbContext(DbContextOptions<CommunityDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("community");
        base.OnModelCreating(modelBuilder);
    }
}