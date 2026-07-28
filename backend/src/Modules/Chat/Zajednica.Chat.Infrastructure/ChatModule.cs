using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Zajednica.Chat.Core.Domain.RepositoryInterfaces;
using Zajednica.Chat.Infrastructure.Database;
using Zajednica.Chat.Infrastructure.Database.Repositories;

namespace Zajednica.Chat.Infrastructure;

public static class ChatModule
{
    public static IServiceCollection AddChatModule(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<ChatDbContext>(o =>
            o.UseNpgsql(connectionString, npg => npg.MigrationsHistoryTable("__EFMigrationsHistory", "chat")));

        AddPersistence(services);

        return services;
    }

    private static void AddPersistence(IServiceCollection services)
    {
        services.AddScoped<IChatRepository, ChatEfRepository>();
    }
}
