using Microsoft.EntityFrameworkCore;
using Zajednica.Feed.Core.Domain.Intents;
using Zajednica.Feed.Core.Domain.Intents.Events;
using Zajednica.Feed.Core.UseCases.Queries;
using Zajednica.Feed.Core.Domain.Posts;

namespace Zajednica.Feed.Infrastructure.Database;

public class FeedDbContext(DbContextOptions<FeedDbContext> options) : DbContext(options)
{
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<IntentEvent> IntentEvents => Set<IntentEvent>();
    public DbSet<IntentView> IntentViews => Set<IntentView>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("feed");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FeedDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
