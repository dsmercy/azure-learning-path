using Azure;
using Azure.Messaging.EventGrid;
using Azure.Messaging.EventHubs.Producer;
using Azure.Messaging.ServiceBus;
using ShopMessaging.Api.BackgroundServices;
using ShopMessaging.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
    c.SwaggerDoc("v1", new() { Title = "Shop Messaging API", Version = "v1",
        Description = "Phase 4 — Service Bus (orders), Event Grid (inventory alerts), Event Hubs (clickstream)" }));

// ── Azure SDK clients — all Singleton (thread-safe, manage connections) ──────

// Service Bus: reliable order queue (FIFO, dead-letter, retry)
builder.Services.AddSingleton(new ServiceBusClient(
    builder.Configuration.GetConnectionString("ServiceBus")!));

// Event Grid: push-based inventory-low event publishing
builder.Services.AddSingleton(new EventGridPublisherClient(
    new Uri(builder.Configuration["EventGrid:TopicUrl"]!),
    new AzureKeyCredential(builder.Configuration["EventGrid:Key"]!)));

// Event Hubs: high-throughput storefront clickstream producer
builder.Services.AddSingleton(new EventHubProducerClient(
    builder.Configuration.GetConnectionString("EventHubs")!,
    builder.Configuration["EventHubs:HubName"]!));

// ── Application services ──────────────────────────────────────────────────────

builder.Services.AddScoped<IOrderQueueService, OrderQueueService>();
builder.Services.AddScoped<IInventoryEventService, InventoryEventService>();
builder.Services.AddScoped<IClickstreamService, ClickstreamService>();

// Background processor: consumes from orders-queue and fulfils orders
builder.Services.AddHostedService<OrderProcessorService>();

builder.Services.AddHealthChecks()
    .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy());

// ── Pipeline ──────────────────────────────────────────────────────────────────

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Shop Messaging API v1");
    c.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();
app.MapControllers();
app.MapHealthChecks("/health");
app.MapGet("/", () => new
{
    service     = "ShopMessaging.Api",
    status      = "running",
    docs        = "/swagger",
    description = "Phase 4 — Service Bus, Event Grid, Event Hubs"
});

app.Run();
