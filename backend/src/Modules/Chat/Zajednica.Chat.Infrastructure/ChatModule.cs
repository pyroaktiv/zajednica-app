using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Zajednica.BuildingBlocks.Infrastructure.DomainEvents;
using Zajednica.Chat.Infrastructure.Database;

namespace Zajednica.Chat.Infrastructure;

public static class ChatModule
{
    public static IServiceCollection AddChatModule(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<ChatDbContext>((sp, o) =>
            o.UseNpgsql(connectionString, npg => npg.MigrationsHistoryTable("__EFMigrationsHistory", "chat"))
             .UseDomainEvents(sp));
        return services;
    }
}