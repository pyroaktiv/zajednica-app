using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Zajednica.BuildingBlocks.Infrastructure.Storage;

public sealed class LocalFileUrlMapper(IHttpContextAccessor httpContext, IConfiguration configuration)
    : BaseFileUrlMapper
{
    private readonly string _configuredBase = (configuration["Storage:PublicBaseUrl"] ?? "").TrimEnd('/');

    protected override string Base
    {
        get
        {
            var origin = string.IsNullOrEmpty(_configuredBase) ? RequestOrigin() : _configuredBase;
            return $"{origin}{LocalFileStorage.PublicPath}";
        }
    }

    private string RequestOrigin()
    {
        var request = httpContext.HttpContext?.Request;
        return request is null ? "" : $"{request.Scheme}://{request.Host}";
    }
}
