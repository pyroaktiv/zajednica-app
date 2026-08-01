using Microsoft.Extensions.Configuration;

namespace Zajednica.BuildingBlocks.Infrastructure.Storage;

public sealed class LocalFileUrlMapper : BaseFileUrlMapper
{
    public LocalFileUrlMapper(IConfiguration configuration)
    {
        var publicBaseUrl = (configuration["Storage:PublicBaseUrl"] ?? "").TrimEnd('/');
        Base = $"{publicBaseUrl}{LocalFileStorage.PublicPath}";
    }

    protected override string Base { get; }
}
