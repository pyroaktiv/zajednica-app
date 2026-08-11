using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Zajednica.BuildingBlocks.Core.Realtime;

namespace Zajednica.BuildingBlocks.Infrastructure.Realtime;

public static class RealtimeInstaller
{
    public const string HubPath = "/hubs/realtime";

    public static IServiceCollection AddRealtime(this IServiceCollection services)
    {
        services.AddSignalR();
        services.AddSingleton<IUserIdProvider, AccountIdUserIdProvider>();
        services.AddScoped<IRealtimePusher, SignalRRealtimePusher>();
        return services;
    }

    public static IEndpointRouteBuilder MapRealtimeHub(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHub<RealtimeHub>(HubPath);
        return endpoints;
    }
}
