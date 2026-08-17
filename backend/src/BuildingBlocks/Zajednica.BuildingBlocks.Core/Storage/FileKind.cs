using Zajednica.BuildingBlocks.Core.Exceptions;

namespace Zajednica.BuildingBlocks.Core.Storage;

public sealed class FileKind
{
    public const long ImageMaxBytes = 5L * 1024 * 1024;
    public const long AudioMaxBytes = 10L * 1024 * 1024;
    public const long DocumentMaxBytes = 20L * 1024 * 1024;

    public static readonly FileKind Image = new("image", "images", ImageMaxBytes,
        new Dictionary<string, string>
        {
            [".jpg"] = "image/jpeg",
            [".jpeg"] = "image/jpeg",
            [".png"] = "image/png",
            [".webp"] = "image/webp"
        });

    public static readonly FileKind Audio = new("audio", "audio", AudioMaxBytes,
        new Dictionary<string, string>
        {
            [".m4a"] = "audio/mp4",
            [".mp3"] = "audio/mpeg",
            [".ogg"] = "audio/ogg",
            [".wav"] = "audio/wav",
            [".webm"] = "audio/webm"
        });

    public static readonly FileKind Document = new("document", "documents", DocumentMaxBytes,
        new Dictionary<string, string>
        {
            [".pdf"] = "application/pdf"
        });

    public static IEnumerable<FileKind> All => [Image, Audio, Document];

    public static string ContentTypeFor(string key)
    {
        var extension = Path.GetExtension(key).ToLowerInvariant();
        foreach (var kind in All)
            if (kind.ContentTypes.TryGetValue(extension, out var contentType))
                return contentType;

        return "application/octet-stream";
    }

    public string Name { get; }
    public string Folder { get; }
    public long MaxSizeBytes { get; }
    public IReadOnlyDictionary<string, string> ContentTypes { get; }

    private FileKind(string name, string folder, long maxSizeBytes, IReadOnlyDictionary<string, string> contentTypes)
    {
        Name = name;
        Folder = folder;
        MaxSizeBytes = maxSizeBytes;
        ContentTypes = contentTypes;
    }

    public StoredFile AcceptUpload(string? fileName, long sizeBytes)
    {
        if (sizeBytes <= 0)
            throw new EntityValidationException("The uploaded file is empty.");
        if (sizeBytes > MaxSizeBytes)
            throw new EntityValidationException(
                $"An {Name} upload may not exceed {MaxSizeBytes / (1024 * 1024)} MB.");

        var extension = Path.GetExtension(fileName ?? "").ToLowerInvariant();
        if (!ContentTypes.TryGetValue(extension, out var contentType))
            throw new EntityValidationException(
                $"Unsupported {Name} format. Allowed: {string.Join(", ", ContentTypes.Keys)}.");

        return new StoredFile($"{Folder}/{Guid.NewGuid()}{extension}", contentType);
    }
}
