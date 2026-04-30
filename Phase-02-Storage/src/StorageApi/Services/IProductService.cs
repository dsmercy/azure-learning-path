using StorageApi.Models;

namespace StorageApi.Services;

public interface IProductService
{
    Task<IEnumerable<ProductResponse>> GetAllAsync(string? category = null, CancellationToken ct = default);
    Task<ProductResponse?> GetByIdAsync(string id, string category, CancellationToken ct = default);
    Task<ProductResponse> CreateAsync(ProductRequest request, CancellationToken ct = default);
    Task<ProductResponse?> UpdateAsync(string id, string category, ProductRequest request, CancellationToken ct = default);
    Task<bool> DeleteAsync(string id, string category, CancellationToken ct = default);
}
