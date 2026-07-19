using Zajednica.BuildingBlocks.Infrastructure.DomainEvents;
using Zajednica.BuildingBlocks.Infrastructure.Notifications;
using Zajednica.Chat.Infrastructure;
using Zajednica.Community.Infrastructure;
using Zajednica.Feed.Infrastructure;
using Zajednica.Identity.Infrastructure;

namespace Zajednica.Api.Startup;

public static class ModulesConfiguration
{
    public static IServiceCollection RegisterModules(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Missing ConnectionStrings:DefaultConnection");

        // Cross-cutting infrastructure shared by every module.
        services.AddNotifications();
        services.AddDomainEvents();

        // One line per module. New module = one more call here.
        services.AddIdentityModule(connectionString, configuration);
        services.AddCommunityModule(connectionString);
        services.AddFeedModule(connectionString);
        services.AddChatModule(connectionString);

        return services;
    }
}
