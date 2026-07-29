using Microsoft.Extensions.Options;
using Zajednica.BuildingBlocks.Core.Storage;

namespace Zajednica.BuildingBlocks.Infrastructure.Storage;

public sealed class LocalFileStorage : IFileStorage
{
    public const string PublicPath = "/uploads";

    private readonly string _publicBaseUrl;

    public LocalFileStorage(IOptions<StorageOptions> options)
    {
        var settings = options.Value;
        var path = string.IsNullOrWhiteSpace(settings.LocalPath) ? "uploads" : settings.LocalPath;

        Root = Path.GetFullPath(path);
        _publicBaseUrl = settings.PublicBaseUrl.TrimEnd('/');
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
