using Microsoft.Extensions.Configuration;
using Zajednica.BuildingBlocks.Core.Storage;

namespace Zajednica.BuildingBlocks.Infrastructure.Storage;

public sealed class LocalFileStorage : IFileStorage
{
    public const string PublicPath = "/uploads";

    private readonly string _publicBaseUrl;

    public LocalFileStorage(IConfiguration configuration)
    {
        var path = configuration["Storage:LocalPath"];

        Root = Path.GetFullPath(string.IsNullOrWhiteSpace(path) ? "uploads" : path);
        _publicBaseUrl = (configuration["Storage:PublicBaseUrl"] ?? "").TrimEnd('/');
    }

    public string Root { get; }

    public string Save(Stream content, StoredFile file)
    {
        var target = Path.Combine(Root, file.ObjectName.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(target)!);

        using var destination = File.Create(target);
        content.CopyTo(destination);

        return $"{_publicBaseUrl}{PublicPath}/{file.ObjectName}";
    }
}
