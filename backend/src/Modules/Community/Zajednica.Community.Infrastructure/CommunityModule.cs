using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Zajednica.Community.Infrastructure.Database;

namespace Zajednica.Community.Infrastructure;

public static class CommunityModule
{
    public static IServiceCollection AddCommunityModule(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<CommunityDbContext>(o =>
            o.UseNpgsql(connectionString, npg => npg.MigrationsHistoryTable("__EFMigrationsHistory", "community")));
        return services;
    }
}