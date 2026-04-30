using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using StorageApi.Models;

namespace StorageApi.Services;

public class FileService(BlobServiceClient blobServiceClient, ILogger<FileService> logger) : IFileService
{
    private readonly BlobContainerClient _container = blobServiceClient.GetBlobContainerClient("uploads");

    public async Task<FileUploadResponse> UploadAsync(IFormFile file, CancellationToken ct = default)
    {
        var blobName = $"{Guid.NewGuid()}-{file.FileName}";
        var blobClient = _container.GetBlobClient(blobName);

        await using var stream = file.OpenReadStream();
        await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = file.ContentType }, cancellationToken: ct);

        var expiresAt = DateTime.UtcNow.AddHours(1);
        var sasUrl = GenerateSasUrl(blobClient, expiresAt);

        logger.LogInformation("Blob {BlobName} uploaded ({Size} bytes)", blobName, file.Length);

        return new FileUploadResponse
        {
            BlobName = blobName,
            SasUrl = sasUrl,
            SizeBytes = file.Length,
            ContentType = file.ContentType ?? "application/octet-stream",
            ExpiresAt = expiresAt
        };
    }

    public async Task<string?> GetSasUrlAsync(string blobName, CancellationToken ct = default)
    {
        var blobClient = _container.GetBlobClient(blobName);
        if (!await blobClient.ExistsAsync(ct)) return null;

        var expiresAt = DateTime.UtcNow.AddHours(1);
        return GenerateSasUrl(blobClient, expiresAt);
    }

    public async Task<IEnumerable<string>> ListAsync(CancellationToken ct = default)
    {
        var names = new List<string>();
        await foreach (var blob in _container.GetBlobsAsync(cancellationToken: ct))
            names.Add(blob.Name);
        return names;
    }

    public async Task<bool> DeleteAsync(string blobName, CancellationToken ct = default)
    {
        var blobClient = _container.GetBlobClient(blobName);
        var result = await blobClient.DeleteIfExistsAsync(cancellationToken: ct);
        if (result.Value) logger.LogInformation("Blob {BlobName} deleted", blobName);
        return result.Value;
    }

    private static string GenerateSasUrl(BlobClient blobClient, DateTime expiresAt)
    {
        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = blobClient.BlobContainerName,
            BlobName = blobClient.Name,
            Resource = "b",
            ExpiresOn = expiresAt
        };
        sasBuilder.SetPermissions(BlobSasPermissions.Read);
        return blobClient.GenerateSasUri(sasBuilder).ToString();
    }
}
