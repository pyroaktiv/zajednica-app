using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Zajednica.Feed.Infrastructure.Database;

namespace Zajednica.Feed.Infrastructure;

public static class FeedModule
{
    public static IServiceCollection AddFeedModule(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<FeedDbContext>(o =>
            o.UseNpgsql(connectionString, npg => npg.MigrationsHistoryTable("__EFMigrationsHistory", "feed")));
        return services;
    }
}