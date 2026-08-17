namespace Zajednica.Feed.Core.UseCases;

public static class TextPreview
{
    public static string Truncate(string text, int maxLength)
    {
        var trimmed = text.Trim();
        if (trimmed.Length <= maxLength)
            return trimmed;

        return trimmed[..maxLength].TrimEnd() + "…";
    }
}
