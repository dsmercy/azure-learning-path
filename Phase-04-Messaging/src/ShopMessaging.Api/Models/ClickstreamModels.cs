using System.ComponentModel.DataAnnotations;

namespace ShopMessaging.Api.Models;

// ── HTTP request DTOs ────────────────────────────────────────────

public sealed class PageViewRequest
{
    [Required] public required string SessionId { get; init; }
    public string CustomerId { get; init; } = "anonymous";
    [Required] public required string PageUrl { get; init; }
    public string ProductId { get; init; } = string.Empty;
}

public sealed class AddToCartRequest
{
    [Required] public required string SessionId { get; init; }
    public string CustomerId { get; init; } = "anonymous";
    [Required] public required string ProductId { get; init; }
    [Required] public required string ProductName { get; init; }
    [Range(1, 100)] public int Quantity { get; init; } = 1;
    [Range(0.01, 99999.99)] public decimal UnitPrice { get; init; }
}

// ── Event Hubs event payloads ────────────────────────────────────

public sealed class PageViewEvent
{
    public string EventType { get; set; } = "PageView";
    public string SessionId { get; set; } = string.Empty;
    public string CustomerId { get; set; } = "anonymous";
    public string PageUrl { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
}

public sealed class AddToCartEvent
{
    public string EventType { get; set; } = "AddToCart";
    public string SessionId { get; set; } = string.Empty;
    public string CustomerId { get; set; } = "anonymous";
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
}
