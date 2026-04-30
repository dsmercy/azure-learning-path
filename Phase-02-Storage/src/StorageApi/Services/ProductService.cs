using Microsoft.Azure.Cosmos;
using StorageApi.Models;

namespace StorageApi.Services;

public class ProductService(CosmosClient cosmos, ILogger<ProductService> logger) : IProductService
{
    private readonly Container _container = cosmos.GetContainer("ProductCatalogDb", "Products");

    public async Task<IEnumerable<ProductResponse>> GetAllAsync(string? category = null, CancellationToken ct = default)
    {
        var query = category is null
            ? new QueryDefinition("SELECT * FROM c ORDER BY c.createdAt DESC")
            : new QueryDefinition("SELECT * FROM c WHERE c.category = @cat ORDER BY c.createdAt DESC")
                .WithParameter("@cat", category);

        var results = new List<ProductResponse>();
        using var iterator = _container.GetItemQueryIterator<ProductItem>(query);
        while (iterator.HasMoreResults)
        {
            var page = await iterator.ReadNextAsync(ct);
            results.AddRange(page.Select(MapToResponse));
        }
        return results;
    }

    public async Task<ProductResponse?> GetByIdAsync(string id, string category, CancellationToken ct = default)
    {
        try
        {
            var response = await _container.ReadItemAsync<ProductItem>(id, new PartitionKey(category), cancellationToken: ct);
            return MapToResponse(response.Resource);
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<ProductResponse> CreateAsync(ProductRequest request, CancellationToken ct = default)
    {
        var item = new ProductItem
        {
            Id = Guid.NewGuid().ToString(),
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            Category = request.Category,
            ImageUrl = request.ImageUrl,
            CreatedAt = DateTime.UtcNow
        };

        await _container.CreateItemAsync(item, new PartitionKey(item.Category), cancellationToken: ct);
        logger.LogInformation("Product {Id} created in category {Category}", item.Id, item.Category);
        return MapToResponse(item);
    }

    public async Task<ProductResponse?> UpdateAsync(string id, string category, ProductRequest request, CancellationToken ct = default)
    {
        try
        {
            var existing = await _container.ReadItemAsync<ProductItem>(id, new PartitionKey(category), cancellationToken: ct);
            var item = existing.Resource;

            item.Name = request.Name;
            item.Description = request.Description;
            item.Price = request.Price;
            item.Category = request.Category;
            item.ImageUrl = request.ImageUrl;
            item.UpdatedAt = DateTime.UtcNow;

            await _container.ReplaceItemAsync(item, id, new PartitionKey(category), cancellationToken: ct);
            logger.LogInformation("Product {Id} updated", id);
            return MapToResponse(item);
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<bool> DeleteAsync(string id, string category, CancellationToken ct = default)
    {
        try
        {
            await _container.DeleteItemAsync<ProductItem>(id, new PartitionKey(category), cancellationToken: ct);
            logger.LogInformation("Product {Id} deleted", id);
            return true;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return false;
        }
    }

    private static ProductResponse MapToResponse(ProductItem item) => new()
    {
        Id = item.Id,
        Name = item.Name,
        Description = item.Description,
        Price = item.Price,
        Category = item.Category,
        ImageUrl = item.ImageUrl,
        CreatedAt = item.CreatedAt,
        UpdatedAt = item.UpdatedAt
    };
}
