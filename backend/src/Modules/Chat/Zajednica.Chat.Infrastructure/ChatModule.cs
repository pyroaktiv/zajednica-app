using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Zajednica.Chat.Api.Internal;
using Zajednica.Chat.Api.Public;
using Zajednica.Chat.Core.Domain.RepositoryInterfaces;
using Zajednica.Chat.Core.UseCases;
using Zajednica.Chat.Core.UseCases.Chats;
using Zajednica.Chat.Core.UseCases.HelpChats;
using Zajednica.Chat.Core.UseCases.Internal;
using Zajednica.Chat.Core.UseCases.Messages;
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
        AddApplicationServices(services);

        return services;
    }

    private static void AddPersistence(IServiceCollection services)
    {
        services.AddScoped<IChatRepository, ChatEfRepository>();
    }

    private static void AddApplicationServices(IServiceCollection services)
    {
        services.AddScoped<MemberDirectory>();
        services.AddScoped<ChatRequirementsService>();
        services.AddScoped<ChatPresenterService>();
        services.AddScoped<ChatNotifier>();

        services.AddScoped<IChatService, ChatService>();
        services.AddScoped<IMessageService, MessageService>();
        services.AddScoped<IHelpRequestChatService, HelpRequestChatService>();
        services.AddScoped<IInternalChatService, InternalChatService>();
    }
}
