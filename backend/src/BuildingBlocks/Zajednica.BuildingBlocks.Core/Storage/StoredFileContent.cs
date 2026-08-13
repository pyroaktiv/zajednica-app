namespace Zajednica.BuildingBlocks.Core.Storage;

public sealed record StoredFileContent(Stream Content, string ContentType) : IDisposable
{
    public void Dispose() => Content.Dispose();
}
