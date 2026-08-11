using Zajednica.BuildingBlocks.Core.Storage;

namespace Zajednica.BuildingBlocks.Infrastructure.Storage;

public abstract class BaseFileUrlMapper : IFileUrlMapper
{
    protected abstract string Base { get; }

    public string? ToUrl(string? key)
    {
        var trimmed = key?.Trim();
        if (string.IsNullOrEmpty(trimmed))
            return null;
        if (trimmed.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            || trimmed.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            return trimmed;

        return $"{Base}/{trimmed.TrimStart('/')}";
    }

    public string? ToKey(string? urlOrKey)
    {
        var trimmed = urlOrKey?.Trim();
        if (string.IsNullOrEmpty(trimmed))
            return null;

        var prefix = $"{Base}/";
        return trimmed.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
            ? trimmed[prefix.Length..]
            : trimmed;
    }
}
