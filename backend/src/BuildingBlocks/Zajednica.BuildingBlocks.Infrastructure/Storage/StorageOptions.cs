namespace Zajednica.BuildingBlocks.Infrastructure.Storage;

public sealed class StorageOptions
{
    public const string SectionName = "Storage";

    public string Provider { get; set; } = "Local";
    public string LocalPath { get; set; } = "uploads";
    public string PublicBaseUrl { get; set; } = "";
    public string ConnectionString { get; set; } = "";
    public string ContainerName { get; set; } = "uploads";
}
