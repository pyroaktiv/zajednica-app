using Microsoft.Extensions.DependencyInjection;
using Zajednica.BuildingBlocks.Core.Security;

namespace Zajednica.BuildingBlocks.Infrastructure.Security;

public static class SecurityInstaller
{
    public static IServiceCollection AddSecurity(this IServiceCollection services)
    {
        services.AddSingleton<ISecureTokenGenerator, SecureTokenGenerator>();
        return services;
    }
}
