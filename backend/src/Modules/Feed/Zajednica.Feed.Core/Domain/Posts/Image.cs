using Zajednica.BuildingBlocks.Core.Domain;
using Zajednica.BuildingBlocks.Core.Exceptions;

namespace Zajednica.Feed.Core.Domain.Posts;

public class Image : Entity
{
    public string Url { get; private set; } = null!;

    private Image() { }

    internal Image(string url)
    {
        var trimmed = url?.Trim();
        if (string.IsNullOrEmpty(trimmed))
            throw new EntityValidationException("Image url is required.");

        Url = trimmed;
    }
}
