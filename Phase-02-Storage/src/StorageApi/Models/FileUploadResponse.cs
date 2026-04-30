namespace StorageApi.Models;

public class FileUploadResponse
{
    public required string BlobName { get; set; }
    public required string SasUrl { get; set; }
    public long SizeBytes { get; set; }
    public required string ContentType { get; set; }
    public DateTime ExpiresAt { get; set; }
}
