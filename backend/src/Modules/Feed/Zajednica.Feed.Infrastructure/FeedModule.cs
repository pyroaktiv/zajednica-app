using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Zajednica.Feed.Api.Public;
using Zajednica.Feed.Core.Domain.RepositoryInterfaces;
using Zajednica.Feed.Core.UseCases;
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
        AddApplicationServices(services);

        return services;
    }

    private static void AddPersistence(IServiceCollection services)
    {
        services.AddScoped<IPostRepository, PostEfRepository>();
        services.AddScoped<IIntentRepository, EventSourcedIntentRepository>();
    }

    private static void AddApplicationServices(IServiceCollection services)
    {
        services.AddScoped<CommunityAccess>();
        services.AddScoped<AuthorDirectory>();
        services.AddScoped<IPostService, PostService>();
        services.AddScoped<ICommentService, CommentService>();
        services.AddScoped<IIntentService, IntentService>();
    }
}
