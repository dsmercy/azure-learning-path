using Microsoft.AspNetCore.Mvc;
using ShopMessaging.Api.Models;
using ShopMessaging.Api.Services;

namespace ShopMessaging.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public sealed class OrdersController(
    IOrderQueueService orderService,
    ILogger<OrdersController> logger) : ControllerBase
{
    /// <summary>Place an order — sends an OrderPlaced message to Service Bus orders-queue.</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PlaceOrder([FromBody] OrderRequest req, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var orderId = await orderService.PlaceOrderAsync(req, ct);

        logger.LogInformation("Order {OrderId} accepted via HTTP", orderId);

        return AcceptedAtAction(nameof(GetStatus), new { id = orderId }, new
        {
            orderId,
            status  = "Pending",
            message = "Order accepted and queued for processing. Watch the console for processor logs."
        });
    }

    /// <summary>Get order status — in a real app this queries the DB; here it shows the concept.</summary>
    [HttpGet("{id}/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetStatus(string id)
    {
        // Real app: query SQL DB for the row updated by OrderProcessorService
        return Ok(new
        {
            orderId = id,
            status  = "Processing",
            note    = "In a real app this reads from the database updated by the background processor."
        });
    }
}
