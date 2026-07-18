using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Zajednica.BuildingBlocks.Infrastructure.DomainEvents;
using Zajednica.Identity.Infrastructure.Database;

namespace Zajednica.Identity.Infrastructure;

public static class IdentityModule
{
    public static IServiceCollection AddIdentityModule(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<IdentityDbContext>((sp, o) =>
            o.UseNpgsql(connectionString, npg => npg.MigrationsHistoryTable("__EFMigrationsHistory", "identity"))
             .UseDomainEvents(sp));
        return services;
    }
}