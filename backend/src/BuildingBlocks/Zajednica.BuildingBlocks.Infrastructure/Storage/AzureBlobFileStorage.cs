using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;
using Zajednica.BuildingBlocks.Core.Storage;

namespace Zajednica.BuildingBlocks.Infrastructure.Storage;

public sealed class AzureBlobFileStorage : IFileStorage
{
    private readonly BlobContainerClient _container;

    public AzureBlobFileStorage(IConfiguration configuration)
    {
        var container = configuration["Storage:ContainerName"];

        _container = new BlobContainerClient(
            configuration["Storage:ConnectionString"],
            string.IsNullOrWhiteSpace(container) ? "uploads" : container);
        _container.CreateIfNotExists(PublicAccessType.None);
    }

    public string Save(Stream content, StoredFile file)
    {
        var blob = _container.GetBlobClient(file.ObjectName);
        blob.Upload(content, new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders { ContentType = file.ContentType }
        });

        return file.ObjectName;
    }

    public StoredFileContent? Open(string key)
    {
        var blob = _container.GetBlobClient(key);
        if (!blob.Exists())
            return null;

        var download = blob.DownloadStreaming().Value;
        return new StoredFileContent(download.Content, download.Details.ContentType);
    }

    public void Delete(string key) =>
        _container.GetBlobClient(key).DeleteIfExists();
}
