using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Zajednica.BuildingBlocks.Tests;
using Zajednica.Feed.Infrastructure.Database;

namespace Zajednica.Feed.Tests;

public class FeedTestFactory : BaseTestFactory<FeedDbContext>
{
    protected override IServiceCollection ReplaceNeededDbContexts(IServiceCollection services)
    {
        var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<FeedDbContext>));
        if (descriptor is not null) services.Remove(descriptor);
        services.AddDbContext<FeedDbContext>(SetupTestContext());
        return services;
    }
}
