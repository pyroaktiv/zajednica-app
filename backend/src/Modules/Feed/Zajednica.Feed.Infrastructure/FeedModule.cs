using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Zajednica.Feed.Core.Domain.RepositoryInterfaces;
using Zajednica.Feed.Infrastructure.Database;
using Zajednica.Feed.Infrastructure.Database.Repositories;

namespace Zajednica.Feed.Infrastructure;

public static class FeedModule
{
    public static IServiceCollection AddFeedModule(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<FeedDbContext>(o =>
            o.UseNpgsql(connectionString, npg => npg.MigrationsHistoryTable("__EFMigrationsHistory", "feed")));

        AddPersistence(services);

        return services;
    }

    private static void AddPersistence(IServiceCollection services)
    {
        services.AddScoped<IPostRepository, PostEfRepository>();
        services.AddScoped<IIntentRepository, EventSourcedIntentRepository>();
    }
}
