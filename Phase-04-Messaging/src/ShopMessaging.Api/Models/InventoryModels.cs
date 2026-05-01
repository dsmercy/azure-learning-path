using System.ComponentModel.DataAnnotations;

namespace ShopMessaging.Api.Models;

public sealed class InventoryCheckRequest
{
    [Required] public required string ProductId { get; init; }
    [Required] public required string ProductName { get; init; }
    [Range(0, int.MaxValue)] public int CurrentStock { get; init; }
    [Range(1, 10000)] public int LowStockThreshold { get; init; } = 10;
}

/// <summary>Payload of the ShopApi.Inventory.StockLow Event Grid event.</summary>
public sealed class InventoryLowEvent
{
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int CurrentStock { get; set; }
    public int Threshold { get; set; }
    public DateTime DetectedAt { get; set; } = DateTime.UtcNow;
}
