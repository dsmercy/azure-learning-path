using ShopMessaging.Api.Models;

namespace ShopMessaging.Api.Services;

public interface IInventoryEventService
{
    Task PublishLowStockEventAsync(InventoryCheckRequest request, CancellationToken ct = default);
}
