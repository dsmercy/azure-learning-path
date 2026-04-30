using StorageApi.Models;

namespace StorageApi.Services;

public interface IFileService
{
    Task<FileUploadResponse> UploadAsync(IFormFile file, CancellationToken ct = default);
    Task<string?> GetSasUrlAsync(string blobName, CancellationToken ct = default);
    Task<IEnumerable<string>> ListAsync(CancellationToken ct = default);
    Task<bool> DeleteAsync(string blobName, CancellationToken ct = default);
}
