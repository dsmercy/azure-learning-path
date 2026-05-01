using System.Text;
using System.Text.Json;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using ShopMessaging.Api.Models;

namespace ShopMessaging.Api.Services;

public sealed class ClickstreamService(
    EventHubProducerClient client,
    ILogger<ClickstreamService> logger) : IClickstreamService
{
    public async Task SendPageViewAsync(PageViewRequest request, CancellationToken ct = default)
    {
        var evt = new PageViewEvent
        {
            SessionId  = request.SessionId,
            CustomerId = request.CustomerId,
            PageUrl    = request.PageUrl,
            ProductId  = request.ProductId,
            OccurredAt = DateTime.UtcNow
        };
        await SendEventAsync(evt, ct);
        logger.LogInformation("PageView event sent for session {SessionId} — url {Url}",
            request.SessionId, request.PageUrl);
    }

    public async Task SendAddToCartAsync(AddToCartRequest request, CancellationToken ct = default)
    {
        var evt = new AddToCartEvent
        {
            SessionId   = request.SessionId,
            CustomerId  = request.CustomerId,
            ProductId   = request.ProductId,
            ProductName = request.ProductName,
            Quantity    = request.Quantity,
            UnitPrice   = request.UnitPrice,
            OccurredAt  = DateTime.UtcNow
        };
        await SendEventAsync(evt, ct);
        logger.LogInformation("AddToCart event sent for session {SessionId} — product {ProductId} x{Qty}",
            request.SessionId, request.ProductId, request.Quantity);
    }

    private async Task SendEventAsync<T>(T payload, CancellationToken ct)
    {
        using var batch = await client.CreateBatchAsync(ct);
        var json = JsonSerializer.Serialize(payload);
        batch.TryAdd(new EventData(Encoding.UTF8.GetBytes(json)));
        await client.SendAsync(batch, ct);
    }
}
