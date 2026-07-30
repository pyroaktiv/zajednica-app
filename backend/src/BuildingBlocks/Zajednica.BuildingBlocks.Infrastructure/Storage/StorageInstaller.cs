using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
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
        if (app.ApplicationServices.GetRequiredService<IFileStorage>() is not LocalFileStorage local)
            return app;

        Directory.CreateDirectory(local.Root);

        var contentTypes = new FileExtensionContentTypeProvider();
        foreach (var kind in FileKind.All)
            foreach (var mapping in kind.ContentTypes)
                contentTypes.Mappings[mapping.Key] = mapping.Value;

        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(local.Root),
            RequestPath = LocalFileStorage.PublicPath,
            ContentTypeProvider = contentTypes,
            ServeUnknownFileTypes = false
        });

        return app;
    }
}
