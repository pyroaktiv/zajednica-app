using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Zajednica.BuildingBlocks.Infrastructure.DomainEvents;
using Zajednica.Community.Infrastructure.Database;

namespace Zajednica.Community.Infrastructure;

public static class CommunityModule
{
    public static IServiceCollection AddCommunityModule(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<CommunityDbContext>((sp, o) =>
            o.UseNpgsql(connectionString, npg => npg.MigrationsHistoryTable("__EFMigrationsHistory", "community"))
             .UseDomainEvents(sp));
        return services;
    }
}