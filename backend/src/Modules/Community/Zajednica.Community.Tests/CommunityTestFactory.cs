using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Zajednica.BuildingBlocks.Tests;
using Zajednica.Community.Infrastructure.Database;

namespace Zajednica.Community.Tests;

public class CommunityTestFactory : BaseTestFactory<CommunityDbContext>
{
    protected override IServiceCollection ReplaceNeededDbContexts(IServiceCollection services)
    {
        var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<CommunityDbContext>));
        if (descriptor is not null) services.Remove(descriptor);
        services.AddDbContext<CommunityDbContext>(SetupTestContext());
        return services;
    }
}
