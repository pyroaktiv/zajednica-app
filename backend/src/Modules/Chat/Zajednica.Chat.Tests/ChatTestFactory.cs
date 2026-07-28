using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Zajednica.BuildingBlocks.Tests;
using Zajednica.Chat.Infrastructure.Database;
using Zajednica.Community.Infrastructure.Database;
using Zajednica.Feed.Infrastructure.Database;
using Zajednica.Identity.Infrastructure.Database;

namespace Zajednica.Chat.Tests;

public class ChatTestFactory : BaseTestFactory<ChatDbContext>
{
    protected override IEnumerable<Type> CollaboratingDbContexts =>
        [typeof(CommunityDbContext), typeof(FeedDbContext), typeof(IdentityDbContext)];

    protected override IServiceCollection ReplaceNeededDbContexts(IServiceCollection services)
    {
        Replace<ChatDbContext>(services);
        Replace<CommunityDbContext>(services);
        Replace<FeedDbContext>(services);
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
