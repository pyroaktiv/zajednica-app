using Microsoft.Extensions.Configuration;
using Zajednica.BuildingBlocks.Core.Storage;

namespace Zajednica.BuildingBlocks.Infrastructure.Storage;

public sealed class LocalFileStorage : IFileStorage
{
    public LocalFileStorage(IConfiguration configuration)
    {
        var path = configuration["Storage:LocalPath"];

        Root = Path.GetFullPath(string.IsNullOrWhiteSpace(path) ? "uploads" : path);
    }

    public string Root { get; }

    public string Save(Stream content, StoredFile file)
    {
        var target = PathFor(file.ObjectName);
        Directory.CreateDirectory(Path.GetDirectoryName(target)!);

        using var destination = File.Create(target);
        content.CopyTo(destination);

        return file.ObjectName;
    }

    public StoredFileContent? Open(string key)
    {
        var path = PathFor(key);
        return File.Exists(path)
            ? new StoredFileContent(File.OpenRead(path), FileKind.ContentTypeFor(key))
            : null;
    }

    public void Delete(string key)
    {
        var path = PathFor(key);
        if (File.Exists(path))
            File.Delete(path);
    }

    private string PathFor(string key) =>
        Path.Combine(Root, key.Replace('/', Path.DirectorySeparatorChar));
}
