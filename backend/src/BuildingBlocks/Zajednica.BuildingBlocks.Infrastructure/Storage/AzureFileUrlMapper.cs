using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;

namespace Zajednica.BuildingBlocks.Infrastructure.Storage;

public sealed class AzureFileUrlMapper : BaseFileUrlMapper
{
    public AzureFileUrlMapper(IConfiguration configuration)
    {
        var container = configuration["Storage:ContainerName"];

        var client = new BlobContainerClient(
            configuration["Storage:ConnectionString"],
            string.IsNullOrWhiteSpace(container) ? "uploads" : container);

        Base = client.Uri.ToString().TrimEnd('/');
    }

    protected override string Base { get; }
}
