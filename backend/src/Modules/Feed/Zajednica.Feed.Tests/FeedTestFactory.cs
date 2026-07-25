using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Zajednica.BuildingBlocks.Tests;
using Zajednica.Community.Infrastructure.Database;
using Zajednica.Feed.Infrastructure.Database;
using Zajednica.Identity.Infrastructure.Database;

namespace Zajednica.Feed.Tests;

public class FeedTestFactory : BaseTestFactory<FeedDbContext>
{
    protected override IEnumerable<Type> CollaboratingDbContexts => [typeof(CommunityDbContext), typeof(IdentityDbContext)];

    protected override IServiceCollection ReplaceNeededDbContexts(IServiceCollection services)
    {
        Replace<FeedDbContext>(services);
        Replace<CommunityDbContext>(services);
        Replace<IdentityDbContext>(services);
        return services;
    }

    private static void Replace<TDbContext>(IServiceCollection services) where TDbContext : DbContext
    {
        var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<TDbContext>));
        if (descriptor is not null) services.Remove(descriptor);
        services.AddDbContext<TDbContext>(SetupTestContext());
    }
}
