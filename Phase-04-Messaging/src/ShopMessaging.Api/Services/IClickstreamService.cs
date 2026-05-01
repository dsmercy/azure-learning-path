using ShopMessaging.Api.Models;

namespace ShopMessaging.Api.Services;

public interface IClickstreamService
{
    Task SendPageViewAsync(PageViewRequest request, CancellationToken ct = default);
    Task SendAddToCartAsync(AddToCartRequest request, CancellationToken ct = default);
}
