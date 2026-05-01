using Microsoft.AspNetCore.Mvc;
using ShopMessaging.Api.Models;
using ShopMessaging.Api.Services;

namespace ShopMessaging.Api.Controllers;

[ApiController]
[Route("api/clickstream")]
[Produces("application/json")]
public sealed class ClickstreamController(IClickstreamService clickstreamService) : ControllerBase
{
    /// <summary>Record a product page view — emits a PageView event to Event Hubs storefront-clickstream.</summary>
    [HttpPost("pageview")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RecordPageView([FromBody] PageViewRequest req, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        await clickstreamService.SendPageViewAsync(req, ct);
        return Accepted(new { message = "PageView event sent to Event Hubs." });
    }

    /// <summary>Record an add-to-cart action — emits an AddToCart event to Event Hubs storefront-clickstream.</summary>
    [HttpPost("addtocart")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RecordAddToCart([FromBody] AddToCartRequest req, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        await clickstreamService.SendAddToCartAsync(req, ct);
        return Accepted(new { message = "AddToCart event sent to Event Hubs." });
    }
}
