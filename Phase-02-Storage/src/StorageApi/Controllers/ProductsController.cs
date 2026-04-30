using Microsoft.AspNetCore.Mvc;
using StorageApi.Models;
using StorageApi.Services;

namespace StorageApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController(IProductService productService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductResponse>>> GetAll(
        [FromQuery] string? category, CancellationToken ct)
    {
        var products = await productService.GetAllAsync(category, ct);
        return Ok(products);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductResponse>> GetById(
        string id, [FromQuery] string category, CancellationToken ct)
    {
        var product = await productService.GetByIdAsync(id, category, ct);
        return product is null ? NotFound() : Ok(product);
    }

    [HttpPost]
    public async Task<ActionResult<ProductResponse>> Create(ProductRequest request, CancellationToken ct)
    {
        var product = await productService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById),
            new { id = product.Id, category = product.Category }, product);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ProductResponse>> Update(
        string id, [FromQuery] string category, ProductRequest request, CancellationToken ct)
    {
        var product = await productService.UpdateAsync(id, category, request, ct);
        return product is null ? NotFound() : Ok(product);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, [FromQuery] string category, CancellationToken ct)
    {
        var deleted = await productService.DeleteAsync(id, category, ct);
        return deleted ? NoContent() : NotFound();
    }
}
