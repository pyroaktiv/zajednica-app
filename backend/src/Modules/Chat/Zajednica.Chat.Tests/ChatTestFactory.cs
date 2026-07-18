using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Zajednica.BuildingBlocks.Tests;
using Zajednica.Chat.Infrastructure.Database;

namespace Zajednica.Chat.Tests;

public class ChatTestFactory : BaseTestFactory<ChatDbContext>
{
    protected override IServiceCollection ReplaceNeededDbContexts(IServiceCollection services)
    {
        var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ChatDbContext>));
        if (descriptor is not null) services.Remove(descriptor);
        services.AddDbContext<ChatDbContext>(SetupTestContext());
        return services;
    }
}
