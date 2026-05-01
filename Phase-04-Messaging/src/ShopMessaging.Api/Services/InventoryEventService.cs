using System.Text.Json;
using Azure.Messaging.EventGrid;
using ShopMessaging.Api.Models;

namespace ShopMessaging.Api.Services;

public sealed class InventoryEventService(
    EventGridPublisherClient client,
    ILogger<InventoryEventService> logger) : IInventoryEventService
{
    public async Task PublishLowStockEventAsync(InventoryCheckRequest request, CancellationToken ct = default)
    {
        var payload = new InventoryLowEvent
        {
            ProductId   = request.ProductId,
            ProductName = request.ProductName,
            CurrentStock = request.CurrentStock,
            Threshold   = request.LowStockThreshold,
            DetectedAt  = DateTime.UtcNow
        };

        var gridEvent = new EventGridEvent(
            subject:     $"products/{request.ProductId}",
            eventType:   "ShopApi.Inventory.StockLow",
            dataVersion: "1.0",
            data:        new BinaryData(JsonSerializer.Serialize(payload)));

        await client.SendEventAsync(gridEvent, ct);

        logger.LogInformation(
            "InventoryLow event published for product {ProductId} — stock {Stock} below threshold {Threshold}",
            request.ProductId, request.CurrentStock, request.LowStockThreshold);
    }
}
