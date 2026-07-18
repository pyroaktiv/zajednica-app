using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Zajednica.BuildingBlocks.Infrastructure.DomainEvents;
using Zajednica.Feed.Infrastructure.Database;

namespace Zajednica.Feed.Infrastructure;

public static class FeedModule
{
    public static IServiceCollection AddFeedModule(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<FeedDbContext>((sp, o) =>
            o.UseNpgsql(connectionString, npg => npg.MigrationsHistoryTable("__EFMigrationsHistory", "feed"))
             .UseDomainEvents(sp));
        return services;
    }
}