using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Zajednica.BuildingBlocks.Tests;
using Zajednica.Chat.Infrastructure.Database;
using Zajednica.Community.Infrastructure.Database;
using Zajednica.Identity.Infrastructure.Database;

namespace Zajednica.Community.Tests;

public class CommunityTestFactory : BaseTestFactory<CommunityDbContext>
{
    protected override IEnumerable<Type> CollaboratingDbContexts => [typeof(ChatDbContext), typeof(IdentityDbContext)];

    protected override IServiceCollection ReplaceNeededDbContexts(IServiceCollection services)
    {
        Replace<CommunityDbContext>(services);
        Replace<ChatDbContext>(services);
        Replace<IdentityDbContext>(services);
        return services;
    }

    private static void Replace<TDbContext>(IServiceCollection services) where TDbContext : DbContext
    {
        var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<TDbContext>));
        if (descriptor is not null) services.Remove(descriptor);
        services.AddDbContext<TDbContext>(SetupTestContext(services));
    }
}
