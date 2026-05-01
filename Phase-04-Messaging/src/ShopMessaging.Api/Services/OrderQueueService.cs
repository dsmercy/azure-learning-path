using System.Text.Json;
using Azure.Messaging.ServiceBus;
using ShopMessaging.Api.Models;

namespace ShopMessaging.Api.Services;

public sealed class OrderQueueService(
    ServiceBusClient client,
    ILogger<OrderQueueService> logger) : IOrderQueueService
{
    private const string QueueName = "orders-queue";

    public async Task<string> PlaceOrderAsync(OrderRequest request, CancellationToken ct = default)
    {
        var order = new OrderMessage
        {
            OrderId = Guid.NewGuid().ToString(),
            CustomerId = request.CustomerId,
            Items = request.Items,
            ShippingAddress = request.ShippingAddress,
            TotalAmount = request.Items.Sum(i => i.Quantity * i.UnitPrice),
            Status = "Pending",
            PlacedAt = DateTime.UtcNow
        };

        await using var sender = client.CreateSender(QueueName);
        var message = new ServiceBusMessage(JsonSerializer.Serialize(order))
        {
            MessageId   = order.OrderId,
            ContentType = "application/json",
            Subject     = "OrderPlaced"
        };

        await sender.SendMessageAsync(message, ct);

        logger.LogInformation(
            "Order {OrderId} queued for customer {CustomerId} — {ItemCount} items, total {Total:C}",
            order.OrderId, order.CustomerId, order.Items.Count, order.TotalAmount);

        return order.OrderId;
    }
}
