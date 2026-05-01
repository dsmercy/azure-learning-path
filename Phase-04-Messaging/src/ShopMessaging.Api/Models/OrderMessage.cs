namespace ShopMessaging.Api.Models;

/// <summary>Queue message body written to Service Bus orders-queue.</summary>
public sealed class OrderMessage
{
    public string OrderId { get; set; } = Guid.NewGuid().ToString();
    public required string CustomerId { get; set; }
    public required List<OrderLineItem> Items { get; set; }
    public required string ShippingAddress { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = "Pending";
    public DateTime PlacedAt { get; set; } = DateTime.UtcNow;
}
