using Microsoft.Extensions.DependencyInjection;
using Zajednica.BuildingBlocks.Core.Notifications;

namespace Zajednica.BuildingBlocks.Infrastructure.Notifications;

public static class NotificationsInstaller
{
    public static IServiceCollection AddNotifications(this IServiceCollection services)
    {
        services.AddScoped<INotificationSender, LoggingNotificationSender>();
        return services;
    }
}