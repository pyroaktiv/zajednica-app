namespace Zajednica.BuildingBlocks.Core.Storage;

public interface IFileUrlMapper
{
    string? ToUrl(string? key);

    string? ToKey(string? urlOrKey);
}
