using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Zajednica.BuildingBlocks.Core.Storage;

namespace Zajednica.BuildingBlocks.Infrastructure.Storage;

public static class StorageInstaller
{
    public static IServiceCollection AddStorage(this IServiceCollection services, IConfiguration configuration)
    {
        var provider = configuration["Storage:Provider"];
        if (string.Equals(provider, "Azure", StringComparison.OrdinalIgnoreCase))
            services.AddSingleton<IFileStorage, AzureBlobFileStorage>();
        else
            services.AddSingleton<IFileStorage, LocalFileStorage>();

        return services;
    }

    public static IApplicationBuilder UseLocalFiles(this IApplicationBuilder app)
    {
        if (app.ApplicationServices.GetRequiredService<IFileStorage>() is LocalFileStorage local)
            Directory.CreateDirectory(local.Root);

        return app;
    }
}
