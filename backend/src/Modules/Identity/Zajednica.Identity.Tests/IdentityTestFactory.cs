using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Zajednica.BuildingBlocks.Tests;
using Zajednica.Identity.Infrastructure.Database;

namespace Zajednica.Identity.Tests;

public class IdentityTestFactory : BaseTestFactory<IdentityDbContext>
{
    protected override IServiceCollection ReplaceNeededDbContexts(IServiceCollection services)
    {
        var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<IdentityDbContext>));
        if (descriptor is not null) services.Remove(descriptor);
        services.AddDbContext<IdentityDbContext>(SetupTestContext(services));
        return services;
    }
}
