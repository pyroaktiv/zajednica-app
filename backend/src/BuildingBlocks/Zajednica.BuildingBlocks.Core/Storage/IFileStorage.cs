namespace Zajednica.BuildingBlocks.Core.Storage;

public interface IFileStorage
{
    string Save(Stream content, StoredFile file);
    StoredFileContent? Open(string key);
    void Delete(string key);
}
