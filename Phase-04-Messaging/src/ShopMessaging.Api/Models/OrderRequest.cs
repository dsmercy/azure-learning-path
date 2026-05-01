using System.ComponentModel.DataAnnotations;

namespace ShopMessaging.Api.Models;

public sealed class OrderRequest
{
    [Required]
    public required string CustomerId { get; init; }

    [Required, MinLength(1, ErrorMessage = "At least one item is required")]
    public required List<OrderLineItem> Items { get; init; }

    [Required, MaxLength(500)]
    public required string ShippingAddress { get; init; }
}

public sealed class OrderLineItem
{
    [Required] public required string ProductId { get; init; }
    [Required] public required string ProductName { get; init; }
    [Range(1, 100)] public int Quantity { get; init; } = 1;
    [Range(0.01, 99999.99)] public decimal UnitPrice { get; init; }
}
