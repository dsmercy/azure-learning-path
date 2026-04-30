namespace StorageApi.Models;

public class ProductRequest
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public required string Category { get; set; }
    public string? ImageUrl { get; set; }
}
