using System.Text.Json;
using Azure.Messaging.ServiceBus;
using ShopMessaging.Api.Models;

namespace ShopMessaging.Api.BackgroundServices;

/// <summary>
/// Long-running hosted service that reads from orders-queue and processes each order.
/// Demonstrates the consumer side of Service Bus (the API controllers are the producer side).
/// </summary>
public sealed class OrderProcessorService : BackgroundService
{
    private readonly ServiceBusClient _client;
    private readonly ILogger<OrderProcessorService> _logger;
    private ServiceBusProcessor? _processor;

    public OrderProcessorService(ServiceBusClient client, ILogger<OrderProcessorService> logger)
    {
        _client = client;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _processor = _client.CreateProcessor("orders-queue", new ServiceBusProcessorOptions
        {
            MaxConcurrentCalls  = 2,
            AutoCompleteMessages = false
        });

        _processor.ProcessMessageAsync += HandleMessageAsync;
        _processor.ProcessErrorAsync   += HandleErrorAsync;

        await _processor.StartProcessingAsync(stoppingToken);
        _logger.LogInformation("Order processor started — listening on orders-queue");

        // Block until the host requests shutdown
        await Task.Delay(Timeout.Infinite, stoppingToken)
            .ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_processor is not null)
        {
            await _processor.StopProcessingAsync(cancellationToken);
            await _processor.DisposeAsync();
        }
        await base.StopAsync(cancellationToken);
        _logger.LogInformation("Order processor stopped");
    }

    private async Task HandleMessageAsync(ProcessMessageEventArgs args)
    {
        var order = JsonSerializer.Deserialize<OrderMessage>(
            args.Message.Body.ToString(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (order is null)
        {
            _logger.LogWarning("Dead-lettering unreadable message {MessageId}", args.Message.MessageId);
            await args.DeadLetterMessageAsync(args.Message, "InvalidPayload", "Could not deserialize OrderMessage");
            return;
        }

        _logger.LogInformation(
            "Processing order {OrderId} for customer {CustomerId} — {ItemCount} items, total {Total:C}",
            order.OrderId, order.CustomerId, order.Items.Count, order.TotalAmount);

        // Real app: update order status in DB, send confirmation email, reserve inventory
        await Task.Delay(200, args.CancellationToken); // simulate processing work

        _logger.LogInformation("Order {OrderId} fulfilled — status updated to Processing", order.OrderId);
        await args.CompleteMessageAsync(args.Message);
    }

    private Task HandleErrorAsync(ProcessErrorEventArgs args)
    {
        _logger.LogError(args.Exception,
            "Service Bus error — source: {Source}, namespace: {Namespace}",
            args.ErrorSource, args.FullyQualifiedNamespace);
        return Task.CompletedTask;
    }
}
