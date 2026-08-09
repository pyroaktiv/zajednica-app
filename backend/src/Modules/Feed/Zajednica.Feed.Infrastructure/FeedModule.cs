using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Zajednica.Feed.Api.Internal;
using Zajednica.Feed.Api.Public;
using Zajednica.Feed.Core.Domain.RepositoryInterfaces;
using Zajednica.Feed.Core.UseCases;
using Zajednica.Feed.Core.UseCases.Comments;
using Zajednica.Feed.Core.UseCases.Intents;
using Zajednica.Feed.Core.UseCases.Internal;
using Zajednica.Feed.Core.UseCases.Posts;
using Zajednica.Feed.Core.UseCases.Queries;
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

        services.AddScoped<EventSourcedIntentRepository>();
        services.AddScoped<IIntentRepository>(sp => sp.GetRequiredService<EventSourcedIntentRepository>());
        services.AddScoped<IIntentQueryStore>(sp => sp.GetRequiredService<EventSourcedIntentRepository>());
    }

    private static void AddApplicationServices(IServiceCollection services)
    {
        services.AddScoped<MemberDirectory>();
        services.AddScoped<MemberRequirementsService>();

        services.AddScoped<IPostService, PostService>();
        services.AddScoped<ICommentService, CommentService>();
        services.AddScoped<IInternalHelpRequestService, InternalHelpRequestService>();

        services.AddScoped<IntentRetrievalService>();
        services.AddScoped<IntentNotifier>();
        services.AddScoped<IntentClosingService>();
        services.AddScoped<IntentPresenterService>();
        services.AddScoped<IIntentCommandService, IntentCommandService>();
        services.AddScoped<IIntentQueryService, IntentQueryService>();

        services.AddHostedService<IntentClosingWorker>();
    }
}
