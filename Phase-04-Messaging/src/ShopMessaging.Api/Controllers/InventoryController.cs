using Microsoft.AspNetCore.Mvc;
using ShopMessaging.Api.Models;
using ShopMessaging.Api.Services;

namespace ShopMessaging.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public sealed class InventoryController(IInventoryEventService eventService) : ControllerBase
{
    /// <summary>
    /// Check stock level — if currentStock is at or below threshold, publishes a
    /// ShopApi.Inventory.StockLow event to the Event Grid topic.
    /// </summary>
    [HttpPost("check")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CheckStock([FromBody] InventoryCheckRequest req, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        if (req.CurrentStock <= req.LowStockThreshold)
        {
            await eventService.PublishLowStockEventAsync(req, ct);
            return Ok(new
            {
                productId     = req.ProductId,
                currentStock  = req.CurrentStock,
                threshold     = req.LowStockThreshold,
                eventPublished = true,
                message       = "InventoryLow event sent to Event Grid — subscribers will be notified."
            });
        }

        return Ok(new
        {
            productId     = req.ProductId,
            currentStock  = req.CurrentStock,
            threshold     = req.LowStockThreshold,
            eventPublished = false,
            message       = "Stock level is healthy — no event published."
        });
    }
}
