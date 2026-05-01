using ShopMessaging.Api.Models;

namespace ShopMessaging.Api.Services;

public interface IOrderQueueService
{
    Task<string> PlaceOrderAsync(OrderRequest request, CancellationToken ct = default);
}
